/**
 * Store Zustand para gestão de produtos PADARIA
 * State management especializado para vertical PADARIA
 */
import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { apiClient } from '@services/api';

export interface ProdutoPadaria {
  id: string;
  nome: string;
  descricao?: string;
  preco: number;
  categoria: string;
  sku?: string;
  ativo: boolean;
  
  // Propriedades específicas PADARIA (JSON)
  verticalProperties: {
    validadeHoras: number;
    alergenos: string[];
    pesoMedio: number;
    ingredientesPrincipais: string[];
    temperaturaArmazenamento?: 'AMBIENTE' | 'REFRIGERADO' | 'CONGELADO';
    tipoMassa?: 'DOCE' | 'SALGADA' | 'NEUTRA';
    dificuldadePreparo: 'BAIXA' | 'MEDIA' | 'ALTA';
  };
  
  // Campos de auditoria
  createdAt: string;
  updatedAt: string;
  tenantId: string;
}

export interface ProdutoFormData {
  nome: string;
  descricao?: string;
  preco: number;
  categoria: string;
  sku?: string;
  ativo: boolean;
  validadeHoras: number;
  alergenos: string[];
  pesoMedio: number;
  ingredientesPrincipais: string[];
  temperaturaArmazenamento?: string;
  tipoMassa?: string;
  dificuldadePreparo: string;
}

interface ProdutosState {
  // Estado
  produtos: ProdutoPadaria[];
  isLoading: boolean;
  error: string | null;
  
  // Paginação
  currentPage: number;
  totalPages: number;
  totalRecords: number;
  pageSize: number;
  
  // Filtros
  searchQuery: string;
  categoriaFilter: string | null;
  alergenosFilter: string[];
  apenasVencendo: boolean;
  
  // Actions
  fetchProdutos: (page?: number, limit?: number) => Promise<void>;
  createProduto: (data: ProdutoFormData) => Promise<ProdutoPadaria>;
  updateProduto: (id: string, data: Partial<ProdutoFormData>) => Promise<ProdutoPadaria>;
  deleteProduto: (id: string) => Promise<void>;
  
  // Filtros
  setSearchQuery: (query: string) => void;
  setCategoriaFilter: (categoria: string | null) => void;
  setAlergenosFilter: (alergenos: string[]) => void;
  toggleApenasVencendo: () => void;
  
  // Paginação
  setPage: (page: number) => void;
  
  // Utilitários
  getProdutoById: (id: string) => ProdutoPadaria | undefined;
  getProdutosPorCategoria: (categoria: string) => ProdutoPadaria[];
  getProdutosVencendo: (horasLimite?: number) => ProdutoPadaria[];
  
  // Reset
  reset: () => void;
}

const initialState = {
  produtos: [],
  isLoading: false,
  error: null,
  currentPage: 1,
  totalPages: 0,
  totalRecords: 0,
  pageSize: 10,
  searchQuery: '',
  categoriaFilter: null,
  alergenosFilter: [],
  apenasVencendo: false,
};

/**
 * Store de produtos especializado para PADARIA
 * Implementa operações CRUD + filtros específicos do vertical
 */
