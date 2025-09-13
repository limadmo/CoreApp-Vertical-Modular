import { Router } from 'express';
import {
  listarClientes,
  obterCliente,
  criarCliente,
  atualizarCliente,
  deletarCliente,
  obterEstatisticas,
  buscarPorCpf,
  buscarPorIdade,
  restaurarCliente
} from '../controllers/clienteControllerNew';
import { authenticateAndIsolateTenant, requirePermission } from '../middleware/authMiddleware';

const router = Router();

// Aplicar middleware de autenticação para todas as rotas
router.use(authenticateAndIsolateTenant);

// GET /api/clientes - Listar clientes com paginação
router.get('/', requirePermission(['clientes:read']), listarClientes);

// GET /api/clientes/estatisticas - Estatísticas de clientes
router.get('/estatisticas', requirePermission(['clientes:read']), obterEstatisticas);

// GET /api/clientes/cpf/:cpf - Buscar cliente por CPF
router.get('/cpf/:cpf', requirePermission(['clientes:read']), buscarPorCpf);

// GET /api/clientes/idade/:idadeMin/:idadeMax - Buscar por faixa etária
router.get('/idade/:idadeMin/:idadeMax', requirePermission(['clientes:read']), buscarPorIdade);

// PUT /api/clientes/:id/restaurar - Restaurar cliente deletado
router.put('/:id/restaurar', requirePermission(['clientes:write']), restaurarCliente);

// GET /api/clientes/:id - Obter cliente por ID
router.get('/:id', requirePermission(['clientes:read']), obterCliente);

// POST /api/clientes - Criar novo cliente
router.post('/', requirePermission(['clientes:write']), criarCliente);

// PUT /api/clientes/:id - Atualizar cliente
router.put('/:id', requirePermission(['clientes:write']), atualizarCliente);

// DELETE /api/clientes/:id - Soft delete cliente
router.delete('/:id', requirePermission(['clientes:delete']), deletarCliente);

export default router;