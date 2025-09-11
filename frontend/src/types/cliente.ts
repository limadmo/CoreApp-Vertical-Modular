/**
 * Tipos e interfaces para módulo de Clientes Brasileiros
 * Baseado na implementação backend CoreApp.Application.DTOs.Cliente
 */

/**
 * Interface base para endereço brasileiro
 */
export interface Endereco {
  cep?: string;
  uf?: string;
  cidade?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  codigoIBGE?: string;
  pontoReferencia?: string;
}

/**
 * Cliente completo com todos os dados brasileiros
 */
export interface Cliente {
  id: string;
  
  // === DADOS PESSOAIS ===
  nome: string;
  nomeCompleto?: string;
  nomeMae?: string;
  nomePai?: string;
  dataNascimento?: string;
  genero?: 'M' | 'F' | 'O' | 'N';
  estadoCivil?: 'Solteiro' | 'Casado' | 'Divorciado' | 'Viúvo' | 'União Estável';
  profissao?: string;
  nacionalidade?: string;
  
  // === DOCUMENTOS BRASILEIROS ===
  cpf?: string;
  cpfValidado: boolean;
  rg?: string;
  rgOrgaoExpedidor?: string;
  rgUFExpedicao?: string;
  rgDataExpedicao?: string;
  
  // === CONTATO ===
  email?: string;
  emailValidado: boolean;
  telefoneCelular?: string;
  telefoneFixo?: string;
  whatsApp?: string;
  
  // === ENDEREÇO ===
  endereco?: Endereco;
  
  // === DADOS COMERCIAIS ===
  categoriaCliente: 'Bronze' | 'Regular' | 'Premium' | 'VIP';
  limiteCredito: number;
  descontoPadrao: number;
  valorTotalCompras: number;
  quantidadeCompras: number;
  dataUltimaCompra?: string;
  pontuacaoFidelidade: number;
  
  // === OBSERVAÇÕES ===
  observacoes?: string;
  preferenciaContato?: string;
  restricoesDietarias?: string;
  alergias?: string;
  
  // === LGPD COMPLIANCE ===
  consentimentoColeta: boolean;
  dataConsentimento?: string;
  ipConsentimento?: string;
  finalidadeColeta?: string;
  consentimentoMarketing: boolean;
  consentimentoCompartilhamento: boolean;
  direitoEsquecimento: boolean;
  dataEsquecimento?: string;
  motivoEsquecimento?: string;
  
  // === SISTEMA ===
  ativo: boolean;
  dataCadastro: string;
  dataUltimaAtualizacao: string;
  tenantId: string;
}

/**
 * DTO para resumo de cliente (listagem)
 */
export interface ClienteResumo {
  id: string;
  nome: string;
  email?: string;
  cpf?: string;
  telefoneCelular?: string;
  categoriaCliente: string;
  valorTotalCompras: number;
  dataUltimaCompra?: string;
  ativo: boolean;
  consentimentoColeta: boolean;
}

/**
 * Request para criação de cliente
 */
export interface CriarClienteRequest {
  // === DADOS PESSOAIS OBRIGATÓRIOS ===
  nome: string;
  
  // === DADOS PESSOAIS OPCIONAIS ===
  nomeCompleto?: string;
  nomeMae?: string;
  nomePai?: string;
  dataNascimento?: string;
  genero?: 'M' | 'F' | 'O' | 'N';
  estadoCivil?: string;
  profissao?: string;
  nacionalidade?: string;
  
  // === DOCUMENTOS ===
  cpf?: string;
  rg?: string;
  rgOrgaoExpedidor?: string;
  rgUFExpedicao?: string;
  rgDataExpedicao?: string;
  
  // === CONTATO ===
  email?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  whatsApp?: string;
  
  // === ENDEREÇO ===
  endereco?: Endereco;
  
  // === COMERCIAL ===
  categoriaCliente?: string;
  limiteCredito?: number;
  descontoPadrao?: number;
  
  // === OBSERVAÇÕES ===
  observacoes?: string;
  preferenciaContato?: string;
  restricoesDietarias?: string;
  alergias?: string;
  
  // === LGPD ===
  consentimentoColeta: boolean;
  consentimentoMarketing?: boolean;
  consentimentoCompartilhamento?: boolean;
  finalidadeColeta?: string;
}

/**
 * Request para atualização de cliente
 */
