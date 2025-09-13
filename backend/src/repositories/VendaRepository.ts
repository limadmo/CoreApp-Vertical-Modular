/**
 * Venda Repository - Implementação concreta
 * Repository específico para operações com vendas
 */

import { PrismaClient, Venda } from '@prisma/client';
import { BaseRepository } from './BaseRepository';
import { TransactionClient } from './IUnitOfWork';

export class VendaRepository extends BaseRepository<Venda, string> {
  
  constructor(client: PrismaClient | TransactionClient) {
    super(client, {
      entityName: 'Venda',
      tableName: 'vendas',
      softDelete: false, // Vendas NUNCA são deletadas (compliance fiscal)
      auditable: true,
      cacheable: true,
      cacheKeyPrefix: 'venda',
      defaultTtl: 180 // 3 minutos
    });
  }

  /**
   * Obtém delegate do Prisma para Venda
   */
  protected getDelegate() {
    return this.client.venda;
  }

  /**
   * Busca venda por número fiscal
   */
  async findByNumeroFiscal(numeroFiscal: string, tenantId: string): Promise<Venda | null> {
    return await this.getDelegate().findFirst({
      where: { numeroFiscal, tenantId },
      include: {
        itens: {
          include: {
            produto: {
              select: {
                id: true,
                nome: true,
                codigoBarras: true,
                categoria: true
              }
            }
          }
        },
        cliente: {
          select: {
            id: true,
            nome: true,
            sobrenome: true,
            cpf: true
          }
        }
      }
    });
  }

