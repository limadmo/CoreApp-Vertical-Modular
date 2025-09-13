/**
 * Produto Repository - Implementação concreta
 * Repository específico para operações com produtos
 */

import { PrismaClient, Produto } from '@prisma/client';
import { BaseRepository } from './BaseRepository';
import { TransactionClient } from './IUnitOfWork';

export class ProdutoRepository extends BaseRepository<Produto, string> {
  
  constructor(client: PrismaClient | TransactionClient) {
    super(client, {
      entityName: 'Produto',
      tableName: 'produtos',
      softDelete: true,
      auditable: true,
      cacheable: true,
      cacheKeyPrefix: 'produto',
      defaultTtl: 300 // 5 minutos
    });
  }

  /**
   * Obtém delegate do Prisma para Produto
   */
  protected getDelegate() {
    return this.client.produto;
  }

  /**
   * Busca produto por código de barras
   */
  async findByCodigoBarras(codigoBarras: string, tenantId: string): Promise<Produto | null> {
    return await this.getDelegate().findFirst({
      where: this.buildBaseWhere({ codigoBarras, tenantId })
    });
  }

  /**
   * Busca produtos com paginação e filtros avançados
   */
  async findWithFilters(filters: {
    search?: string;
    categoria?: string;
    precoMin?: number;
    precoMax?: number;
    ativo?: boolean;
    skip: number;
    take: number;
    tenantId: string;
  }): Promise<{
    data: Produto[];
    total: number;
    hasNext: boolean;
    hasPrevious: boolean;
  }> {
    const where: any = {
      tenantId: filters.tenantId,
      ativo: filters.ativo ?? true
    };

    if (filters.search) {
      where.OR = [
        { nome: { contains: filters.search, mode: 'insensitive' } },
        { codigoBarras: { contains: filters.search, mode: 'insensitive' } },
        { categoria: { contains: filters.search, mode: 'insensitive' } }
      ];
    }

    if (filters.categoria) {
      where.categoria = { contains: filters.categoria, mode: 'insensitive' };
    }

    if (filters.precoMin !== undefined || filters.precoMax !== undefined) {
      where.precoVenda = {};
      if (filters.precoMin !== undefined) {
        where.precoVenda.gte = filters.precoMin;
      }
      if (filters.precoMax !== undefined) {
        where.precoVenda.lte = filters.precoMax;
      }
    }

    const [data, total] = await Promise.all([
      this.getDelegate().findMany({
        where,
        skip: filters.skip,
        take: filters.take,
        orderBy: { nome: 'asc' },
        include: {
          estoque: {
            select: {
              quantidadeAtual: true,
              quantidadeMinima: true,
              dataValidade: true
            }
          }
        }
      }),
      this.getDelegate().count({ where })
    ]);

    return {
      data,
      total,
      hasNext: filters.skip + filters.take < total,
      hasPrevious: filters.skip > 0
    };
  }

  /**
   * Busca produtos por categoria
   */
  async findByCategoria(categoria: string, tenantId: string): Promise<Produto[]> {
    return await this.getDelegate().findMany({
      where: this.buildBaseWhere({
        categoria: { contains: categoria, mode: 'insensitive' },
        tenantId
      }),
      orderBy: { nome: 'asc' },
      include: {
        estoque: {
          select: {
            quantidadeAtual: true,
            quantidadeMinima: true
          }
        }
      }
    });
  }

  /**
   * Busca produtos com estoque baixo
   */
  async findWithLowStock(tenantId: string): Promise<any[]> {
    return await this.getDelegate().findMany({
      where: this.buildBaseWhere({ tenantId }),
      include: {
        estoque: {
          where: {
            ativo: true,
            OR: [
              { quantidadeAtual: { lte: { quantidadeMinima: true } } },
              { quantidadeAtual: 0 }
            ]
          }
        }
      },
      orderBy: { nome: 'asc' }
    });
  }

  /**
   * Busca produtos próximos ao vencimento (para padaria)
   */
  async findNearExpiration(
    daysAhead: number,
    tenantId: string
  ): Promise<any[]> {
    const targetDate = new Date();
    targetDate.setDate(targetDate.getDate() + daysAhead);

    return await this.getDelegate().findMany({
      where: this.buildBaseWhere({
        tenantId,
        validadeHoras: { not: null }
      }),
      include: {
        estoque: {
          where: {
            ativo: true,
            dataValidade: {
              lte: targetDate
            }
          }
        }
      },
      orderBy: { nome: 'asc' }
    });
  }

  /**
   * Busca produtos por faixa de preço
   */
  async findByPriceRange(
    minPrice: number,
    maxPrice: number,
    tenantId: string
  ): Promise<Produto[]> {
    return await this.getDelegate().findMany({
      where: this.buildBaseWhere({
        tenantId,
        precoVenda: {
          gte: minPrice,
          lte: maxPrice
        }
      }),
      orderBy: { precoVenda: 'asc' }
    });
  }

  /**
   * Obtém categorias únicas
   */
  async getUniqueCategories(tenantId: string): Promise<string[]> {
    const result = await this.getDelegate().findMany({
      where: this.buildBaseWhere({ tenantId }),
      select: { categoria: true },
      distinct: ['categoria']
    });

    return result
      .map(r => r.categoria)
      .filter((cat): cat is string => cat !== null)
      .sort();
  }

  /**
   * Obtém estatísticas dos produtos
   */
  async getStatistics(tenantId: string): Promise<{
    total: number;
    ativos: number;
    inativos: number;
    categorias: number;
    estoqueBaixo: number;
    semEstoque: number;
    precoMedio: number;
  }> {
    const [
      total,
      ativos,
      inativos,
      categorias,
      estoqueInfo,
      precoMedio
    ] = await Promise.all([
      this.count({}, tenantId),
      this.count({ ativo: true }, tenantId),
      this.count({ ativo: false }, tenantId),
      this.getUniqueCategories(tenantId),
      this.getDelegate().findMany({
        where: this.buildBaseWhere({ tenantId }),
        include: {
          estoque: {
            select: {
              quantidadeAtual: true,
              quantidadeMinima: true
            }
          }
        }
      }),
      this.getDelegate().aggregate({
        where: this.buildBaseWhere({ tenantId }),
        _avg: { precoVenda: true }
      })
    ]);

    const estoqueBaixo = estoqueInfo.filter(p => 
      p.estoque && p.estoque.quantidadeAtual <= p.estoque.quantidadeMinima
    ).length;

    const semEstoque = estoqueInfo.filter(p => 
      p.estoque && p.estoque.quantidadeAtual === 0
    ).length;

    return {
      total,
      ativos,
      inativos,
      categorias: categorias.length,
      estoqueBaixo,
      semEstoque,
      precoMedio: Number(precoMedio._avg.precoVenda) || 0
    };
  }

  /**
   * Valida se código de barras já existe (excluindo um ID específico)
   */
  async codigoBarrasExists(
    codigoBarras: string, 
    tenantId: string, 
    excludeId?: string
  ): Promise<boolean> {
    const where: any = {
      codigoBarras,
      tenantId,
      ativo: true
    };

    if (excludeId) {
      where.NOT = { id: excludeId };
    }

    const count = await this.getDelegate().count({ where });
    return count > 0;
  }

  /**
   * Atualiza preços em lote por categoria
   */
  async updatePricesByCategory(
    categoria: string,
    percentualAumento: number,
    tenantId: string,
    userId: string
  ): Promise<number> {
    return await this.updateMany(
      { categoria, tenantId },
      { 
        precoVenda: {
          multiply: (1 + percentualAumento / 100)
        }
      },
      tenantId,
      userId
    );
  }
}