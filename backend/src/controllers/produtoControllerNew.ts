/**
 * Produto Controller - REST API Completa
 * Operações CRUD para produtos com Repository Pattern + Vertical específico
 * 
 * @swagger
 * components:
 *   schemas:
 *     Produto:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           example: "clnx8k3r40000..."
 *         nome:
 *           type: string
 *           example: "Pão Francês"
 *           description: "Nome do produto"
 *         codigoBarras:
 *           type: string
 *           example: "1234567890123"
 *           description: "Código de barras (opcional)"
 *         precoVenda:
 *           type: number
 *           format: decimal
 *           example: 0.50
 *           description: "Preço de venda"
 *         quantidade:
 *           type: integer
 *           example: 100
 *           description: "Quantidade em estoque"
 *         categoria:
 *           type: string
 *           example: "Panificação"
 *           description: "Categoria do produto"
 *         validadeHoras:
 *           type: integer
 *           example: 24
 *           description: "Validade em horas (específico para padaria)"
 *         tempoPreparo:
 *           type: integer
 *           example: 30
 *           description: "Tempo de preparo em minutos"
 *         ingredientes:
 *           type: string
 *           example: "Farinha, água, sal, fermento"
 *           description: "Lista de ingredientes"
 *         ativo:
 *           type: boolean
 *           example: true
 *         dataCadastro:
 *           type: string
 *           format: date-time
 *     
 *     ProdutoCreateRequest:
 *       type: object
 *       required:
 *         - nome
 *         - precoVenda
 *       properties:
 *         nome:
 *           type: string
 *           minLength: 2
 *           maxLength: 100
 *           example: "Pão Francês"
 *         codigoBarras:
 *           type: string
 *           pattern: "^\\d{8,13}$"
 *           example: "1234567890123"
 *         precoVenda:
 *           type: number
 *           format: decimal
 *           minimum: 0.01
 *           example: 0.50
 *         quantidade:
 *           type: integer
 *           minimum: 0
 *           example: 100
 *         categoria:
 *           type: string
 *           example: "Panificação"
 *         validadeHoras:
 *           type: integer
 *           minimum: 1
 *           example: 24
 *         tempoPreparo:
 *           type: integer
 *           minimum: 1
 *           example: 30
 *         ingredientes:
 *           type: string
 *           example: "Farinha, água, sal, fermento"
 */

import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { ProdutoRepository } from '../repositories/ProdutoRepository';

const prisma = new PrismaClient();
const produtoRepo = new ProdutoRepository(prisma);

// Interface para request autenticado
interface AuthRequest extends Request {
  auth?: {
    userId: string;
    tenantId: string;
    login: string;
    permissions: string[];
  };
}

// Validação de entrada
const validateProdutoData = (data: any) => {
  const errors: string[] = [];
  
  if (!data.nome || data.nome.trim().length < 2) {
    errors.push('Nome deve ter pelo menos 2 caracteres');
  }
  
  if (!data.precoVenda || data.precoVenda <= 0) {
    errors.push('Preço de venda deve ser maior que zero');
  }
  
  if (data.codigoBarras && !/^\d{8,13}$/.test(data.codigoBarras)) {
    errors.push('Código de barras deve ter entre 8 e 13 dígitos');
  }
  
  if (data.quantidade !== undefined && data.quantidade < 0) {
    errors.push('Quantidade não pode ser negativa');
  }
  
  if (data.validadeHoras !== undefined && data.validadeHoras <= 0) {
    errors.push('Validade em horas deve ser maior que zero');
  }
  
  if (data.tempoPreparo !== undefined && data.tempoPreparo <= 0) {
    errors.push('Tempo de preparo deve ser maior que zero');
  }
  
  return errors;
};

/**
 * @swagger
 * /api/produtos:
 *   get:
 *     summary: Listar produtos com paginação e filtros
 *     description: Obtém lista paginada de produtos com opções de busca e filtros
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           minimum: 1
 *           default: 1
 *         description: Número da página
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           minimum: 1
 *           maximum: 100
 *           default: 20
 *         description: Itens por página
 *       - in: query
 *         name: search
 *         schema:
 *           type: string
 *         description: Busca por nome, código de barras ou categoria
 *       - in: query
 *         name: categoria
 *         schema:
 *           type: string
 *         description: Filtrar por categoria
 *       - in: query
 *         name: precoMin
 *         schema:
 *           type: number
 *         description: Preço mínimo
 *       - in: query
 *         name: precoMax
 *         schema:
 *           type: number
 *         description: Preço máximo
 *       - in: query
 *         name: ativo
 *         schema:
 *           type: boolean
 *           default: true
 *         description: Filtrar por status ativo
 *     responses:
 *       200:
 *         description: Lista de produtos retornada com sucesso
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para visualizar produtos
 */
