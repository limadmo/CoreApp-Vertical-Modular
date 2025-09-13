import { Router } from 'express';
import {
  listarProdutos,
  obterProduto,
  criarProduto,
  atualizarProduto,
  deletarProduto,
  buscarPorCodigoBarras,
  listarCategorias,
  listarEstoqueBaixo,
  listarProximosVencimento,
  obterEstatisticas,
  atualizarPrecosPorCategoria
} from '../controllers/produtoControllerNew';
import { authenticateAndIsolateTenant, requirePermission } from '../middleware/authMiddleware';

const router = Router();

// Aplicar middleware de autenticação para todas as rotas
router.use(authenticateAndIsolateTenant);

// GET /api/produtos - Listar produtos com paginação
router.get('/', requirePermission(['produtos:read']), listarProdutos);

// GET /api/produtos/categorias - Listar categorias únicas
router.get('/categorias', requirePermission(['produtos:read']), listarCategorias);

// GET /api/produtos/estatisticas - Estatísticas de produtos
router.get('/estatisticas', requirePermission(['produtos:read']), obterEstatisticas);

// GET /api/produtos/estoque-baixo - Listar produtos com estoque baixo
router.get('/estoque-baixo', requirePermission(['produtos:read']), listarEstoqueBaixo);

// GET /api/produtos/proximos-vencimento/:dias - Produtos próximos ao vencimento
router.get('/proximos-vencimento/:dias', requirePermission(['produtos:read']), listarProximosVencimento);

// NOTE: listarPorCategoria function not implemented yet - use regular listar with filters

// GET /api/produtos/codigo/:codigo - Buscar produto por código de barras
router.get('/codigo/:codigo', requirePermission(['produtos:read']), buscarPorCodigoBarras);

// GET /api/produtos/:id - Obter produto por ID
router.get('/:id', requirePermission(['produtos:read']), obterProduto);

// POST /api/produtos - Criar novo produto
router.post('/', requirePermission(['produtos:write']), criarProduto);

// PUT /api/produtos/:id - Atualizar produto
router.put('/:id', requirePermission(['produtos:write']), atualizarProduto);

// PUT /api/produtos/categoria/:categoria/precos - Atualizar preços por categoria
router.put('/categoria/:categoria/precos', requirePermission(['produtos:write']), atualizarPrecosPorCategoria);

// DELETE /api/produtos/:id - Soft delete produto
router.delete('/:id', requirePermission(['produtos:delete']), deletarProduto);

export default router;