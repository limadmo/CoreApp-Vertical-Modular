/**
 * Cliente Controller - REST API Completa
 * Operações CRUD completas com Repository Pattern + Multi-tenant
 * 
 * @swagger
 * components:
 *   schemas:
 *     Cliente:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           example: "clnx8k3r40000..."
 *         nome:
 *           type: string
 *           example: "João"
 *           description: "Nome do cliente"
 *         sobrenome:
 *           type: string
 *           example: "Silva"
 *           description: "Sobrenome do cliente"
 *         cpf:
 *           type: string
 *           example: "123.456.789-00"
 *           description: "CPF do cliente (opcional)"
 *         dataNascimento:
 *           type: string
 *           format: date
 *           example: "1990-05-15"
 *           description: "Data de nascimento (opcional)"
 *         ativo:
 *           type: boolean
 *           example: true
 *         dataCadastro:
 *           type: string
 *           format: date-time
 *         dataUltimaAtualizacao:
 *           type: string
 *           format: date-time
 *     
 *     ClienteCreateRequest:
 *       type: object
 *       required:
 *         - nome
 *         - sobrenome
 *       properties:
 *         nome:
 *           type: string
 *           minLength: 2
 *           maxLength: 50
 *           example: "João"
 *         sobrenome:
 *           type: string
 *           minLength: 2
 *           maxLength: 50
 *           example: "Silva"
 *         cpf:
 *           type: string
 *           pattern: "^\\d{3}\\.\\d{3}\\.\\d{3}-\\d{2}$"
 *           example: "123.456.789-00"
 *         dataNascimento:
 *           type: string
 *           format: date
 *           example: "1990-05-15"
 *     
 *     ClienteListResponse:
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
 *                 $ref: '#/components/schemas/Cliente'
 *             pagination:
 *               type: object
 *               properties:
 *                 currentPage:
 *                   type: integer
 *                   example: 1
 *                 totalPages:
 *                   type: integer
 *                   example: 5
 *                 totalItems:
 *                   type: integer
 *                   example: 100
 *                 itemsPerPage:
 *                   type: integer
 *                   example: 20
 *                 hasNext:
 *                   type: boolean
 *                   example: true
 *                 hasPrevious:
 *                   type: boolean
 *                   example: false
 */

import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { ClienteRepository } from '../repositories/ClienteRepository';
import { 
  authenticateAndIsolateTenant,
  requirePermission,
  auditLog 
} from '../middleware/authMiddleware';

const prisma = new PrismaClient();
const clienteRepo = new ClienteRepository(prisma);

// Tipos de request com contexto de autenticação
interface AuthRequest extends Request {
  auth?: {
    userId: string;
    tenantId: string;
    login: string;
    permissions: string[];
  };
}

// Validação de entrada
const validateClienteData = (data: any) => {
  const errors: string[] = [];
  
  if (!data.nome || data.nome.trim().length < 2) {
    errors.push('Nome deve ter pelo menos 2 caracteres');
  }
  
  if (!data.sobrenome || data.sobrenome.trim().length < 2) {
    errors.push('Sobrenome deve ter pelo menos 2 caracteres');
  }
  
  if (data.cpf && !/^\d{3}\.\d{3}\.\d{3}-\d{2}$/.test(data.cpf)) {
    errors.push('CPF deve estar no formato 000.000.000-00');
  }
  
  if (data.dataNascimento && new Date(data.dataNascimento) > new Date()) {
    errors.push('Data de nascimento não pode ser futura');
  }
  
  return errors;
};

