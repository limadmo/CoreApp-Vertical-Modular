/**
 * Exportações centralizadas de tipos CoreApp
 */

// Tipos de tenant e módulos
export type {
  Tenant,
  TenantTheme,
  TenantContextType,
  Module,
  ModuleCode,
  VerticalType,
} from './tenant';

// Tipos de clientes brasileiros
export type {
  Cliente,
  ClienteResumo,
  CriarClienteRequest,
  AtualizarClienteRequest,
  BuscarClienteRequest,
  PagedResult,
  Endereco,
  EnderecoViaCep,
  BuscarCepRequest,
  BuscarEnderecoRequest,
  CpfValidationResult,
  TelefoneValidationResult,
  CepValidationResult,
  DireitoEsquecimentoRequest,
  ClienteLgpd,
  ClienteEstatisticas,
  ClienteHistorico,
  VendaHistorico,
  FidelidadeHistorico,
  AlteracaoHistorico,
  HistoricoClienteRequest,
} from './cliente';

// Tipos comuns da aplicação
export interface ApiResponse<T = any> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T = any> {
  data: T[];
  total: number;
  page: number;
  limit: number;
  totalPages: number;
}

export interface BaseEntity {
  id: string;
  createdAt: string;
  updatedAt: string;
  tenantId: string;
}

/** Estados de loading comuns */
export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

/** Tipos de notificação */
export type NotificationType = 'success' | 'error' | 'warning' | 'info';

/** Props comuns para componentes de formulário */
export interface FormProps<T = any> {
  initialData?: T;
  onSubmit: (data: T) => Promise<void>;
  isLoading?: boolean;
  disabled?: boolean;
}

/** Props para componentes de tabela */
export interface TableProps<T = any> {
  data: T[];
  totalRecords: number;
  currentPage: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  isLoading?: boolean;
}

/** Configuração de coluna para DataTable */
export interface ColumnDef<T = any> {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string | number;
  render: (item: T) => React.ReactNode;
}