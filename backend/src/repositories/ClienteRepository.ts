/**
 * Cliente Repository - Implementação concreta
 * Repository específico para operações com clientes
 */

import { PrismaClient, Cliente } from '@prisma/client';
import { BaseRepository } from './BaseRepository';
import { TransactionClient } from './IUnitOfWork';

export class ClienteRepository extends BaseRepository<Cliente, string> {
  
  constructor(client: PrismaClient | TransactionClient) {
    super(client, {
      entityName: 'Cliente',
      tableName: 'clientes',
      softDelete: true,
      auditable: true,
      cacheable: true,
      cacheKeyPrefix: 'cliente',
      defaultTtl: 300 // 5 minutos
    });
  }

  /**
   * Obtém delegate do Prisma para Cliente
   */
  protected getDelegate() {
    return this.client.cliente;
  }

  /**
   * Busca cliente por CPF
   */
  async findByCpf(cpf: string, tenantId: string): Promise<Cliente | null> {
    return await this.getDelegate().findFirst({
      where: this.buildBaseWhere({ cpf, tenantId })
    });
  }

  /**
   * Busca clientes com paginação e filtros avançados
   */
  async findWithFilters(filters: {
    search?: string;
    ativo?: boolean;
    skip: number;
    take: number;
    tenantId: string;
  }): Promise<{
    data: Cliente[];
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
        { sobrenome: { contains: filters.search, mode: 'insensitive' } },
        { cpf: { contains: filters.search, mode: 'insensitive' } }
      ];
    }

    const [data, total] = await Promise.all([
      this.getDelegate().findMany({
        where,
        skip: filters.skip,
        take: filters.take,
        orderBy: { nome: 'asc' }
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
   * Obtém clientes por faixa etária
   */
  async findByAgeRange(
    minAge: number, 
    maxAge: number, 
    tenantId: string
  ): Promise<Cliente[]> {
    const now = new Date();
    const maxDate = new Date(now.getFullYear() - minAge, now.getMonth(), now.getDate());
    const minDate = new Date(now.getFullYear() - maxAge, now.getMonth(), now.getDate());

    return await this.getDelegate().findMany({
      where: this.buildBaseWhere({
        tenantId,
        dataNascimento: {
          gte: minDate,
          lte: maxDate
        }
      }),
      orderBy: { nome: 'asc' }
    });
  }

  /**
   * Obtém estatísticas dos clientes
   */
  async getStatistics(tenantId: string): Promise<{
    total: number;
    ativos: number;
    inativos: number;
    comCpf: number;
    semCpf: number;
    novosMes: number;
  }> {
    const now = new Date();
    const inicioMes = new Date(now.getFullYear(), now.getMonth(), 1);

    const [
      total,
      ativos, 
      inativos,
      comCpf,
      semCpf,
      novosMes
    ] = await Promise.all([
      this.count({}, tenantId),
      this.count({ ativo: true }, tenantId),
      this.count({ ativo: false }, tenantId),
      this.getDelegate().count({
        where: {
          tenantId,
          cpf: { not: null },
          ativo: true
        }
      }),
      this.getDelegate().count({
        where: {
          tenantId,
          cpf: null,
          ativo: true
        }
      }),
      this.getDelegate().count({
        where: {
          tenantId,
          dataCadastro: { gte: inicioMes },
          ativo: true
        }
      })
    ]);

    return {
      total,
      ativos,
      inativos,
      comCpf,
      semCpf,
      novosMes
    };
  }

  /**
   * Valida se CPF já existe (excluindo um ID específico)
   */
  async cpfExists(cpf: string, tenantId: string, excludeId?: string): Promise<boolean> {
    const where: any = {
      cpf,
      tenantId,
      ativo: true
    };

    if (excludeId) {
      where.NOT = { id: excludeId };
    }

    const count = await this.getDelegate().count({ where });
    return count > 0;
  }
}