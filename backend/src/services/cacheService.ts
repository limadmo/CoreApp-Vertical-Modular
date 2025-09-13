/**
 * Serviço de Cache - Resiliente com Fallback
 * Cache com estratégia: 2→5→7→10→12→15→20→30min, desabilita vendas após 30min
 */

import NodeCache from 'node-cache';

export interface CacheConfig {
  enabled: boolean;
  defaultTtl: number; // Tempo em segundos
  fallbackTtl: number[];
  maxFallbackLevel: number;
  disableSalesAfterMinutes: number;
}

export interface CacheStatus {
  enabled: boolean;
  fallbackLevel: number;
  lastSuccessfulUpdate: Date | null;
  salesDisabled: boolean;
  currentTtl: number;
}

/**
 * Configuração de cache resiliente
 */
const CACHE_CONFIG: CacheConfig = {
  enabled: process.env.NODE_ENV === 'production', // Desabilitado em desenvolvimento
  defaultTtl: 120, // 2 minutos padrão
  fallbackTtl: [120, 300, 420, 600, 720, 900, 1200, 1800], // 2→5→7→10→12→15→20→30min
  maxFallbackLevel: 7, // Máximo 30 minutos
  disableSalesAfterMinutes: 30
};

class CacheService {
  private cache: NodeCache;
  private status: CacheStatus;
  private connectionRetryCount: number = 0;

  constructor() {
    this.cache = new NodeCache({
      stdTTL: CACHE_CONFIG.defaultTtl,
      checkperiod: 60, // Verificar expiração a cada 60 segundos
      useClones: false // Melhor performance, cuidado com mutações
    });

    this.status = {
      enabled: CACHE_CONFIG.enabled,
      fallbackLevel: 0,
      lastSuccessfulUpdate: null,
      salesDisabled: false,
      currentTtl: CACHE_CONFIG.defaultTtl
    };

    // Logs para desenvolvimento
    if (process.env.NODE_ENV === 'development') {
      console.log(`[CACHE] Inicializado - Enabled: ${this.status.enabled}, TTL: ${this.status.currentTtl}s`);
    }
  }

  /**
   * Obtém valor do cache
   * @param key - Chave do cache
   * @returns Valor do cache ou undefined
   */
  get<T>(key: string): T | undefined {
    if (!this.status.enabled) {
      return undefined;
    }

    try {
      const value = this.cache.get<T>(key);
      
      if (value !== undefined) {
        // Sucesso - resetar fallback se necessário
        if (this.status.fallbackLevel > 0) {
          this.resetToNormalOperation();
        }
      }

      return value;
    } catch (error) {
      console.error(`[CACHE] Erro ao obter ${key}:`, error);
      return undefined;
    }
  }

  /**
   * Define valor no cache
   * @param key - Chave do cache
   * @param value - Valor a ser armazenado
   * @param customTtl - TTL customizado (opcional)
   * @returns Boolean indicando sucesso
   */
  set<T>(key: string, value: T, customTtl?: number): boolean {
    if (!this.status.enabled) {
      return false;
    }

    try {
      const ttl = customTtl || this.status.currentTtl;
      const success = this.cache.set(key, value, ttl);
      
      if (success) {
        this.status.lastSuccessfulUpdate = new Date();
        
        // Sucesso - resetar fallback se necessário
        if (this.status.fallbackLevel > 0) {
          this.resetToNormalOperation();
        }
      } else {
        this.handleCacheFailure();
      }

      return success;
    } catch (error) {
      console.error(`[CACHE] Erro ao definir ${key}:`, error);
      this.handleCacheFailure();
      return false;
    }
  }

  /**
   * Remove item do cache
   * @param key - Chave a ser removida
   * @returns Boolean indicando sucesso
   */
  delete(key: string): boolean {
    if (!this.status.enabled) {
      return false;
    }

    try {
      return this.cache.del(key) > 0;
    } catch (error) {
      console.error(`[CACHE] Erro ao deletar ${key}:`, error);
      return false;
    }
  }

  /**
   * Limpa todo o cache
   */
  flush(): void {
    if (!this.status.enabled) {
      return;
    }

    try {
      this.cache.flushAll();
      console.log('[CACHE] Cache limpo completamente');
    } catch (error) {
      console.error('[CACHE] Erro ao limpar cache:', error);
    }
  }

  /**
   * Obtém estatísticas do cache
   */
  getStats(): {
    keys: number;
    hits: number;
    misses: number;
    status: CacheStatus;
    ttlBreakdown: { [key: string]: number };
  } {
    const stats = this.cache.getStats();
    const keys = this.cache.keys();
    const ttlBreakdown: { [key: string]: number } = {};

    // Analisar TTLs dos itens
    keys.forEach(key => {
      const ttl = this.cache.getTtl(key);
      if (ttl) {
        const remainingSeconds = Math.ceil((ttl - Date.now()) / 1000);
        ttlBreakdown[key] = remainingSeconds;
      }
    });

    return {
      keys: stats.keys,
      hits: stats.hits,
      misses: stats.misses,
      status: { ...this.status },
      ttlBreakdown
    };
  }