// Listar clientes com paginação
export const listarClientes = async (req: TenantRequest, res: Response, next: NextFunction) => {
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
        { sobrenome: { contains: search as string, mode: 'insensitive' } },
        { cpf: { contains: search as string, mode: 'insensitive' } }
      ];
    }
    
    // Buscar clientes com contagem total
    const [clientes, total] = await Promise.all([
      prisma.cliente.findMany({
        where,
        skip,
        take: limitNum,
        orderBy: { dataCadastro: 'desc' },
        select: {
          id: true,
          nome: true,
          sobrenome: true,
          cpf: true,
          dataNascimento: true,
          ativo: true,
          dataCadastro: true
        }
      }),
      prisma.cliente.count({ where })
    ]);
    
    const totalPages = Math.ceil(total / limitNum);
    
    res.json({
      success: true,
      data: {
        items: clientes,
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

// Obter cliente por ID
export const obterCliente = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    
    const cliente = await prisma.cliente.findFirst({
      where: {
        id,
        tenantId: req.tenantId
      }
    });
    
    if (!cliente) {
      throw createError('Cliente não encontrado', 404);
    }
    
    res.json({
      success: true,
      data: cliente
    });
    
  } catch (error) {
    next(error);
  }
};

// Criar cliente
export const criarCliente = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { nome, sobrenome, cpf, dataNascimento } = req.body;
    
    // Validações básicas
    if (!nome || !sobrenome) {
      throw createError('Nome e sobrenome são obrigatórios', 400);
    }
    
    // Verificar CPF único (se fornecido)
    if (cpf) {
      const cpfExiste = await prisma.cliente.findFirst({
        where: {
          cpf,
          tenantId: req.tenantId,
          ativo: true
        }
      });
      
      if (cpfExiste) {
        throw createError('CPF já cadastrado', 409);
      }
    }
    
    const cliente = await prisma.cliente.create({
      data: {
        nome,
        sobrenome,
        cpf,
        dataNascimento: dataNascimento ? new Date(dataNascimento) : null,
        tenantId: req.tenantId,
        usuarioCadastro: 'api-user',
        usuarioUltimaAtualizacao: 'api-user'
      }
    });
    
    res.status(201).json({
      success: true,
      data: cliente,
      message: 'Cliente criado com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Atualizar cliente
export const atualizarCliente = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { nome, sobrenome, cpf, dataNascimento } = req.body;
    
    // Verificar se cliente existe
    const clienteExiste = await prisma.cliente.findFirst({
      where: {
        id,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!clienteExiste) {
      throw createError('Cliente não encontrado', 404);
    }
    
    // Verificar CPF único (se fornecido e diferente do atual)
    if (cpf && cpf !== clienteExiste.cpf) {
      const cpfExiste = await prisma.cliente.findFirst({
        where: {
          cpf,
          tenantId: req.tenantId,
          ativo: true,
          NOT: { id }
        }
      });
      
      if (cpfExiste) {
        throw createError('CPF já cadastrado', 409);
      }
    }
    
    const cliente = await prisma.cliente.update({
      where: { id },
      data: {
        nome,
        sobrenome,
        cpf,
        dataNascimento: dataNascimento ? new Date(dataNascimento) : null,
        usuarioUltimaAtualizacao: 'api-user'
      }
    });
    
    res.json({
      success: true,
      data: cliente,
      message: 'Cliente atualizado com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Soft delete cliente
export const deletarCliente = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { motivo = 'Removido via API' } = req.body;
    
    // Verificar se cliente existe e está ativo
    const clienteExiste = await prisma.cliente.findFirst({
      where: {
        id,
        tenantId: req.tenantId,
        ativo: true
      }
    });
    
    if (!clienteExiste) {
      throw createError('Cliente não encontrado', 404);
    }
    
    // Soft delete
    await prisma.cliente.update({
      where: { id },
      data: {
        ativo: false,
        dataDelecao: new Date(),
        motivoDelecao: motivo,
        usuarioDelecao: 'api-user',
        usuarioUltimaAtualizacao: 'api-user'
      }
    });
    
    res.json({
      success: true,
      message: 'Cliente removido com sucesso'
    });
    
  } catch (error) {
    next(error);
  }
};

// Estatísticas de clientes
export const obterEstatisticas = async (req: TenantRequest, res: Response, next: NextFunction) => {
  try {
    const [totalClientes, clientesAtivos, clientesInativos] = await Promise.all([
      prisma.cliente.count({ where: { tenantId: req.tenantId } }),
      prisma.cliente.count({ where: { tenantId: req.tenantId, ativo: true } }),
      prisma.cliente.count({ where: { tenantId: req.tenantId, ativo: false } })
    ]);
    
    const estatisticas = {
      totalClientes,
      clientesAtivos,
      clientesInativos,
      novosMesAtual: 0 // TODO: implementar contagem por mês
    };
    
    res.json({
      success: true,
      data: estatisticas
    });
    
  } catch (error) {
    next(error);
  }
};