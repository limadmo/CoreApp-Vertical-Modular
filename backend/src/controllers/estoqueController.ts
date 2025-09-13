import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { TenantRequest } from '../middleware/tenant';
import { createError } from '../middleware/errorHandler';

const prisma = new PrismaClient();

// Listar estoque com paginação
export const listarEstoque = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      page = 1, 
      limit = 20, 
      search,
      ativo = 'true',
      estoqueMinimo = 'false'
    } = req.query;
    
    const pageNum = parseInt(page as string, 10);
    const limitNum = parseInt(limit as string, 10);
    const skip = (pageNum - 1) * limitNum;
    const isAtivo = ativo === 'true';
    const filtrarEstoqueMinimo = estoqueMinimo === 'true';
    
    // Filtros de busca
    const where: any = {
      tenantId: req.tenantId,
      ativo: isAtivo,
      produto: {
        ativo: true
      }
    };
    
    // Filtro para produtos com estoque abaixo do mínimo
    if (filtrarEstoqueMinimo) {
      where.quantidadeAtual = {
        lte: prisma.estoque.fields.quantidadeMinima
      };
    }
    
    if (search) {
      where.produto = {
        ...where.produto,
        OR: [
          { nome: { contains: search as string, mode: 'insensitive' } },
          { codigoBarras: { contains: search as string, mode: 'insensitive' } },
          { lote: { contains: search as string, mode: 'insensitive' } }
        ]
      };
    }
    
    // Buscar estoque com contagem total
    const [estoques, total] = await Promise.all([
      prisma.estoque.findMany({
        where,
        skip,
        take: limitNum,
        orderBy: { dataUltimaAtualizacao: 'desc' },
        select: {
          id: true,
          quantidadeAtual: true,
          quantidadeMinima: true,
          quantidadeMaxima: true,
          precoCusto: true,
          lote: true,
          dataValidade: true,
          dataUltimaAtualizacao: true,
          produto: {
            select: {
              id: true,
              nome: true,
              codigoBarras: true,
              precoVenda: true,
              lote: true
            }
          }
        }
      }),
      prisma.estoque.count({ where })
    ]);
    
    const totalPages = Math.ceil(total / limitNum);
    
    res.json({
      success: true,
      data: {
        items: estoques,
        pagination: {
          currentPage: pageNum,
          totalPages,
          totalItems: total,
          itemsPerPage: limitNum,
          hasNext: pageNum < totalPages,
          hasPrevious: pageNum > 1
        }
      }
    });
    
  } catch (error) {
    next(error);
  }
};

// Obter estoque por produto ID
export const obterEstoqueProduto = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { produtoId } = req.params;
    
    const estoque = await prisma.estoque.findFirst({
      where: {
        produtoId,
        tenantId: req.tenantId,
        ativo: true
      },
      include: {
        produto: {
          select: {
            id: true,
            nome: true,
            codigoBarras: true,
            precoVenda: true,
            lote: true
          }
        }
      }
    });
    
    if (!estoque) {
      throw createError('Estoque não encontrado para este produto', 404);
    }
    
    res.json({
      success: true,
      data: estoque
    });
    
  } catch (error) {
    next(error);
  }
};

