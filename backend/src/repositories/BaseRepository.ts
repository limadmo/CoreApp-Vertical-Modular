/**
 * Implementação Base Repository - Prisma ORM
 * Repository genérico com todas as operações CRUD + Multi-tenant
 */

import { PrismaClient } from '@prisma/client';
import { IBaseRepository } from './IBaseRepository';
import { TransactionClient } from './IUnitOfWork';
import { cacheService } from '../services/cacheService';

/**
 * Configuração base para operações do repository
 */
interface RepositoryConfig {
  entityName: string;
  tableName: string;
  softDelete: boolean;
  auditable: boolean;
  cacheable: boolean;
  cacheKeyPrefix: string;
  defaultTtl: number;
}

/**
 * Classe base abstrata para todos os repositories
 * Implementa IBaseRepository com Prisma ORM
 */
export abstract class BaseRepository<TEntity, TKey = string> 
  implements IBaseRepository<TEntity, TKey> {
  
  protected readonly config: RepositoryConfig;
  protected readonly client: PrismaClient | TransactionClient;

  constructor(
    client: PrismaClient | TransactionClient,
    config: RepositoryConfig
  ) {
    this.client = client;
    this.config = config;
  }

  // ====================================
  // QUERY OPERATIONS (Read)
  // ====================================

  async findById(id: TKey, tenantId: string): Promise<TEntity | null> {
    const cacheKey = this.getCacheKey('findById', id, tenantId);
    
    if (this.config.cacheable) {
      const cached = cacheService.get<TEntity>(cacheKey);
      if (cached) return cached;
    }

    const entity = await this.getDelegate().findFirst({
      where: this.buildBaseWhere({ id, tenantId })
    });

    if (entity && this.config.cacheable) {
      cacheService.set(cacheKey, entity, this.config.defaultTtl);
    }

    return entity as TEntity | null;
  }

  async findFirst(criteria: Partial<TEntity>, tenantId: string): Promise<TEntity | null> {
    const entity = await this.getDelegate().findFirst({
      where: this.buildBaseWhere({ ...criteria, tenantId })
    });

    return entity as TEntity | null;
  }

  async findMany(criteria?: Partial<TEntity>, tenantId?: string): Promise<TEntity[]> {
    const cacheKey = this.getCacheKey('findMany', criteria, tenantId);
    
    if (this.config.cacheable && tenantId) {
      const cached = cacheService.get<TEntity[]>(cacheKey);
      if (cached) return cached;
    }

    const where = criteria || tenantId ? this.buildBaseWhere({ ...criteria, tenantId }) : {};
    const entities = await this.getDelegate().findMany({ where });

    if (this.config.cacheable && tenantId) {
      cacheService.set(cacheKey, entities, this.config.defaultTtl);
    }

    return entities as TEntity[];
  }

  async findPaginated(
    skip: number, 
    take: number, 
    criteria?: Partial<TEntity>, 
    tenantId?: string
  ): Promise<{
    data: TEntity[];
    total: number;
    hasNext: boolean;
    hasPrevious: boolean;
  }> {
    const where = criteria || tenantId ? this.buildBaseWhere({ ...criteria, tenantId }) : {};

    const [data, total] = await Promise.all([
      this.getDelegate().findMany({
        where,
        skip,
        take,
        orderBy: { dataCadastro: 'desc' }
      }),
      this.getDelegate().count({ where })
    ]);

    return {
      data: data as TEntity[],
      total,
      hasNext: skip + take < total,
      hasPrevious: skip > 0
    };
  }

  async count(criteria?: Partial<TEntity>, tenantId?: string): Promise<number> {
    const where = criteria || tenantId ? this.buildBaseWhere({ ...criteria, tenantId }) : {};
    return await this.getDelegate().count({ where });
  }

  async exists(criteria: Partial<TEntity>, tenantId: string): Promise<boolean> {
    const count = await this.getDelegate().count({
      where: this.buildBaseWhere({ ...criteria, tenantId }),
      take: 1
    });
    return count > 0;
  }

  // ====================================
  // COMMAND OPERATIONS (Write)
  // ====================================

  async add(entity: Omit<TEntity, 'id'>, tenantId: string, userId: string): Promise<TEntity> {
    const dataToInsert = {
      ...entity,
      tenantId,
      usuarioCadastro: userId,
      dataCadastro: new Date(),
      dataUltimaAtualizacao: new Date(),
      usuarioUltimaAtualizacao: userId,
      ativo: true
    };

    const created = await this.getDelegate().create({
      data: dataToInsert
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      this.invalidateTenantCache(tenantId);
    }

    // Log de auditoria
    if (this.config.auditable) {
      await this.logAudit(created.id, 'CREATE', dataToInsert, userId, tenantId);
    }

    return created as TEntity;
  }

  async addMany(entities: Omit<TEntity, 'id'>[], tenantId: string, userId: string): Promise<number> {
    const dataToInsert = entities.map(entity => ({
      ...entity,
      tenantId,
      usuarioCadastro: userId,
      dataCadastro: new Date(),
      dataUltimaAtualizacao: new Date(),
      usuarioUltimaAtualizacao: userId,
      ativo: true
    }));

    const result = await this.getDelegate().createMany({
      data: dataToInsert
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      this.invalidateTenantCache(tenantId);
    }

    return result.count;
  }

  async update(
    id: TKey, 
    updates: Partial<TEntity>, 
    tenantId: string, 
    userId: string
  ): Promise<TEntity | null> {
    const existing = await this.findById(id, tenantId);
    if (!existing) return null;

    const dataToUpdate = {
      ...updates,
      dataUltimaAtualizacao: new Date(),
      usuarioUltimaAtualizacao: userId
    };

    const updated = await this.getDelegate().update({
      where: { id, tenantId },
      data: dataToUpdate
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      this.invalidateEntityCache(id, tenantId);
      this.invalidateTenantCache(tenantId);
    }

    // Log de auditoria
    if (this.config.auditable) {
      await this.logAudit(id as string, 'UPDATE', dataToUpdate, userId, tenantId);
    }

    return updated as TEntity;
  }

  async updateMany(
    criteria: Partial<TEntity>, 
    updates: Partial<TEntity>, 
    tenantId: string, 
    userId: string
  ): Promise<number> {
    const dataToUpdate = {
      ...updates,
      dataUltimaAtualizacao: new Date(),
      usuarioUltimaAtualizacao: userId
    };

    const result = await this.getDelegate().updateMany({
      where: this.buildBaseWhere({ ...criteria, tenantId }),
      data: dataToUpdate
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      this.invalidateTenantCache(tenantId);
    }

    return result.count;
  }

  async remove(id: TKey, tenantId: string, userId: string, reason?: string): Promise<boolean> {
    if (!this.config.softDelete) {
      throw new Error(`Hard delete not supported for ${this.config.entityName}. Use hardDelete() method.`);
    }

    const existing = await this.findById(id, tenantId);
    if (!existing) return false;

    await this.getDelegate().update({
      where: { id, tenantId },
      data: {
        ativo: false,
        dataDelecao: new Date(),
        usuarioDelecao: userId,
        motivoDelecao: reason,
        dataUltimaAtualizacao: new Date(),
        usuarioUltimaAtualizacao: userId
      }
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      this.invalidateEntityCache(id, tenantId);
      this.invalidateTenantCache(tenantId);
    }

    // Log de auditoria
    if (this.config.auditable) {
      await this.logAudit(id as string, 'DELETE', { reason }, userId, tenantId);
    }

    return true;
  }

  async removeMany(ids: TKey[], tenantId: string, userId: string, reason?: string): Promise<number> {
    if (!this.config.softDelete) {
      throw new Error(`Hard delete not supported for ${this.config.entityName}. Use hardDelete() method.`);
    }

    const result = await this.getDelegate().updateMany({
      where: {
        id: { in: ids },
        tenantId,
        ativo: true
      },
      data: {
        ativo: false,
        dataDelecao: new Date(),
        usuarioDelecao: userId,
        motivoDelecao: reason,
        dataUltimaAtualizacao: new Date(),
        usuarioUltimaAtualizacao: userId
      }
    });

    // Invalida cache relacionado
    if (this.config.cacheable) {
      ids.forEach(id => this.invalidateEntityCache(id, tenantId));
      this.invalidateTenantCache(tenantId);
    }

    return result.count;
  }

  async hardDelete(id: TKey, tenantId: string): Promise<boolean> {
    try {
      await this.getDelegate().delete({
        where: { id, tenantId }
      });

      // Invalida cache relacionado
      if (this.config.cacheable) {
        this.invalidateEntityCache(id, tenantId);
        this.invalidateTenantCache(tenantId);
      }

      return true;
    } catch (error) {
      return false;
    }
  }

  // ====================================
  // ADVANCED OPERATIONS
  // ====================================

  async executeInTransaction<TResult>(
    operation: (repository: this) => Promise<TResult>
  ): Promise<TResult> {
    return await (this.client as PrismaClient).$transaction(async (tx) => {
      const txRepository = new (this.constructor as any)(tx, this.config);
      return await operation(txRepository);
    });
  }

  async executeCustomQuery<TResult>(query: string, parameters?: any[]): Promise<TResult> {
    return await (this.client as any).$queryRaw(query, ...(parameters || []));
  }

  async findAdvanced(options: {
    where?: any;
    include?: any;
    orderBy?: any;
    skip?: number;
    take?: number;
    tenantId?: string;
  }): Promise<TEntity[]> {
    const where = options.tenantId 
      ? this.buildBaseWhere({ ...options.where, tenantId: options.tenantId })
      : options.where;

    const entities = await this.getDelegate().findMany({
      where,
      include: options.include,
      orderBy: options.orderBy,
      skip: options.skip,
      take: options.take
    });

    return entities as TEntity[];
  }

  // ====================================
  // AUDIT & METADATA
  // ====================================

  async getAuditHistory(id: TKey, tenantId: string): Promise<any[]> {
    if (!this.config.auditable) {
      return [];
    }

    // Implementação específica de auditoria seria aqui
    // Por simplicidade, retornando array vazio
    return [];
  }

  async restore(id: TKey, tenantId: string, userId: string): Promise<boolean> {
    if (!this.config.softDelete) {
      return false;
    }

    try {
      await this.getDelegate().update({
        where: { id, tenantId },
        data: {
          ativo: true,
          dataDelecao: null,
          usuarioDelecao: null,
          motivoDelecao: null,
          dataUltimaAtualizacao: new Date(),
          usuarioUltimaAtualizacao: userId
        }
      });

      // Invalida cache relacionado
      if (this.config.cacheable) {
        this.invalidateEntityCache(id, tenantId);
        this.invalidateTenantCache(tenantId);
      }

      return true;
    } catch (error) {
      return false;
    }
  }

  async getMetadata(id: TKey, tenantId: string): Promise<any | null> {
    const entity = await this.getDelegate().findFirst({
      where: { id, tenantId },
      select: {
        dataCadastro: true,
        dataUltimaAtualizacao: true,
        usuarioCadastro: true,
        usuarioUltimaAtualizacao: true,
        ativo: true,
        dataDelecao: true,
        usuarioDelecao: true
      }
    });

    if (!entity) return null;

    return {
      createdAt: entity.dataCadastro,
      updatedAt: entity.dataUltimaAtualizacao,
      createdBy: entity.usuarioCadastro,
      updatedBy: entity.usuarioUltimaAtualizacao,
      isDeleted: !entity.ativo,
      deletedAt: entity.dataDelecao,
      deletedBy: entity.usuarioDelecao,
      version: 1 // Implementar versionamento se necessário
    };
  }

  // ====================================
  // HELPER METHODS
  // ====================================

  /**
   * Obtém o delegate específico do Prisma para esta entidade
   * Deve ser implementado pelas classes filhas
   */
  protected abstract getDelegate(): any;

  /**
   * Constrói WHERE clause base com tenant e soft delete
   */
  protected buildBaseWhere(criteria: any): any {
    const where: any = { ...criteria };

    if (this.config.softDelete && !criteria.includeDeleted) {
      where.ativo = true;
    }

    return where;
  }

  /**
   * Gera chave de cache
   */
  private getCacheKey(operation: string, ...params: any[]): string {
    const paramsString = params.map(p => 
      typeof p === 'object' ? JSON.stringify(p) : String(p)
    ).join(':');
    
    return `${this.config.cacheKeyPrefix}:${operation}:${paramsString}`;
  }

  /**
   * Invalida cache de uma entidade específica
   */
  private invalidateEntityCache(id: TKey, tenantId: string): void {
    const patterns = [
      `${this.config.cacheKeyPrefix}:findById:${id}:${tenantId}`,
      `${this.config.cacheKeyPrefix}:*:${tenantId}`
    ];
    
    cacheService.scheduleInvalidateCache(patterns.join('|'));
  }

  /**
   * Invalida cache de todas as entidades do tenant
   */
  private invalidateTenantCache(tenantId: string): void {
    cacheService.scheduleInvalidateCache(`${this.config.cacheKeyPrefix}:*:${tenantId}`);
  }

  /**
   * Log de auditoria (implementação simplificada)
   */
  private async logAudit(
    entityId: string,
    action: string,
    changes: any,
    userId: string,
    tenantId: string
  ): Promise<void> {
    // Implementação de auditoria seria aqui
    // Por simplicidade, apenas log no console em desenvolvimento
    if (process.env.NODE_ENV === 'development') {
      console.log(`[AUDIT] ${this.config.entityName}:${entityId} ${action} by ${userId} in ${tenantId}`);
    }
  }
}