/**
 * Venda Controller - REST API Completa
 * Operações CRUD completas com Repository Pattern + Multi-tenant
 * Compliance fiscal brasileiro
 * 
 * @swagger
 * components:
 *   schemas:
 *     Venda:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           example: "vnd_clnx8k3r40000..."
 *         numeroFiscal:
 *           type: string
 *           example: "202501000001"
 *           description: "Número fiscal único YYYYMM000001"
 *         clienteId:
 *           type: string
 *           example: "cli_clnx8k3r40000..."
 *         total:
 *           type: number
 *           format: float
 *           example: 125.50
 *           description: "Valor total da venda"
 *         subtotal:
 *           type: number
 *           format: float
 *           example: 120.00
 *         desconto:
 *           type: number
 *           format: float
 *           example: 5.00
 *         acrescimo:
 *           type: number
 *           format: float
 *           example: 10.50
 *         formaPagamento:
 *           type: string
 *           enum: [DINHEIRO, CARTAO_DEBITO, CARTAO_CREDITO, PIX, VALE]
 *           example: "PIX"
 *         status:
 *           type: string
 *           enum: [PENDENTE, FINALIZADA, CANCELADA]
 *           example: "FINALIZADA"
 *         dataVenda:
 *           type: string
 *           format: date-time
 *         observacoes:
 *           type: string
 *           example: "Entrega programada para amanhã"
 *         itens:
 *           type: array
 *           items:
 *             $ref: '#/components/schemas/VendaItem'
 *         cliente:
 *           $ref: '#/components/schemas/ClienteResumido'
 *     
 *     VendaItem:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *         produtoId:
 *           type: string
 *         quantidade:
 *           type: number
 *           example: 2
 *         precoUnitario:
 *           type: number
 *           format: float
 *           example: 25.50
 *         subtotal:
 *           type: number
 *           format: float
 *           example: 51.00
 *         desconto:
 *           type: number
 *           format: float
 *           example: 1.00
 *         produto:
 *           type: object
 *           properties:
 *             nome:
 *               type: string
 *             categoria:
 *               type: string
 *     
 *     VendaCreateRequest:
 *       type: object
 *       required:
 *         - formaPagamento
 *         - itens
 *       properties:
 *         clienteId:
 *           type: string
 *           example: "cli_clnx8k3r40000..."
 *         formaPagamento:
 *           type: string
 *           enum: [DINHEIRO, CARTAO_DEBITO, CARTAO_CREDITO, PIX, VALE]
 *         desconto:
 *           type: number
 *           format: float
 *           example: 5.00
 *         acrescimo:
 *           type: number
 *           format: float
 *           example: 2.50
 *         observacoes:
 *           type: string
 *         itens:
 *           type: array
 *           minItems: 1
 *           items:
 *             type: object
 *             required:
 *               - produtoId
 *               - quantidade
 *               - precoUnitario
 *             properties:
 *               produtoId:
 *                 type: string
 *               quantidade:
 *                 type: number
 *                 minimum: 0.01
 *               precoUnitario:
 *                 type: number
 *                 format: float
 *                 minimum: 0
 *               desconto:
 *                 type: number
 *                 format: float
 *                 minimum: 0
 *     
 *     VendaListResponse:
 *       type: object
 *       properties:
 *         success:
 *           type: boolean
 *           example: true
 *         data:
 *           type: object
 *           properties:
 *             items:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/Venda'
 *             pagination:
 *               $ref: '#/components/schemas/PaginationInfo'
 */

import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { VendaRepository } from '../repositories/VendaRepository';
import { ProdutoRepository } from '../repositories/ProdutoRepository';

const prisma = new PrismaClient();
const vendaRepo = new VendaRepository(prisma);
const produtoRepo = new ProdutoRepository(prisma);

interface AuthRequest extends Request {
  auth?: {
    userId: string;
    tenantId: string;
    login: string;
    permissions: string[];
  };
}

const createError = (message: string, statusCode: number) => {
  const error = new Error(message) as any;
  error.statusCode = statusCode;
  return error;
};

// Validação de dados de venda
const validateVendaData = (data: any) => {
  const errors: string[] = [];
  
  if (!data.formaPagamento) {
    errors.push('Forma de pagamento é obrigatória');
  }
  
  const formasValidas = ['DINHEIRO', 'CARTAO_DEBITO', 'CARTAO_CREDITO', 'PIX', 'VALE'];
  if (data.formaPagamento && !formasValidas.includes(data.formaPagamento)) {
    errors.push('Forma de pagamento inválida');
  }
  
  if (!data.itens || !Array.isArray(data.itens) || data.itens.length === 0) {
    errors.push('Pelo menos um item é obrigatório');
  }
  
  if (data.itens) {
    data.itens.forEach((item: any, index: number) => {
      if (!item.produtoId) {
        errors.push(`Item ${index + 1}: produto é obrigatório`);
      }
      if (!item.quantidade || item.quantidade <= 0) {
        errors.push(`Item ${index + 1}: quantidade deve ser maior que zero`);
      }
      if (item.precoUnitario === undefined || item.precoUnitario < 0) {
        errors.push(`Item ${index + 1}: preço unitário inválido`);
      }
    });
  }
  
  if (data.desconto && data.desconto < 0) {
    errors.push('Desconto não pode ser negativo');
  }
  
  if (data.acrescimo && data.acrescimo < 0) {
    errors.push('Acréscimo não pode ser negativo');
  }
  
  return errors;
};