// Criar/Atualizar estoque
export const criarOuAtualizarEstoque = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      produtoId,
      quantidadeAtual,
      quantidadeMinima,
      quantidadeMaxima,
      precoCusto,
      lote,
      dataValidade
    } = req.body;
    
    // Validações básicas
    if (!produtoId) {
      throw createError('ID do produto é obrigatório', 400);
    }
    
    // Verificar se produto existe
    const produto = await prisma.produto.findFirst({
      where: {
        id: produtoId,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!produto) {
      throw createError('Produto não encontrado', 404);
    }
    
    // Verificar se estoque já existe
    const estoqueExistente = await prisma.estoque.findFirst({
      where: {
        produtoId,
        tenantId: req.tenantId
      }
    });
    
    let estoque;
    
    if (estoqueExistente) {
      // Atualizar estoque existente
      estoque = await prisma.estoque.update({
        where: { id: estoqueExistente.id },
        data: {
          quantidadeAtual: quantidadeAtual ?? estoqueExistente.quantidadeAtual,
          quantidadeMinima: quantidadeMinima ?? estoqueExistente.quantidadeMinima,
          quantidadeMaxima: quantidadeMaxima ?? estoqueExistente.quantidadeMaxima,
          precoCusto: precoCusto ? parseFloat(precoCusto) : estoqueExistente.precoCusto,
          lote: lote ?? estoqueExistente.lote,
          dataValidade: dataValidade ? new Date(dataValidade) : estoqueExistente.dataValidade,
          ativo: true, // Reativar se estava inativo
          usuarioUltimaAtualizacao: 'api-user'
        },
        include: {
          produto: {
            select: {
              id: true,
              nome: true,
              codigoBarras: true,
              precoVenda: true
            }
          }
        }
      });
    } else {
      // Criar novo estoque
      estoque = await prisma.estoque.create({
        data: {
          produtoId,
          quantidadeAtual: quantidadeAtual || 0,
          quantidadeMinima: quantidadeMinima || 0,
          quantidadeMaxima: quantidadeMaxima || 1000,
          precoCusto: precoCusto ? parseFloat(precoCusto) : 0,
          lote,
          dataValidade: dataValidade ? new Date(dataValidade) : null,
          tenantId: req.tenantId,
          usuarioCadastro: 'api-user',
          usuarioUltimaAtualizacao: 'api-user'
        },
        include: {
          produto: {
            select: {
              id: true,
              nome: true,
              codigoBarras: true,
              precoVenda: true
            }
          }
        }
      });
    }
    
    res.status(estoqueExistente ? 200 : 201).json({
      success: true,
      data: estoque,
      message: estoqueExistente ? 'Estoque atualizado com sucesso' : 'Estoque criado com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Movimentar estoque (entrada/saída/ajuste)
export const movimentarEstoque = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const {
      produtoId,
      tipo, // ENTRADA, SAIDA, AJUSTE
      quantidade,
      motivo
    } = req.body;
    
    // Validações
    if (!produtoId || !tipo || quantidade === undefined || !motivo) {
      throw createError('Todos os campos são obrigatórios (produtoId, tipo, quantidade, motivo)', 400);
    }
    
    if (!['ENTRADA', 'SAIDA', 'AJUSTE'].includes(tipo)) {
      throw createError('Tipo deve ser: ENTRADA, SAIDA ou AJUSTE', 400);
    }
    
    const qtd = parseInt(quantidade);
    if (isNaN(qtd) || qtd <= 0) {
      throw createError('Quantidade deve ser um número positivo', 400);
    }
    
    // Buscar estoque atual
    const estoque = await prisma.estoque.findFirst({
      where: {
        produtoId,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!estoque) {
      throw createError('Estoque não encontrado para este produto', 404);
    }
    
    // Calcular nova quantidade
    let novaQuantidade = estoque.quantidadeAtual;
    
    switch (tipo) {
      case 'ENTRADA':
        novaQuantidade += qtd;
        break;
      case 'SAIDA':
        novaQuantidade -= qtd;
        if (novaQuantidade < 0) {
          throw createError('Estoque insuficiente para esta saída', 400);
        }
        break;
      case 'AJUSTE':
        novaQuantidade = qtd;
        break;
    }
    
    // Executar transação
    const result = await prisma.$transaction(async (prisma) => {
      // Atualizar estoque
      const estoqueAtualizado = await prisma.estoque.update({
        where: { id: estoque.id },
        data: {
          quantidadeAtual: novaQuantidade,
          usuarioUltimaAtualizacao: 'api-user'
        }
      });
      
      // Registrar movimentação
      const movimentacao = await prisma.movimentacaoEstoque.create({
        data: {
          produtoId,
          tipo,
          quantidade: qtd,
          quantidadeAnterior: estoque.quantidadeAtual,
          quantidadeAtual: novaQuantidade,
          motivo,
          usuarioId: 'api-user',
          tenantId: req.tenantId
        }
      });
      
      return { estoque: estoqueAtualizado, movimentacao };
    });
    
    res.json({
      success: true,
      data: result,
      message: 'Movimentação de estoque realizada com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Listar movimentações de estoque
export const listarMovimentacoes = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      page = 1, 
      limit = 20,
      produtoId,
      tipo,
      dataInicio,
      dataFim 
    } = req.query;
    
    const pageNum = parseInt(page as string, 10);
    const limitNum = parseInt(limit as string, 10);
    const skip = (pageNum - 1) * limitNum;
    
    // Filtros
    const where: any = {
      tenantId: req.tenantId
    };
    
    if (produtoId) {
      where.produtoId = produtoId as string;
    }
    
    if (tipo) {
      where.tipo = tipo as string;
    }
    
    if (dataInicio && dataFim) {
      where.dataMovimentacao = {
        gte: new Date(dataInicio as string),
        lte: new Date(dataFim as string)
      };
    }
    
    const [movimentacoes, total] = await Promise.all([
      prisma.movimentacaoEstoque.findMany({
        where,
        skip,
        take: limitNum,
        orderBy: { dataMovimentacao: 'desc' },
        include: {
          produto: {
            select: {
              id: true,
              nome: true,
              codigoBarras: true
            }
          }
        }
      }),
      prisma.movimentacaoEstoque.count({ where })
    ]);
    
    const totalPages = Math.ceil(total / limitNum);
    
    res.json({
      success: true,
      data: {
        items: movimentacoes,
        pagination: {
          currentPage: pageNum,
          totalPages,
          totalItems: total,
          itemsPerPage: limitNum,
          hasNext: pageNum < totalPages,
          hasPrevious: pageNum > 1
        }
      }
    });
    
  } catch (error) {
    next(error);
  }
};