  /**
   * Gerencia falha no cache - aplica estratégia de fallback
   */
  private handleCacheFailure(): void {
    this.connectionRetryCount++;

    // Aplicar próximo nível de fallback
    if (this.status.fallbackLevel < CACHE_CONFIG.maxFallbackLevel) {
      this.status.fallbackLevel++;
      this.status.currentTtl = CACHE_CONFIG.fallbackTtl[this.status.fallbackLevel];
      
      console.warn(`[CACHE] Falha detectada - Fallback nível ${this.status.fallbackLevel} (TTL: ${this.status.currentTtl / 60}min)`);
    }

    // Verificar se deve desabilitar vendas
    this.checkSalesDisabling();
  }

  /**
   * Reseta para operação normal após sucesso
   */
  private resetToNormalOperation(): void {
    const previousLevel = this.status.fallbackLevel;
    
    this.status.fallbackLevel = 0;
    this.status.currentTtl = CACHE_CONFIG.defaultTtl;
    this.connectionRetryCount = 0;
    this.status.salesDisabled = false;

    if (previousLevel > 0) {
      console.log(`[CACHE] Recuperado do fallback nível ${previousLevel} - Operação normal restaurada`);
    }
  }

  /**
   * Verifica se deve desabilitar vendas baseado no tempo sem cache
   */
  private checkSalesDisabling(): void {
    if (!this.status.lastSuccessfulUpdate) {
      return;
    }

    const minutesSinceLastSuccess = Math.floor(
      (Date.now() - this.status.lastSuccessfulUpdate.getTime()) / (1000 * 60)
    );

    if (minutesSinceLastSuccess >= CACHE_CONFIG.disableSalesAfterMinutes) {
      if (!this.status.salesDisabled) {
        this.status.salesDisabled = true;
        console.error(`[CACHE] VENDAS DESABILITADAS - Sem cache por ${minutesSinceLastSuccess} minutos`);
      }
    }
  }

  /**
   * Verifica se as vendas estão habilitadas
   */
  isSalesEnabled(): boolean {
    this.checkSalesDisabling();
    return !this.status.salesDisabled;
  }

  /**
   * Força reabilitação das vendas (use com cuidado)
   */
  forceEnableSales(): void {
    this.status.salesDisabled = false;
    this.status.lastSuccessfulUpdate = new Date();
    console.warn('[CACHE] VENDAS FORÇADAMENTE REABILITADAS');
  }

  /**
   * Obtém ou define com callback para geração
   * @param key - Chave do cache
   * @param generator - Função que gera o valor se não existir no cache
   * @param customTtl - TTL customizado
   * @returns Valor do cache ou gerado
   */
  async getOrSet<T>(
    key: string, 
    generator: () => Promise<T> | T, 
    customTtl?: number
  ): Promise<T> {
    const cached = this.get<T>(key);
    
    if (cached !== undefined) {
      return cached;
    }

    try {
      const value = await generator();
      this.set(key, value, customTtl);
      return value;
    } catch (error) {
      console.error(`[CACHE] Erro no generator para ${key}:`, error);
      throw error;
    }
  }

  /**
   * Cache específico para estoque com TTL adaptativo
   */
  setEstoque(tenantId: string, produtoId: string, estoque: any): boolean {
    const key = `estoque:${tenantId}:${produtoId}`;
    
    // TTL mais longo para dados críticos como estoque
    const ttl = this.status.currentTtl * 2;
    
    return this.set(key, estoque, ttl);
  }

  /**
   * Obtém estoque do cache
   */
  getEstoque(tenantId: string, produtoId: string): any | undefined {
    const key = `estoque:${tenantId}:${produtoId}`;
    return this.get(key);
  }

  /**
   * Cache para produtos com invalidação por categoria
   */
  setProdutos(tenantId: string, produtos: any[]): boolean {
    const key = `produtos:${tenantId}`;
    return this.set(key, produtos);
  }

  /**
   * Obtém produtos do cache
   */
  getProdutos(tenantId: string): any[] | undefined {
    const key = `produtos:${tenantId}`;
    return this.get(key);
  }

  /**
   * Invalida cache relacionado a um tenant
   */
  invalidateTenant(tenantId: string): void {
    const keys = this.cache.keys();
    const tenantKeys = keys.filter(key => key.includes(tenantId));
    
    tenantKeys.forEach(key => this.delete(key));
    
    console.log(`[CACHE] Invalidado cache do tenant ${tenantId} (${tenantKeys.length} chaves)`);
  }
}

// Singleton para o serviço de cache
export const cacheService = new CacheService();

// Middleware para verificar se vendas estão habilitadas
export function checkSalesEnabled(req: any, res: any, next: any) {
  if (!cacheService.isSalesEnabled()) {
    return res.status(503).json({
      error: 'Vendas temporariamente desabilitadas devido a problemas de cache',
      code: 'SALES_DISABLED_CACHE_FAILURE',
      retry: true,
      retryAfter: 60 // segundos
    });
  }
  
  next();
}