export interface AtualizarClienteRequest {
  nome?: string;
  nomeCompleto?: string;
  nomeMae?: string;
  nomePai?: string;
  dataNascimento?: string;
  genero?: 'M' | 'F' | 'O' | 'N';
  estadoCivil?: string;
  profissao?: string;
  nacionalidade?: string;
  cpf?: string;
  rg?: string;
  rgOrgaoExpedidor?: string;
  rgUFExpedicao?: string;
  rgDataExpedicao?: string;
  email?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  whatsApp?: string;
  endereco?: Endereco;
  categoriaCliente?: string;
  limiteCredito?: number;
  descontoPadrao?: number;
  observacoes?: string;
  preferenciaContato?: string;
  restricoesDietarias?: string;
  alergias?: string;
}

/**
 * Request para busca de clientes
 */
export interface BuscarClienteRequest {
  termo?: string;
  nome?: string;
  cpf?: string;
  email?: string;
  telefone?: string;
  categoriaCliente?: string;
  uf?: string;
  cidade?: string;
  dataCadastroInicial?: string;
  dataCadastroFinal?: string;
  valorMinimoCompras?: number;
  valorMaximoCompras?: number;
  apenasAtivos?: boolean;
  apenasComConsentimento?: boolean;
  
  // Paginação
  pagina: number;
  tamanhoPagina: number;
  
  // Ordenação
  ordenarPor?: string;
  direcaoOrdenacao: 'ASC' | 'DESC';
}

/**
 * Resultado paginado de clientes
 */
export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

/**
 * Dados de endereço via ViaCEP
 */
export interface EnderecoViaCep {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  localidade: string;
  uf: string;
  ibge: string;
  gia?: string;
  ddd?: string;
  siafi?: string;
  erro?: boolean;
}

/**
 * Request para busca de CEP
 */
export interface BuscarCepRequest {
  cep: string;
}

/**
 * Request para busca reversa de endereço
 */
export interface BuscarEnderecoRequest {
  uf: string;
  cidade: string;
  logradouro: string;
}

/**
 * Resultado de validação de CPF
 */
export interface CpfValidationResult {
  isValid: boolean;
  formattedCpf: string;
  message: string;
  cleanCpf: string;
}

/**
 * Resultado de validação de telefone
 */
export interface TelefoneValidationResult {
  isValid: boolean;
  formattedPhone: string;
  phoneType: 'CELULAR' | 'FIXO' | 'INVALIDO';
  ddd: string;
  region: string;
  message: string;
}

/**
 * Resultado de validação de CEP
 */
export interface CepValidationResult {
  isValid: boolean;
  formattedCep: string;
  endereco?: EnderecoViaCep;
  message: string;
}

/**
 * Request para direito ao esquecimento (LGPD)
 */
export interface DireitoEsquecimentoRequest {
  motivo: string;
  confirmacao: boolean;
}

/**
 * Relatório LGPD do cliente
 */
export interface ClienteLgpd {
  clienteId: string;
  nome: string;
  consentimentoColeta: boolean;
  dataConsentimento?: string;
  ipConsentimento?: string;
  finalidadeColeta?: string;
  consentimentoMarketing: boolean;
  consentimentoCompartilhamento: boolean;
  direitoEsquecimento: boolean;
  dataEsquecimento?: string;
  motivoEsquecimento?: string;
  dadosColetados: string[];
  basesLegais: string[];
}

/**
 * Estatísticas de clientes
 */
export interface ClienteEstatisticas {
  totalClientes: number;
  clientesAtivos: number;
  clientesInativos: number;
  clientesComConsentimento: number;
  clientesSemConsentimento: number;
  clientesPorCategoria: Record<string, number>;
  clientesPorUF: Record<string, number>;
  clientesPorMes: Record<string, number>;
  valorTotalCompras: number;
  ticketMedioGeral: number;
  totalPontosFidelidade: number;
}

/**
 * Histórico do cliente
 */
export interface ClienteHistorico {
  clienteId: string;
  nomeCliente: string;
  vendas: VendaHistorico[];
  fidelidade: FidelidadeHistorico[];
  alteracoes: AlteracaoHistorico[];
  valorTotalCompras: number;
  quantidadeCompras: number;
  ticketMedio: number;
  pontuacaoFidelidade: number;
}

export interface VendaHistorico {
  id: string;
  data: string;
  valor: number;
  desconto: number;
  quantidadeItens: number;
  status: string;
}

export interface FidelidadeHistorico {
  data: string;
  tipo: 'GANHO' | 'RESGATE' | 'BONUS';
  pontos: number;
  descricao: string;
}

export interface AlteracaoHistorico {
  data: string;
  campo: string;
  valorAnterior: string;
  valorNovo: string;
  usuario: string;
}

/**
 * Request para histórico do cliente
 */
export interface HistoricoClienteRequest {
  dataInicial?: string;
  dataFinal?: string;
  limite: number;
  incluirVendas: boolean;
  incluirFidelidade: boolean;
  incluirAlteracoes: boolean;
}