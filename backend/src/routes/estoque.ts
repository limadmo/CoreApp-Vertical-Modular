import { Router } from 'express';
import {
  listarEstoque,
  obterEstoqueProduto,
  criarOuAtualizarEstoque,
  movimentarEstoque,
  listarMovimentacoes
} from '../controllers/estoqueController';

const router = Router();

// GET /api/estoque - Listar estoque com paginação
router.get('/', listarEstoque);

// GET /api/estoque/movimentacoes - Listar movimentações de estoque
router.get('/movimentacoes', listarMovimentacoes);

// GET /api/estoque/produto/:produtoId - Obter estoque por produto
router.get('/produto/:produtoId', obterEstoqueProduto);

// POST /api/estoque - Criar ou atualizar estoque
router.post('/', criarOuAtualizarEstoque);

// POST /api/estoque/movimentar - Movimentar estoque (entrada/saída/ajuste)
router.post('/movimentar', movimentarEstoque);

export default router;