export const useProdutosStore = create<ProdutosState>()(
  devtools(
    persist(
      (set, get) => ({
        ...initialState,

        /**
         * Buscar produtos com filtros e paginação
         */
        fetchProdutos: async (page = 1, limit = 10) => {
          set({ isLoading: true, error: null });
          
          try {
            const state = get();
            const params: any = {
              page,
              limit,
            };

            // Aplicar filtros
            if (state.searchQuery) {
              params.search = state.searchQuery;
            }
            if (state.categoriaFilter) {
              params.categoria = state.categoriaFilter;
            }
            if (state.alergenosFilter.length > 0) {
              params.alergenos = state.alergenosFilter.join(',');
            }
            if (state.apenasVencendo) {
              params.apenasVencendo = true;
            }

            const response = await apiClient.getPaginated<ProdutoPadaria>(
              '/produtos',
              params
            );

            set({
              produtos: response.data,
              currentPage: response.page,
              totalPages: response.totalPages,
              totalRecords: response.total,
              pageSize: response.limit,
              isLoading: false,
            });
          } catch (error) {
            set({
              error: error instanceof Error ? error.message : 'Erro ao buscar produtos',
              isLoading: false,
            });
          }
        },

        /**
         * Criar novo produto PADARIA
         */
        createProduto: async (data: ProdutoFormData) => {
          set({ isLoading: true, error: null });

          try {
            const produtoData = {
              nome: data.nome,
              descricao: data.descricao,
              preco: data.preco,
              categoria: data.categoria,
              sku: data.sku,
              ativo: data.ativo,
              verticalProperties: {
                validadeHoras: data.validadeHoras,
                alergenos: data.alergenos,
                pesoMedio: data.pesoMedio,
                ingredientesPrincipais: data.ingredientesPrincipais,
                temperaturaArmazenamento: data.temperaturaArmazenamento,
                tipoMassa: data.tipoMassa,
                dificuldadePreparo: data.dificuldadePreparo,
              },
            };

            const novoProduto = await apiClient.post<ProdutoPadaria>('/produtos', produtoData);

            // Atualizar lista local
            set(state => ({
              produtos: [novoProduto, ...state.produtos],
              totalRecords: state.totalRecords + 1,
              isLoading: false,
            }));

            return novoProduto;
          } catch (error) {
            set({
              error: error instanceof Error ? error.message : 'Erro ao criar produto',
              isLoading: false,
            });
            throw error;
          }
        },

        /**
         * Atualizar produto existente
         */
        updateProduto: async (id: string, data: Partial<ProdutoFormData>) => {
          set({ isLoading: true, error: null });

          try {
            const updateData: any = { ...data };
            
            // Mover propriedades verticais para o campo correto
            if (data.validadeHoras || data.alergenos || data.pesoMedio) {
              updateData.verticalProperties = {};
              
              if (data.validadeHoras) updateData.verticalProperties.validadeHoras = data.validadeHoras;
              if (data.alergenos) updateData.verticalProperties.alergenos = data.alergenos;
              if (data.pesoMedio) updateData.verticalProperties.pesoMedio = data.pesoMedio;
              if (data.ingredientesPrincipais) updateData.verticalProperties.ingredientesPrincipais = data.ingredientesPrincipais;
              if (data.temperaturaArmazenamento) updateData.verticalProperties.temperaturaArmazenamento = data.temperaturaArmazenamento;
              if (data.tipoMassa) updateData.verticalProperties.tipoMassa = data.tipoMassa;
              if (data.dificuldadePreparo) updateData.verticalProperties.dificuldadePreparo = data.dificuldadePreparo;
              
              // Remover do nível raiz
              delete updateData.validadeHoras;
              delete updateData.alergenos;
              delete updateData.pesoMedio;
              delete updateData.ingredientesPrincipais;
              delete updateData.temperaturaArmazenamento;
              delete updateData.tipoMassa;
              delete updateData.dificuldadePreparo;
            }

            const produtoAtualizado = await apiClient.put<ProdutoPadaria>(`/produtos/${id}`, updateData);

            // Atualizar lista local
            set(state => ({
              produtos: state.produtos.map(p => p.id === id ? produtoAtualizado : p),
              isLoading: false,
            }));

            return produtoAtualizado;
          } catch (error) {
            set({
              error: error instanceof Error ? error.message : 'Erro ao atualizar produto',
              isLoading: false,
            });
            throw error;
          }
        },

        /**
         * Excluir produto
         */
        deleteProduto: async (id: string) => {
          set({ isLoading: true, error: null });

          try {
            await apiClient.delete(`/produtos/${id}`);

            // Remover da lista local
            set(state => ({
              produtos: state.produtos.filter(p => p.id !== id),
              totalRecords: state.totalRecords - 1,
              isLoading: false,
            }));
          } catch (error) {
            set({
              error: error instanceof Error ? error.message : 'Erro ao excluir produto',
              isLoading: false,
            });
            throw error;
          }
        },

        /**
         * Definir query de busca
         */
        setSearchQuery: (query: string) => {
          set({ searchQuery: query });
          // Recarregar produtos após delay
          setTimeout(() => get().fetchProdutos(1), 300);
        },

        /**
         * Definir filtro de categoria
         */
        setCategoriaFilter: (categoria: string | null) => {
          set({ categoriaFilter: categoria });
          get().fetchProdutos(1);
        },

        /**
         * Definir filtro de alérgenos
         */
        setAlergenosFilter: (alergenos: string[]) => {
          set({ alergenosFilter: alergenos });
          get().fetchProdutos(1);
        },

        /**
         * Toggle filtro apenas produtos vencendo
         */
        toggleApenasVencendo: () => {
          set(state => ({ apenasVencendo: !state.apenasVencendo }));
          get().fetchProdutos(1);
        },

        /**
         * Definir página atual
         */
        setPage: (page: number) => {
          get().fetchProdutos(page);
        },

        /**
         * Obter produto por ID
         */
        getProdutoById: (id: string) => {
          return get().produtos.find(p => p.id === id);
        },

        /**
         * Obter produtos por categoria
         */
        getProdutosPorCategoria: (categoria: string) => {
          return get().produtos.filter(p => p.categoria === categoria);
        },

        /**
         * Obter produtos próximos ao vencimento
         */
        getProdutosVencendo: (horasLimite = 2) => {
          return get().produtos.filter(p => {
            const validadeHoras = p.verticalProperties.validadeHoras;
            return validadeHoras <= horasLimite;
          });
        },

        /**
         * Reset do store
         */
        reset: () => {
          set(initialState);
        },
      }),
      {
        name: 'produtos-store',
        partialize: (state) => ({
          // Persistir apenas filtros, não os dados
          searchQuery: state.searchQuery,
          categoriaFilter: state.categoriaFilter,
          alergenosFilter: state.alergenosFilter,
          pageSize: state.pageSize,
        }),
      }
    ),
    {
      name: 'produtos-store',
    }
  )
);