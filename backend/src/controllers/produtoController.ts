import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { TenantRequest } from '../middleware/tenant';
import { createError } from '../middleware/errorHandler';

const prisma = new PrismaClient();

// Listar produtos com paginação
export const listarProdutos = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      page = 1, 
      limit = 20, 
      search,
      ativo = 'true' 
    } = req.query;
    
    const pageNum = parseInt(page as string, 10);
    const limitNum = parseInt(limit as string, 10);
    const skip = (pageNum - 1) * limitNum;
    const isAtivo = ativo === 'true';
    
    // Filtros de busca
    const where: any = {
      tenantId: req.tenantId,
      ativo: isAtivo
    };
    
    if (search) {
      where.OR = [
        { nome: { contains: search as string, mode: 'insensitive' } },
        { codigoBarras: { contains: search as string, mode: 'insensitive' } },
        { lote: { contains: search as string, mode: 'insensitive' } }
      ];
    }
    
    // Buscar produtos com contagem total
    const [produtos, total] = await Promise.all([
      prisma.produto.findMany({
        where,
        skip,
        take: limitNum,
        orderBy: { dataCadastro: 'desc' },
        select: {
          id: true,
          nome: true,
          codigoBarras: true,
          precoVenda: true,
          quantidade: true,
          lote: true,
          ativo: true,
          dataCadastro: true,
          estoque: {
            select: {
              quantidadeAtual: true,
              quantidadeMinima: true,
              precoCusto: true,
              dataValidade: true
            }
          }
        }
      }),
      prisma.produto.count({ where })
    ]);
    
    const totalPages = Math.ceil(total / limitNum);
    
    res.json({
      success: true,
      data: {
        items: produtos,
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

// Obter produto por ID
export const obterProduto = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    
    const produto = await prisma.produto.findFirst({
      where: {
        id,
        tenantId: req.tenantId
      },
      include: {
        estoque: true
      }
    });
    
    if (!produto) {
      throw createError('Produto não encontrado', 404);
    }
    
    res.json({
      success: true,
      data: produto
    });
    
  } catch (error) {
    next(error);
  }
};

// Criar produto
export const criarProduto = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      nome, 
      codigoBarras, 
      precoVenda, 
      quantidade = 0, 
      lote 
    } = req.body;
    
    // Validações básicas
    if (!nome) {
      throw createError('Nome do produto é obrigatório', 400);
    }
    
    if (precoVenda <= 0) {
      throw createError('Preço de venda deve ser maior que zero', 400);
    }
    
    // Verificar código de barras único (se fornecido)
    if (codigoBarras) {
      const codigoExiste = await prisma.produto.findFirst({
        where: {
          codigoBarras,
          tenantId: req.tenantId,
          ativo: true
        }
      });
      
      if (codigoExiste) {
        throw createError('Código de barras já cadastrado', 409);
      }
    }
    
    const produto = await prisma.produto.create({
      data: {
        nome,
        codigoBarras,
        precoVenda: parseFloat(precoVenda),
        quantidade: parseInt(quantidade),
        lote,
        tenantId: req.tenantId,
        usuarioCadastro: 'api-user',
        usuarioUltimaAtualizacao: 'api-user'
      }
    });
    
    res.status(201).json({
      success: true,
      data: produto,
      message: 'Produto criado com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Atualizar produto
export const atualizarProduto = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { 
      nome, 
      codigoBarras, 
      precoVenda, 
      quantidade, 
      lote 
    } = req.body;
    
    // Verificar se produto existe
    const produtoExiste = await prisma.produto.findFirst({
      where: {
        id,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!produtoExiste) {
      throw createError('Produto não encontrado', 404);
    }
    
    // Verificar código de barras único (se fornecido e diferente do atual)
    if (codigoBarras && codigoBarras !== produtoExiste.codigoBarras) {
      const codigoExiste = await prisma.produto.findFirst({
        where: {
          codigoBarras,
          tenantId: req.tenantId,
          ativo: true,
          NOT: { id }
        }
      });
      
      if (codigoExiste) {
        throw createError('Código de barras já cadastrado', 409);
      }
    }
    
    const produto = await prisma.produto.update({
      where: { id },
      data: {
        nome,
        codigoBarras,
        precoVenda: precoVenda ? parseFloat(precoVenda) : undefined,
        quantidade: quantidade !== undefined ? parseInt(quantidade) : undefined,
        lote,
        usuarioUltimaAtualizacao: 'api-user'
      }
    });
    
    res.json({
      success: true,
      data: produto,
      message: 'Produto atualizado com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Soft delete produto
export const deletarProduto = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { motivo = 'Removido via API' } = req.body;
    
    // Verificar se produto existe e está ativo
    const produtoExiste = await prisma.produto.findFirst({
      where: {
        id,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!produtoExiste) {
      throw createError('Produto não encontrado', 404);
    }
    
    // Soft delete do produto e estoque relacionado
    await prisma.$transaction([
      prisma.produto.update({
        where: { id },
        data: {
          ativo: false,
          dataDelecao: new Date(),
          motivoDelecao: motivo,
          usuarioDelecao: 'api-user',
          usuarioUltimaAtualizacao: 'api-user'
        }
      }),
      prisma.estoque.updateMany({
        where: { produtoId: id },
        data: {
          ativo: false,
          dataDelecao: new Date(),
          motivoDelecao: motivo,
          usuarioDelecao: 'api-user',
          usuarioUltimaAtualizacao: 'api-user'
        }
      })
    ]);
    
    res.json({
      success: true,
      message: 'Produto removido com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Buscar produtos por código de barras
export const buscarPorCodigoBarras = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { codigo } = req.params;
    
    const produto = await prisma.produto.findFirst({
      where: {
        codigoBarras: codigo,
        tenantId: req.tenantId,
        ativo: true
      },
      include: {
        estoque: true
      }
    });
    
    if (!produto) {
      throw createError('Produto não encontrado', 404);
    }
    
    res.json({
      success: true,
      data: produto
    });
    
  } catch (error) {
    next(error);
  }
};