export const listarProdutos = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      page = 1, 
      limit = 20, 
      search,
      categoria,
      precoMin,
      precoMax,
      ativo = 'true' 
    } = req.query;
    
    const pageNum = Math.max(1, parseInt(page as string, 10));
    const limitNum = Math.min(100, Math.max(1, parseInt(limit as string, 10)));
    const skip = (pageNum - 1) * limitNum;
    const isAtivo = ativo === 'true';
    const tenantId = req.auth!.tenantId;
    
    const result = await produtoRepo.findWithFilters({
      search: search as string,
      categoria: categoria as string,
      precoMin: precoMin ? parseFloat(precoMin as string) : undefined,
      precoMax: precoMax ? parseFloat(precoMax as string) : undefined,
      ativo: isAtivo,
      skip,
      take: limitNum,
      tenantId
    });
    
    const totalPages = Math.ceil(result.total / limitNum);
    
    res.json({
      success: true,
      data: {
        items: result.data,
        pagination: {
          currentPage: pageNum,
          totalPages,
          totalItems: result.total,
          itemsPerPage: limitNum,
          hasNext: result.hasNext,
          hasPrevious: result.hasPrevious
        }
      }
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao listar produtos:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/{id}:
 *   get:
 *     summary: Obter produto por ID
 *     description: Retorna os dados completos de um produto específico
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do produto
 *     responses:
 *       200:
 *         description: Produto encontrado com sucesso
 *       404:
 *         description: Produto não encontrado
 */
export const obterProduto = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const produto = await produtoRepo.findById(id, tenantId);
    
    if (!produto) {
      return res.status(404).json({
        success: false,
        error: 'Produto não encontrado',
        code: 'PRODUTO_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      data: produto
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao obter produto:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos:
 *   post:
 *     summary: Criar novo produto
 *     description: Cria um novo produto no sistema
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/ProdutoCreateRequest'
 *     responses:
 *       201:
 *         description: Produto criado com sucesso
 *       400:
 *         description: Dados inválidos
 *       409:
 *         description: Código de barras já cadastrado
 */
export const criarProduto = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      nome, 
      codigoBarras, 
      precoVenda, 
      quantidade = 0,
      categoria,
      validadeHoras,
      tempoPreparo,
      ingredientes 
    } = req.body;
    
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    // Validação de entrada
    const validationErrors = validateProdutoData(req.body);
    if (validationErrors.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'Dados inválidos',
        details: validationErrors
      });
    }
    
    // Verificar código de barras único (se fornecido)
    if (codigoBarras) {
      const codigoExiste = await produtoRepo.codigoBarrasExists(codigoBarras, tenantId);
      if (codigoExiste) {
        return res.status(409).json({
          success: false,
          error: 'Código de barras já cadastrado',
          code: 'CODIGO_BARRAS_ALREADY_EXISTS'
        });
      }
    }
    
    const produtoData = {
      nome: nome.trim(),
      codigoBarras: codigoBarras || null,
      precoVenda: parseFloat(precoVenda),
      quantidade: parseInt(quantidade),
      lote: null,
      categoria: categoria?.trim() || null,
      validadeHoras: validadeHoras ? parseInt(validadeHoras) : null,
      tempoPreparo: tempoPreparo ? parseInt(tempoPreparo) : null,
      ingredientes: ingredientes?.trim() || null
    };
    
    const produto = await produtoRepo.add(produtoData, tenantId, userId);
    
    res.status(201).json({
      success: true,
      data: produto,
      message: 'Produto criado com sucesso'
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao criar produto:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/{id}:
 *   put:
 *     summary: Atualizar produto
 *     description: Atualiza os dados de um produto existente
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do produto
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/ProdutoCreateRequest'
 *     responses:
 *       200:
 *         description: Produto atualizado com sucesso
 *       400:
 *         description: Dados inválidos
 *       404:
 *         description: Produto não encontrado
 *       409:
 *         description: Código de barras já cadastrado
 */
export const atualizarProduto = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { 
      nome, 
      codigoBarras, 
      precoVenda, 
      quantidade,
      categoria,
      validadeHoras,
      tempoPreparo,
      ingredientes 
    } = req.body;
    
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    // Validação de entrada
    const validationErrors = validateProdutoData(req.body);
    if (validationErrors.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'Dados inválidos',
        details: validationErrors
      });
    }
    
    // Verificar se produto existe
    const produtoExiste = await produtoRepo.findById(id, tenantId);
    if (!produtoExiste || !produtoExiste.ativo) {
      return res.status(404).json({
        success: false,
        error: 'Produto não encontrado',
        code: 'PRODUTO_NOT_FOUND'
      });
    }
    
    // Verificar código de barras único (se fornecido e diferente do atual)
    if (codigoBarras && codigoBarras !== produtoExiste.codigoBarras) {
      const codigoExiste = await produtoRepo.codigoBarrasExists(codigoBarras, tenantId, id);
      if (codigoExiste) {
        return res.status(409).json({
          success: false,
          error: 'Código de barras já cadastrado',
          code: 'CODIGO_BARRAS_ALREADY_EXISTS'
        });
      }
    }
    
    const updateData = {
      nome: nome.trim(),
      codigoBarras: codigoBarras || null,
      precoVenda: parseFloat(precoVenda),
      quantidade: quantidade !== undefined ? parseInt(quantidade) : produtoExiste.quantidade,
      categoria: categoria?.trim() || null,
      validadeHoras: validadeHoras ? parseInt(validadeHoras) : null,
      tempoPreparo: tempoPreparo ? parseInt(tempoPreparo) : null,
      ingredientes: ingredientes?.trim() || null
    };
    
    const produto = await produtoRepo.update(id, updateData, tenantId, userId);
    
    res.json({
      success: true,
      data: produto,
      message: 'Produto atualizado com sucesso'
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao atualizar produto:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/{id}:
 *   delete:
 *     summary: Remover produto (soft delete)
 *     description: Remove logicamente um produto do sistema
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do produto
 *     responses:
 *       200:
 *         description: Produto removido com sucesso
 *       404:
 *         description: Produto não encontrado
 */
export const deletarProduto = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { motivo = 'Removido via API' } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    const success = await produtoRepo.remove(id, tenantId, userId, motivo);
    
    if (!success) {
      return res.status(404).json({
        success: false,
        error: 'Produto não encontrado',
        code: 'PRODUTO_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      message: 'Produto removido com sucesso'
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao deletar produto:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/estatisticas:
 *   get:
 *     summary: Obter estatísticas de produtos
 *     description: Retorna estatísticas detalhadas dos produtos do tenant
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Estatísticas retornadas com sucesso
 */
export const obterEstatisticas = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const estatisticas = await produtoRepo.getStatistics(tenantId);
    
    res.json({
      success: true,
      data: estatisticas
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao obter estatísticas:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/categorias:
 *   get:
 *     summary: Listar categorias únicas
 *     description: Retorna lista de todas as categorias de produtos do tenant
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Categorias retornadas com sucesso
 */
export const listarCategorias = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const categorias = await produtoRepo.getUniqueCategories(tenantId);
    
    res.json({
      success: true,
      data: categorias
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao listar categorias:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/codigo-barras/{codigoBarras}:
 *   get:
 *     summary: Buscar produto por código de barras
 *     description: Encontra produto através do código de barras
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: codigoBarras
 *         required: true
 *         schema:
 *           type: string
 *         description: Código de barras do produto
 *     responses:
 *       200:
 *         description: Produto encontrado com sucesso
 *       404:
 *         description: Produto não encontrado
 */
export const buscarPorCodigoBarras = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { codigoBarras } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const produto = await produtoRepo.findByCodigoBarras(codigoBarras, tenantId);
    
    if (!produto) {
      return res.status(404).json({
        success: false,
        error: 'Produto não encontrado',
        code: 'PRODUTO_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      data: produto
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao buscar por código de barras:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/estoque-baixo:
 *   get:
 *     summary: Listar produtos com estoque baixo
 *     description: Retorna produtos com estoque baixo ou zerado
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Produtos com estoque baixo retornados com sucesso
 */
export const listarEstoqueBaixo = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const produtos = await produtoRepo.findWithLowStock(tenantId);
    
    res.json({
      success: true,
      data: produtos
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao listar estoque baixo:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/vencimento/{dias}:
 *   get:
 *     summary: Produtos próximos ao vencimento
 *     description: Retorna produtos que vencem nos próximos N dias (para padaria)
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: dias
 *         required: true
 *         schema:
 *           type: integer
 *         description: Número de dias à frente
 *     responses:
 *       200:
 *         description: Produtos próximos ao vencimento retornados com sucesso
 */
export const listarProximosVencimento = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { dias } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const produtos = await produtoRepo.findNearExpiration(parseInt(dias), tenantId);
    
    res.json({
      success: true,
      data: produtos
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao listar próximos ao vencimento:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/produtos/categoria/{categoria}/preco:
 *   put:
 *     summary: Atualizar preços por categoria
 *     description: Aplica aumento/desconto percentual em todos os produtos de uma categoria
 *     tags: [Produtos]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: categoria
 *         required: true
 *         schema:
 *           type: string
 *         description: Nome da categoria
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               percentualAumento:
 *                 type: number
 *                 example: 10.5
 *                 description: Percentual de aumento (negativo para desconto)
 *     responses:
 *       200:
 *         description: Preços atualizados com sucesso
 */
export const atualizarPrecosPorCategoria = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { categoria } = req.params;
    const { percentualAumento } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    if (!percentualAumento || isNaN(percentualAumento)) {
      return res.status(400).json({
        success: false,
        error: 'Percentual de aumento é obrigatório e deve ser numérico'
      });
    }
    
    const produtosAtualizados = await produtoRepo.updatePricesByCategory(
      categoria,
      parseFloat(percentualAumento),
      tenantId,
      userId
    );
    
    res.json({
      success: true,
      message: `${produtosAtualizados} produtos atualizados com sucesso`,
      data: {
        produtosAtualizados,
        categoria,
        percentualAumento: parseFloat(percentualAumento)
      }
    });
    
  } catch (error) {
    console.error('[PRODUTO_CONTROLLER] Erro ao atualizar preços por categoria:', error);
    next(error);
  }
};