/**
 * @swagger
 * /api/vendas:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Listar vendas com paginação e filtros
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: page
 *         in: query
 *         schema:
 *           type: integer
 *           default: 1
 *       - name: limit
 *         in: query
 *         schema:
 *           type: integer
 *           default: 20
 *           maximum: 100
 *       - name: dataInicio
 *         in: query
 *         schema:
 *           type: string
 *           format: date
 *       - name: dataFim
 *         in: query
 *         schema:
 *           type: string
 *           format: date
 *       - name: clienteId
 *         in: query
 *         schema:
 *           type: string
 *       - name: status
 *         in: query
 *         schema:
 *           type: string
 *           enum: [PENDENTE, FINALIZADA, CANCELADA]
 *       - name: formaPagamento
 *         in: query
 *         schema:
 *           type: string
 *           enum: [DINHEIRO, CARTAO_DEBITO, CARTAO_CREDITO, PIX, VALE]
 *       - name: valorMin
 *         in: query
 *         schema:
 *           type: number
 *       - name: valorMax
 *         in: query
 *         schema:
 *           type: number
 *     responses:
 *       200:
 *         description: Lista de vendas
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/VendaListResponse'
 */
export const listarVendas = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const {
      page = 1,
      limit = 20,
      dataInicio,
      dataFim,
      clienteId,
      status,
      formaPagamento,
      valorMin,
      valorMax
    } = req.query;
    
    const tenantId = req.auth!.tenantId;
    const pageNum = parseInt(page as string, 10);
    const limitNum = Math.min(parseInt(limit as string, 10), 100);
    const skip = (pageNum - 1) * limitNum;
    
    const filters = {
      dataInicio: dataInicio ? new Date(dataInicio as string) : undefined,
      dataFim: dataFim ? new Date(dataFim as string) : undefined,
      clienteId: clienteId as string,
      status: status as string,
      formaPagamento: formaPagamento as string,
      valorMin: valorMin ? parseFloat(valorMin as string) : undefined,
      valorMax: valorMax ? parseFloat(valorMax as string) : undefined,
      skip,
      take: limitNum,
      tenantId
    };
    
    const result = await vendaRepo.findWithFilters(filters);
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
    console.error('[VENDA_CONTROLLER] Erro ao listar vendas:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/{id}:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Obter venda por ID
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: id
 *         in: path
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Dados da venda
 *       404:
 *         description: Venda não encontrada
 */
export const obterVenda = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const venda = await vendaRepo.findById(id, tenantId);
    
    if (!venda) {
      throw createError('Venda não encontrada', 404);
    }
    
    res.json({
      success: true,
      data: venda
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao obter venda:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/numero-fiscal/{numeroFiscal}:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Buscar venda por número fiscal
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: numeroFiscal
 *         in: path
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Dados da venda
 *       404:
 *         description: Venda não encontrada
 */
export const obterVendaPorNumeroFiscal = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { numeroFiscal } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const venda = await vendaRepo.findByNumeroFiscal(numeroFiscal, tenantId);
    
    if (!venda) {
      throw createError('Venda não encontrada', 404);
    }
    
    res.json({
      success: true,
      data: venda
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao obter venda por número fiscal:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas:
 *   post:
 *     tags:
 *       - Vendas
 *     summary: Criar nova venda
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/VendaCreateRequest'
 *     responses:
 *       201:
 *         description: Venda criada com sucesso
 *       400:
 *         description: Dados inválidos
 */
export const criarVenda = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    const validationErrors = validateVendaData(req.body);
    if (validationErrors.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'Dados inválidos',
        details: validationErrors
      });
    }
    
    const { clienteId, formaPagamento, desconto = 0, acrescimo = 0, observacoes, itens } = req.body;
    
    // Validar produtos e calcular totais
    let subtotal = 0;
    const itensValidados = [];
    
    for (const item of itens) {
      const produto = await produtoRepo.findById(item.produtoId, tenantId);
      if (!produto) {
        return res.status(400).json({
          success: false,
          error: `Produto não encontrado: ${item.produtoId}`
        });
      }
      
      const itemSubtotal = (item.quantidade * item.precoUnitario) - (item.desconto || 0);
      subtotal += itemSubtotal;
      
      itensValidados.push({
        produtoId: item.produtoId,
        quantidade: item.quantidade,
        precoUnitario: item.precoUnitario,
        desconto: item.desconto || 0,
        subtotal: itemSubtotal
      });
    }
    
    const total = subtotal - desconto + acrescimo;
    const numeroFiscal = await vendaRepo.getNextNumeroFiscal(tenantId);
    
    const vendaData = {
      numeroFiscal,
      clienteId: clienteId || null,
      formaPagamento,
      subtotal,
      desconto,
      acrescimo,
      total,
      status: 'FINALIZADA' as const,
      dataVenda: new Date(),
      observacoes,
      itens: {
        create: itensValidados
      }
    };
    
    const venda = await vendaRepo.add(vendaData, tenantId, userId);
    
    res.status(201).json({
      success: true,
      data: venda,
      message: 'Venda criada com sucesso'
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao criar venda:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/{id}/cancelar:
 *   patch:
 *     tags:
 *       - Vendas
 *     summary: Cancelar venda
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: id
 *         in: path
 *         required: true
 *         schema:
 *           type: string
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - motivo
 *             properties:
 *               motivo:
 *                 type: string
 *                 example: "Cancelamento solicitado pelo cliente"
 *     responses:
 *       200:
 *         description: Venda cancelada com sucesso
 *       404:
 *         description: Venda não encontrada
 *       400:
 *         description: Venda não pode ser cancelada
 */
export const cancelarVenda = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { motivo } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    if (!motivo) {
      return res.status(400).json({
        success: false,
        error: 'Motivo do cancelamento é obrigatório'
      });
    }
    
    const venda = await vendaRepo.cancelSale(id, tenantId, userId, motivo);
    
    if (!venda) {
      throw createError('Venda não encontrada ou não pode ser cancelada', 404);
    }
    
    res.json({
      success: true,
      data: venda,
      message: 'Venda cancelada com sucesso'
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao cancelar venda:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/cliente/{clienteId}:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Listar vendas de um cliente
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: clienteId
 *         in: path
 *         required: true
 *         schema:
 *           type: string
 *       - name: limit
 *         in: query
 *         schema:
 *           type: integer
 *           default: 10
 *     responses:
 *       200:
 *         description: Lista de vendas do cliente
 */
export const listarVendasCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { clienteId } = req.params;
    const { limit } = req.query;
    const tenantId = req.auth!.tenantId;
    
    const vendas = await vendaRepo.findByCliente(
      clienteId, 
      tenantId, 
      limit ? parseInt(limit as string) : undefined
    );
    
    res.json({
      success: true,
      data: vendas
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao listar vendas do cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/estatisticas:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Obter estatísticas de vendas
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Estatísticas de vendas
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 data:
 *                   type: object
 *                   properties:
 *                     totalHoje:
 *                       type: integer
 *                     faturamentoHoje:
 *                       type: number
 *                     totalMes:
 *                       type: integer
 *                     faturamentoMes:
 *                       type: number
 *                     totalCanceladas:
 *                       type: integer
 *                     ticketMedioMes:
 *                       type: number
 */
export const obterEstatisticas = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const estatisticas = await vendaRepo.getStatistics(tenantId);
    
    res.json({
      success: true,
      data: estatisticas
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao obter estatísticas:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/relatorios/vendas:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Relatório de vendas por período
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: dataInicio
 *         in: query
 *         required: true
 *         schema:
 *           type: string
 *           format: date
 *       - name: dataFim
 *         in: query
 *         required: true
 *         schema:
 *           type: string
 *           format: date
 *     responses:
 *       200:
 *         description: Relatório de vendas
 */
export const relatorioVendas = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { dataInicio, dataFim } = req.query;
    const tenantId = req.auth!.tenantId;
    
    if (!dataInicio || !dataFim) {
      return res.status(400).json({
        success: false,
        error: 'Data início e data fim são obrigatórias'
      });
    }
    
    const relatorio = await vendaRepo.getSalesReport(
      new Date(dataInicio as string),
      new Date(dataFim as string),
      tenantId
    );
    
    res.json({
      success: true,
      data: relatorio
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao gerar relatório:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/vendas/proximo-numero-fiscal:
 *   get:
 *     tags:
 *       - Vendas
 *     summary: Obter próximo número fiscal
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Próximo número fiscal
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 data:
 *                   type: object
 *                   properties:
 *                     numeroFiscal:
 *                       type: string
 *                       example: "202501000001"
 */
export const obterProximoNumeroFiscal = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const numeroFiscal = await vendaRepo.getNextNumeroFiscal(tenantId);
    
    res.json({
      success: true,
      data: { numeroFiscal }
    });
    
  } catch (error) {
    console.error('[VENDA_CONTROLLER] Erro ao obter próximo número fiscal:', error);
    next(error);
  }
};