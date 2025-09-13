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
 */

import { Request, Response, NextFunction } from 'express';
import { PrismaClient } from '@prisma/client';
import { ClienteRepository } from '../repositories/ClienteRepository';

const prisma = new PrismaClient();
const clienteRepo = new ClienteRepository(prisma);

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

/**
 * @swagger
 * /api/clientes:
 *   get:
 *     summary: Listar clientes com paginação e filtros
 *     description: Obtém lista paginada de clientes com opções de busca e filtros
 *     tags: [Clientes]
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
 *         description: Busca por nome, sobrenome ou CPF
 *       - in: query
 *         name: ativo
 *         schema:
 *           type: boolean
 *           default: true
 *         description: Filtrar por status ativo
 *     responses:
 *       200:
 *         description: Lista de clientes retornada com sucesso
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para visualizar clientes
 *       500:
 *         description: Erro interno do servidor
 */
export const listarClientes = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { 
      page = 1, 
      limit = 20, 
      search,
      ativo = 'true' 
    } = req.query;
    
    const pageNum = Math.max(1, parseInt(page as string, 10));
    const limitNum = Math.min(100, Math.max(1, parseInt(limit as string, 10)));
    const skip = (pageNum - 1) * limitNum;
    const isAtivo = ativo === 'true';
    const tenantId = req.auth!.tenantId;
    
    const result = await clienteRepo.findWithFilters({
      search: search as string,
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
    console.error('[CLIENTE_CONTROLLER] Erro ao listar clientes:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/{id}:
 *   get:
 *     summary: Obter cliente por ID
 *     description: Retorna os dados completos de um cliente específico
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do cliente
 *     responses:
 *       200:
 *         description: Cliente encontrado com sucesso
 *       404:
 *         description: Cliente não encontrado
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para visualizar clientes
 */
export const obterCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const cliente = await clienteRepo.findById(id, tenantId);
    
    if (!cliente) {
      return res.status(404).json({
        success: false,
        error: 'Cliente não encontrado',
        code: 'CLIENTE_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      data: cliente
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao obter cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes:
 *   post:
 *     summary: Criar novo cliente
 *     description: Cria um novo cliente no sistema
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/ClienteCreateRequest'
 *     responses:
 *       201:
 *         description: Cliente criado com sucesso
 *       400:
 *         description: Dados inválidos
 *       409:
 *         description: CPF já cadastrado
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para criar clientes
 */
export const criarCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { nome, sobrenome, cpf, dataNascimento } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    // Validação de entrada
    const validationErrors = validateClienteData(req.body);
    if (validationErrors.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'Dados inválidos',
        details: validationErrors
      });
    }
    
    // Verificar CPF único (se fornecido)
    if (cpf) {
      const cpfExiste = await clienteRepo.cpfExists(cpf, tenantId);
      if (cpfExiste) {
        return res.status(409).json({
          success: false,
          error: 'CPF já cadastrado',
          code: 'CPF_ALREADY_EXISTS'
        });
      }
    }
    
    const clienteData = {
      nome: nome.trim(),
      sobrenome: sobrenome.trim(),
      cpf: cpf || null,
      dataNascimento: dataNascimento ? new Date(dataNascimento) : null
    };
    
    const cliente = await clienteRepo.add(clienteData, tenantId, userId);
    
    res.status(201).json({
      success: true,
      data: cliente,
      message: 'Cliente criado com sucesso'
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao criar cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/{id}:
 *   put:
 *     summary: Atualizar cliente
 *     description: Atualiza os dados de um cliente existente
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do cliente
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/ClienteCreateRequest'
 *     responses:
 *       200:
 *         description: Cliente atualizado com sucesso
 *       400:
 *         description: Dados inválidos
 *       404:
 *         description: Cliente não encontrado
 *       409:
 *         description: CPF já cadastrado
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para editar clientes
 */
export const atualizarCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { nome, sobrenome, cpf, dataNascimento } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    // Validação de entrada
    const validationErrors = validateClienteData(req.body);
    if (validationErrors.length > 0) {
      return res.status(400).json({
        success: false,
        error: 'Dados inválidos',
        details: validationErrors
      });
    }
    
    // Verificar se cliente existe
    const clienteExiste = await clienteRepo.findById(id, tenantId);
    if (!clienteExiste || !clienteExiste.ativo) {
      return res.status(404).json({
        success: false,
        error: 'Cliente não encontrado',
        code: 'CLIENTE_NOT_FOUND'
      });
    }
    
    // Verificar CPF único (se fornecido e diferente do atual)
    if (cpf && cpf !== clienteExiste.cpf) {
      const cpfExiste = await clienteRepo.cpfExists(cpf, tenantId, id);
      if (cpfExiste) {
        return res.status(409).json({
          success: false,
          error: 'CPF já cadastrado',
          code: 'CPF_ALREADY_EXISTS'
        });
      }
    }
    
    const updateData = {
      nome: nome.trim(),
      sobrenome: sobrenome.trim(),
      cpf: cpf || null,
      dataNascimento: dataNascimento ? new Date(dataNascimento) : null
    };
    
    const cliente = await clienteRepo.update(id, updateData, tenantId, userId);
    
    res.json({
      success: true,
      data: cliente,
      message: 'Cliente atualizado com sucesso'
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao atualizar cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/{id}:
 *   delete:
 *     summary: Remover cliente (soft delete)
 *     description: Remove logicamente um cliente do sistema
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do cliente
 *     requestBody:
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               motivo:
 *                 type: string
 *                 example: "Cliente solicitou remoção"
 *                 description: Motivo da remoção (opcional)
 *     responses:
 *       200:
 *         description: Cliente removido com sucesso
 *       404:
 *         description: Cliente não encontrado
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para deletar clientes
 */
export const deletarCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const { motivo = 'Removido via API' } = req.body;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    const success = await clienteRepo.remove(id, tenantId, userId, motivo);
    
    if (!success) {
      return res.status(404).json({
        success: false,
        error: 'Cliente não encontrado',
        code: 'CLIENTE_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      message: 'Cliente removido com sucesso'
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao deletar cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/estatisticas:
 *   get:
 *     summary: Obter estatísticas de clientes
 *     description: Retorna estatísticas detalhadas dos clientes do tenant
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Estatísticas retornadas com sucesso
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para visualizar relatórios
 */
export const obterEstatisticas = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const tenantId = req.auth!.tenantId;
    
    const estatisticas = await clienteRepo.getStatistics(tenantId);
    
    res.json({
      success: true,
      data: estatisticas
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao obter estatísticas:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/cpf/{cpf}:
 *   get:
 *     summary: Buscar cliente por CPF
 *     description: Encontra cliente através do CPF
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: cpf
 *         required: true
 *         schema:
 *           type: string
 *         description: CPF do cliente (formato 000.000.000-00)
 *     responses:
 *       200:
 *         description: Cliente encontrado com sucesso
 *       404:
 *         description: Cliente não encontrado
 *       401:
 *         description: Não autorizado
 */
export const buscarPorCpf = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { cpf } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const cliente = await clienteRepo.findByCpf(cpf, tenantId);
    
    if (!cliente) {
      return res.status(404).json({
        success: false,
        error: 'Cliente não encontrado',
        code: 'CLIENTE_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      data: cliente
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao buscar por CPF:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/{id}/restore:
 *   post:
 *     summary: Restaurar cliente removido
 *     description: Restaura um cliente que foi removido logicamente
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID do cliente
 *     responses:
 *       200:
 *         description: Cliente restaurado com sucesso
 *       404:
 *         description: Cliente não encontrado
 *       401:
 *         description: Não autorizado
 *       403:
 *         description: Sem permissão para restaurar clientes
 */
export const restaurarCliente = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { id } = req.params;
    const tenantId = req.auth!.tenantId;
    const userId = req.auth!.userId;
    
    const success = await clienteRepo.restore(id, tenantId, userId);
    
    if (!success) {
      return res.status(404).json({
        success: false,
        error: 'Cliente não encontrado',
        code: 'CLIENTE_NOT_FOUND'
      });
    }
    
    res.json({
      success: true,
      message: 'Cliente restaurado com sucesso'
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao restaurar cliente:', error);
    next(error);
  }
};

/**
 * @swagger
 * /api/clientes/idade/{minAge}/{maxAge}:
 *   get:
 *     summary: Buscar clientes por faixa etária
 *     description: Encontra clientes em uma faixa etária específica
 *     tags: [Clientes]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: minAge
 *         required: true
 *         schema:
 *           type: integer
 *         description: Idade mínima
 *       - in: path
 *         name: maxAge
 *         required: true
 *         schema:
 *           type: integer
 *         description: Idade máxima
 *     responses:
 *       200:
 *         description: Clientes encontrados com sucesso
 *       401:
 *         description: Não autorizado
 */
export const buscarPorIdade = async (req: AuthRequest, res: Response, next: NextFunction) => {
  try {
    const { minAge, maxAge } = req.params;
    const tenantId = req.auth!.tenantId;
    
    const clientes = await clienteRepo.findByAgeRange(
      parseInt(minAge), 
      parseInt(maxAge), 
      tenantId
    );
    
    res.json({
      success: true,
      data: clientes
    });
    
  } catch (error) {
    console.error('[CLIENTE_CONTROLLER] Erro ao buscar por idade:', error);
    next(error);
  }
};