  /**
   * Busca vendas com paginação e filtros avançados
   */
  async findWithFilters(filters: {
    dataInicio?: Date;
    dataFim?: Date;
    clienteId?: string;
    status?: string;
    formaPagamento?: string;
    valorMin?: number;
    valorMax?: number;
    skip: number;
    take: number;
    tenantId: string;
  }): Promise<{
    data: any[];
    total: number;
    hasNext: boolean;
    hasPrevious: boolean;
  }> {
    const where: any = { tenantId: filters.tenantId };

    if (filters.dataInicio || filters.dataFim) {
      where.dataVenda = {};
      if (filters.dataInicio) {
        where.dataVenda.gte = filters.dataInicio;
      }
      if (filters.dataFim) {
        where.dataVenda.lte = filters.dataFim;
      }
    }

    if (filters.clienteId) {
      where.clienteId = filters.clienteId;
    }

    if (filters.status) {
      where.status = filters.status;
    }

    if (filters.formaPagamento) {
      where.formaPagamento = filters.formaPagamento;
    }

    if (filters.valorMin !== undefined || filters.valorMax !== undefined) {
      where.total = {};
      if (filters.valorMin !== undefined) {
        where.total.gte = filters.valorMin;
      }
      if (filters.valorMax !== undefined) {
        where.total.lte = filters.valorMax;
      }
    }

    const [data, total] = await Promise.all([
      this.getDelegate().findMany({
        where,
        skip: filters.skip,
        take: filters.take,
        orderBy: { dataVenda: 'desc' },
        include: {
          cliente: {
            select: {
              nome: true,
              sobrenome: true,
              cpf: true
            }
          },
          itens: {
            select: {
              quantidade: true,
              precoUnitario: true,
              subtotal: true,
              produto: {
                select: {
                  nome: true,
                  categoria: true
                }
              }
            }
          },
          _count: {
            select: {
              itens: true
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
   * Busca vendas por período
   */
  async findByPeriod(
    dataInicio: Date,
    dataFim: Date,
    tenantId: string
  ): Promise<any[]> {
    return await this.getDelegate().findMany({
      where: {
        tenantId,
        dataVenda: {
          gte: dataInicio,
          lte: dataFim
        },
        status: 'FINALIZADA'
      },
      include: {
        cliente: {
          select: { nome: true, sobrenome: true }
        },
        itens: {
          include: {
            produto: {
              select: { nome: true, categoria: true }
            }
          }
        }
      },
      orderBy: { dataVenda: 'desc' }
    });
  }

  /**
   * Busca vendas de um cliente específico
   */
  async findByCliente(
    clienteId: string,
    tenantId: string,
    limit?: number
  ): Promise<any[]> {
    return await this.getDelegate().findMany({
      where: {
        clienteId,
        tenantId,
        status: 'FINALIZADA'
      },
      include: {
        itens: {
          include: {
            produto: {
              select: { nome: true, categoria: true }
            }
          }
        }
      },
      orderBy: { dataVenda: 'desc' },
      take: limit
    });
  }

  /**
   * Obtém vendas por forma de pagamento
   */
  async findByFormaPagamento(
    formaPagamento: string,
    tenantId: string,
    dataInicio?: Date,
    dataFim?: Date
  ): Promise<any[]> {
    const where: any = {
      tenantId,
      formaPagamento,
      status: 'FINALIZADA'
    };

    if (dataInicio || dataFim) {
      where.dataVenda = {};
      if (dataInicio) where.dataVenda.gte = dataInicio;
      if (dataFim) where.dataVenda.lte = dataFim;
    }

    return await this.getDelegate().findMany({
      where,
      include: {
        cliente: {
          select: { nome: true, sobrenome: true }
        }
      },
      orderBy: { dataVenda: 'desc' }
    });
  }

  /**
   * Cancela uma venda (apenas muda status)
   */
  async cancelSale(
    vendaId: string,
    tenantId: string,
    userId: string,
    motivo: string
  ): Promise<Venda | null> {
    const venda = await this.findById(vendaId, tenantId);
    if (!venda || venda.status !== 'FINALIZADA') {
      return null;
    }

    return await this.getDelegate().update({
      where: { id: vendaId },
      data: {
        status: 'CANCELADA',
        dataEstorno: new Date(),
        motivoEstorno: motivo,
        usuarioEstorno: userId,
        dataUltimaAtualizacao: new Date(),
        usuarioUltimaAtualizacao: userId
      }
    });
  }

  /**
   * Obtém relatório de vendas por período
   */
  async getSalesReport(
    dataInicio: Date,
    dataFim: Date,
    tenantId: string
  ): Promise<{
    totalVendas: number;
    totalFaturamento: number;
    ticketMedio: number;
    vendasPorDia: any[];
    vendasPorFormaPagamento: any[];
    vendasPorCategoria: any[];
    topProdutos: any[];
  }> {
    const vendas = await this.findByPeriod(dataInicio, dataFim, tenantId);

    const totalVendas = vendas.length;
    const totalFaturamento = vendas.reduce((sum, v) => sum + Number(v.total), 0);
    const ticketMedio = totalVendas > 0 ? totalFaturamento / totalVendas : 0;

    // Vendas por dia
    const vendasPorDia = vendas.reduce((acc, venda) => {
      const dia = venda.dataVenda.toISOString().split('T')[0];
      if (!acc[dia]) {
        acc[dia] = { dia, vendas: 0, faturamento: 0 };
      }
      acc[dia].vendas++;
      acc[dia].faturamento += Number(venda.total);
      return acc;
    }, {} as any);

    // Vendas por forma de pagamento
    const vendasPorFormaPagamento = vendas.reduce((acc, venda) => {
      const forma = venda.formaPagamento;
      if (!acc[forma]) {
        acc[forma] = { forma, vendas: 0, valor: 0 };
      }
      acc[forma].vendas++;
      acc[forma].valor += Number(venda.total);
      return acc;
    }, {} as any);

    // Vendas por categoria
    const vendasPorCategoria = vendas.reduce((acc, venda) => {
      venda.itens.forEach((item: any) => {
        const categoria = item.produto.categoria || 'Sem categoria';
        if (!acc[categoria]) {
          acc[categoria] = { categoria, quantidade: 0, valor: 0 };
        }
        acc[categoria].quantidade += item.quantidade;
        acc[categoria].valor += Number(item.subtotal);
      });
      return acc;
    }, {} as any);

    // Top produtos
    const produtos = vendas.reduce((acc, venda) => {
      venda.itens.forEach((item: any) => {
        const produto = item.produto.nome;
        if (!acc[produto]) {
          acc[produto] = { produto, quantidade: 0, valor: 0 };
        }
        acc[produto].quantidade += item.quantidade;
        acc[produto].valor += Number(item.subtotal);
      });
      return acc;
    }, {} as any);

    const topProdutos = Object.values(produtos)
      .sort((a: any, b: any) => b.quantidade - a.quantidade)
      .slice(0, 10);

    return {
      totalVendas,
      totalFaturamento,
      ticketMedio,
      vendasPorDia: Object.values(vendasPorDia),
      vendasPorFormaPagamento: Object.values(vendasPorFormaPagamento),
      vendasPorCategoria: Object.values(vendasPorCategoria),
      topProdutos
    };
  }

  /**
   * Obtém estatísticas das vendas
   */
  async getStatistics(tenantId: string): Promise<{
    totalHoje: number;
    faturamentoHoje: number;
    totalMes: number;
    faturamentoMes: number;
    totalCanceladas: number;
    ticketMedioMes: number;
  }> {
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    
    const inicioMes = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
    const fimDia = new Date(hoje);
    fimDia.setHours(23, 59, 59, 999);

    const [vendasHoje, vendasMes, canceladas] = await Promise.all([
      this.getDelegate().aggregate({
        where: {
          tenantId,
          dataVenda: { gte: hoje, lte: fimDia },
          status: 'FINALIZADA'
        },
        _count: { id: true },
        _sum: { total: true }
      }),
      this.getDelegate().aggregate({
        where: {
          tenantId,
          dataVenda: { gte: inicioMes },
          status: 'FINALIZADA'
        },
        _count: { id: true },
        _sum: { total: true }
      }),
      this.getDelegate().count({
        where: {
          tenantId,
          dataVenda: { gte: inicioMes },
          status: 'CANCELADA'
        }
      })
    ]);

    const faturamentoHoje = Number(vendasHoje._sum.total) || 0;
    const faturamentoMes = Number(vendasMes._sum.total) || 0;
    const totalMes = vendasMes._count.id;
    const ticketMedioMes = totalMes > 0 ? faturamentoMes / totalMes : 0;

    return {
      totalHoje: vendasHoje._count.id,
      faturamentoHoje,
      totalMes,
      faturamentoMes,
      totalCanceladas: canceladas,
      ticketMedioMes
    };
  }

  /**
   * Gera próximo número fiscal
   */
  async getNextNumeroFiscal(tenantId: string): Promise<string> {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    
    const prefix = `${year}${month}`;
    
    const lastVenda = await this.getDelegate().findFirst({
      where: {
        tenantId,
        numeroFiscal: { startsWith: prefix }
      },
      orderBy: { numeroFiscal: 'desc' }
    });

    let nextNumber = 1;
    if (lastVenda) {
      const lastNumber = parseInt(lastVenda.numeroFiscal.slice(-6));
      nextNumber = lastNumber + 1;
    }

    return `${prefix}${String(nextNumber).padStart(6, '0')}`;
  }
}