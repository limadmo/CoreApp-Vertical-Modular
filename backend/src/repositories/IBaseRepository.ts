/**
 * Interface Base Repository - Padrão Repository Genérico
 * Abstração completa para operações CRUD com multi-tenant
 */

export interface IBaseRepository<TEntity, TKey = string> {
  // ====================================
  // QUERY OPERATIONS (Read)
  // ====================================
  
  /**
   * Busca entidade por ID
   * @param id - ID da entidade
   * @param tenantId - ID do tenant para isolamento
   * @returns Entidade ou null se não encontrada
   */
  findById(id: TKey, tenantId: string): Promise<TEntity | null>;

  /**
   * Busca primeira entidade que satisfaz critério
   * @param criteria - Critério de busca
   * @param tenantId - ID do tenant
   * @returns Primeira entidade encontrada ou null
   */
  findFirst(criteria: Partial<TEntity>, tenantId: string): Promise<TEntity | null>;

  /**
   * Busca todas as entidades que satisfazem critério
   * @param criteria - Critério de busca (opcional)
   * @param tenantId - ID do tenant
   * @returns Lista de entidades
   */
  findMany(criteria?: Partial<TEntity>, tenantId?: string): Promise<TEntity[]>;

  /**
   * Busca com paginação
   * @param skip - Número de registros a pular
   * @param take - Número de registros a retornar
   * @param criteria - Critério de busca (opcional)
   * @param tenantId - ID do tenant
   * @returns Lista paginada de entidades
   */
  findPaginated(
    skip: number, 
    take: number, 
    criteria?: Partial<TEntity>, 
    tenantId?: string
  ): Promise<{
    data: TEntity[];
    total: number;
    hasNext: boolean;
    hasPrevious: boolean;
  }>;

  /**
   * Conta total de registros que satisfazem critério
   * @param criteria - Critério de busca (opcional)
   * @param tenantId - ID do tenant
   * @returns Número total de registros
   */
  count(criteria?: Partial<TEntity>, tenantId?: string): Promise<number>;

  /**
   * Verifica se existe entidade que satisfaz critério
   * @param criteria - Critério de busca
   * @param tenantId - ID do tenant
   * @returns Boolean indicando existência
   */
  exists(criteria: Partial<TEntity>, tenantId: string): Promise<boolean>;

  // ====================================
  // COMMAND OPERATIONS (Write)
  // ====================================

  /**
   * Adiciona nova entidade
   * @param entity - Dados da entidade
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está criando
   * @returns Entidade criada
   */
  add(entity: Omit<TEntity, 'id'>, tenantId: string, userId: string): Promise<TEntity>;

  /**
   * Adiciona múltiplas entidades
   * @param entities - Array de entidades
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está criando
   * @returns Número de entidades criadas
   */
  addMany(entities: Omit<TEntity, 'id'>[], tenantId: string, userId: string): Promise<number>;

  /**
   * Atualiza entidade existente
   * @param id - ID da entidade
   * @param updates - Campos a serem atualizados
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está atualizando
   * @returns Entidade atualizada ou null se não encontrada
   */
  update(
    id: TKey, 
    updates: Partial<TEntity>, 
    tenantId: string, 
    userId: string
  ): Promise<TEntity | null>;

  /**
   * Atualiza múltiplas entidades que satisfazem critério
   * @param criteria - Critério de busca
   * @param updates - Campos a serem atualizados
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está atualizando
   * @returns Número de entidades atualizadas
   */
  updateMany(
    criteria: Partial<TEntity>, 
    updates: Partial<TEntity>, 
    tenantId: string, 
    userId: string
  ): Promise<number>;

  /**
   * Remove entidade (soft delete)
   * @param id - ID da entidade
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está removendo
   * @param reason - Motivo da remoção (opcional)
   * @returns Boolean indicando sucesso
   */
  remove(id: TKey, tenantId: string, userId: string, reason?: string): Promise<boolean>;

  /**
   * Remove múltiplas entidades (soft delete)
   * @param ids - Array de IDs
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está removendo
   * @param reason - Motivo da remoção (opcional)
   * @returns Número de entidades removidas
   */
  removeMany(ids: TKey[], tenantId: string, userId: string, reason?: string): Promise<number>;

  /**
   * Remove entidade permanentemente (hard delete)
   * CUIDADO: Esta operação é irreversível
   * @param id - ID da entidade
   * @param tenantId - ID do tenant
   * @returns Boolean indicando sucesso
   */
  hardDelete(id: TKey, tenantId: string): Promise<boolean>;

  // ====================================
  // ADVANCED OPERATIONS
  // ====================================

  /**
   * Executa operação em transação
   * @param operation - Operação a ser executada
   * @returns Resultado da operação
   */
  executeInTransaction<TResult>(
    operation: (repository: this) => Promise<TResult>
  ): Promise<TResult>;

  /**
   * Executa query personalizada
   * @param query - Query customizada (específica do ORM)
   * @param parameters - Parâmetros da query
   * @returns Resultado da query
   */
  executeCustomQuery<TResult>(query: string, parameters?: any[]): Promise<TResult>;

  /**
   * Busca com critérios complexos
   * @param options - Opções de busca avançada
   * @returns Lista de entidades
   */
  findAdvanced(options: {
    where?: any;
    include?: any;
    orderBy?: any;
    skip?: number;
    take?: number;
    tenantId?: string;
  }): Promise<TEntity[]>;

  // ====================================
  // AUDIT & METADATA
  // ====================================

  /**
   * Obtém histórico de mudanças de uma entidade
   * @param id - ID da entidade
   * @param tenantId - ID do tenant
   * @returns Histórico de mudanças
   */
  getAuditHistory(id: TKey, tenantId: string): Promise<{
    id: string;
    entityId: string;
    action: 'CREATE' | 'UPDATE' | 'DELETE';
    changes: any;
    userId: string;
    timestamp: Date;
  }[]>;

  /**
   * Restaura entidade soft deleted
   * @param id - ID da entidade
   * @param tenantId - ID do tenant
   * @param userId - ID do usuário que está restaurando
   * @returns Boolean indicando sucesso
   */
  restore(id: TKey, tenantId: string, userId: string): Promise<boolean>;

  /**
   * Obtém metadados da entidade
   * @param id - ID da entidade
   * @param tenantId - ID do tenant
   * @returns Metadados (criação, modificação, etc.)
   */
  getMetadata(id: TKey, tenantId: string): Promise<{
    createdAt: Date;
    updatedAt: Date;
    createdBy: string;
    updatedBy: string;
    isDeleted: boolean;
    deletedAt?: Date;
    deletedBy?: string;
    version: number;
  } | null>;
}