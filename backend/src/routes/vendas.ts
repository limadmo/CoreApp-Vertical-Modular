import { Router } from 'express';
import {
  listarVendas,
  obterVenda,
  criarVenda,
  cancelarVenda,
  obterVendaPorNumeroFiscal,
  listarVendasCliente,
  obterEstatisticas,
  relatorioVendas,
  obterProximoNumeroFiscal
} from '../controllers/vendaControllerNew';
import { authenticateAndIsolateTenant, requirePermission } from '../middleware/authMiddleware';

const router = Router();

// Aplicar middleware de autenticação para todas as rotas
router.use(authenticateAndIsolateTenant);

// GET /api/vendas - Listar vendas com paginação e filtros
router.get('/', requirePermission(['vendas:read']), listarVendas);

// GET /api/vendas/estatisticas - Estatísticas de vendas
router.get('/estatisticas', requirePermission(['vendas:read']), obterEstatisticas);

// GET /api/vendas/proximo-numero-fiscal - Obter próximo número fiscal
router.get('/proximo-numero-fiscal', requirePermission(['vendas:read']), obterProximoNumeroFiscal);

// GET /api/vendas/relatorios/vendas - Relatório de vendas por período
router.get('/relatorios/vendas', requirePermission(['vendas:read']), relatorioVendas);

// GET /api/vendas/numero-fiscal/:numeroFiscal - Buscar venda por número fiscal
router.get('/numero-fiscal/:numeroFiscal', requirePermission(['vendas:read']), obterVendaPorNumeroFiscal);

// GET /api/vendas/cliente/:clienteId - Listar vendas de um cliente
router.get('/cliente/:clienteId', requirePermission(['vendas:read']), listarVendasCliente);

// GET /api/vendas/:id - Obter venda por ID
router.get('/:id', requirePermission(['vendas:read']), obterVenda);

// POST /api/vendas - Criar nova venda
router.post('/', requirePermission(['vendas:write']), criarVenda);

// PATCH /api/vendas/:id/cancelar - Cancelar venda
router.patch('/:id/cancelar', requirePermission(['vendas:cancel']), cancelarVenda);

export default router;