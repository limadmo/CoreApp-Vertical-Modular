/**
 * Gerador de clientes brasileiros usando Faker.js
 * Dados realistas com locale pt_BR e validações brasileiras
 */
import { faker } from '@faker-js/faker';
import { Cliente, ClienteResumo } from '../types/cliente';

// Configurar locale português brasileiro (Faker.js v10+ usa faker.locale)
faker.locale = 'pt_BR';

/**
 * Algoritmo para gerar CPF válido
 */
function gerarCpfValido(): string {
  const digits = [];
  
  // Gera os primeiros 9 dígitos
  for (let i = 0; i < 9; i++) {
    digits.push(faker.number.int({ min: 0, max: 9 }));
  }
  
  // Calcula primeiro dígito verificador
  let sum = 0;
  for (let i = 0; i < 9; i++) {
    sum += digits[i] * (10 - i);
  }
  let remainder = sum % 11;
  digits.push(remainder < 2 ? 0 : 11 - remainder);
  
  // Calcula segundo dígito verificador
  sum = 0;
  for (let i = 0; i < 10; i++) {
    sum += digits[i] * (11 - i);
  }
  remainder = sum % 11;
  digits.push(remainder < 2 ? 0 : 11 - remainder);
  
  return digits.join('');
}

/**
 * Gera telefone celular brasileiro válido por estado
 */
function gerarTelefoneCelular(uf: string): string {
  // DDDs por estado
  const dddPorUF: Record<string, number[]> = {
    'SP': [11, 12, 13, 14, 15, 16, 17, 18, 19],
    'RJ': [21, 22, 24],
    'MG': [31, 32, 33, 34, 35, 37, 38],
    'RS': [51, 53, 54, 55],
    'PR': [41, 42, 43, 44, 45, 46],
    'SC': [47, 48, 49],
    'GO': [62, 64],
    'MS': [67],
    'MT': [65, 66],
    'DF': [61],
    'BA': [71, 73, 74, 75, 77],
    'PE': [81, 87],
    'CE': [85, 88],
    'PA': [91, 93, 94],
    'MA': [98, 99],
    'PB': [83],
    'RN': [84],
    'AL': [82],
    'SE': [79],
    'PI': [86, 89],
    'AC': [68],
    'AM': [92, 97],
    'RO': [69],
    'RR': [95],
    'AP': [96],
    'TO': [63]
  };
  
  const ddds = dddPorUF[uf] || [11];
  const ddd = faker.helpers.arrayElement(ddds);
  
  // Gera número de celular (9 dígitos começando com 9)
  const numero = '9' + faker.string.numeric(8);
  
  return `${ddd}${numero}`;
}

/**
 * Gera endereço brasileiro válido por estado
 */
function gerarEnderecoValido(uf: string) {
  // CEPs base por estado (primeiros 2 dígitos)
  const cepBasePorUF: Record<string, string> = {
    'SP': '01000-000',
    'RJ': '20000-000', 
    'MG': '30000-000',
    'RS': '90000-000',
    'PR': '80000-000',
    'SC': '88000-000',
    'GO': '72000-000',
    'MS': '79000-000',
    'MT': '78000-000',
    'DF': '70000-000',
    'BA': '40000-000',
    'PE': '50000-000',
    'CE': '60000-000',
    'PA': '66000-000',
    'MA': '65000-000',
    'PB': '58000-000',
    'RN': '59000-000',
    'AL': '57000-000',
    'SE': '49000-000',
    'PI': '64000-000',
    'AC': '69900-000',
    'AM': '69000-000',
    'RO': '76000-000',
    'RR': '69300-000',
    'AP': '68900-000',
    'TO': '77000-000'
  };

  const cepBase = cepBasePorUF[uf] || '01000-000';
  const prefix = cepBase.substring(0, 2);
  const cep = prefix + faker.string.numeric(3) + '-' + faker.string.numeric(3);
  
  const logradouros = [
    'Rua das Flores', 'Avenida Principal', 'Rua São João', 'Avenida Paulista',
    'Rua do Comércio', 'Avenida Central', 'Rua XV de Novembro', 'Avenida Brasil',
    'Rua da Paz', 'Avenida Santos Dumont', 'Rua José da Silva', 'Avenida Getúlio Vargas'
  ];

  const bairros = [
    'Centro', 'Vila Nova', 'Jardim das Rosas', 'Bairro Alto',
    'Vila São Pedro', 'Jardim América', 'Bela Vista', 'Campo Grande',
    'Cidade Nova', 'Vila Mariana', 'Copacabana', 'Ipanema'
  ];

  const cidades: Record<string, string[]> = {
    'SP': ['São Paulo', 'Santos', 'Campinas', 'São Bernardo do Campo'],
    'RJ': ['Rio de Janeiro', 'Niterói', 'Nova Iguaçu', 'Duque de Caxias'],
    'MG': ['Belo Horizonte', 'Contagem', 'Uberlândia', 'Juiz de Fora'],
    'RS': ['Porto Alegre', 'Caxias do Sul', 'Pelotas', 'Santa Maria'],
    'PR': ['Curitiba', 'Londrina', 'Maringá', 'Foz do Iguaçu'],
    'SC': ['Florianópolis', 'Joinville', 'Blumenau', 'São José'],
    'GO': ['Goiânia', 'Aparecida de Goiânia', 'Anápolis', 'Rio Verde'],
    'MS': ['Campo Grande', 'Dourados', 'Três Lagoas', 'Corumbá'],
    'MT': ['Cuiabá', 'Várzea Grande', 'Rondonópolis', 'Sinop'],
    'DF': ['Brasília'],
    'BA': ['Salvador', 'Feira de Santana', 'Vitória da Conquista', 'Camaçari'],
    'PE': ['Recife', 'Jaboatão dos Guararapes', 'Olinda', 'Caruaru'],
    'CE': ['Fortaleza', 'Caucaia', 'Juazeiro do Norte', 'Sobral'],
    'PA': ['Belém', 'Ananindeua', 'Santarém', 'Marabá'],
    'MA': ['São Luís', 'Imperatriz', 'Caxias', 'Timon'],
    'PB': ['João Pessoa', 'Campina Grande', 'Santa Rita', 'Patos'],
    'RN': ['Natal', 'Mossoró', 'Parnamirim', 'São Gonçalo do Amarante'],
    'AL': ['Maceió', 'Arapiraca', 'Palmeira dos Índios', 'Rio Largo'],
    'SE': ['Aracaju', 'Nossa Senhora do Socorro', 'Lagarto', 'Itabaiana'],
    'PI': ['Teresina', 'Parnaíba', 'Picos', 'Piripiri'],
    'AC': ['Rio Branco', 'Cruzeiro do Sul', 'Sena Madureira', 'Tarauacá'],
    'AM': ['Manaus', 'Parintins', 'Itacoatiara', 'Manacapuru'],
    'RO': ['Porto Velho', 'Ji-Paraná', 'Ariquemes', 'Vilhena'],
    'RR': ['Boa Vista', 'Rorainópolis', 'Caracaraí', 'Alto Alegre'],
    'AP': ['Macapá', 'Santana', 'Laranjal do Jari', 'Oiapoque'],
    'TO': ['Palmas', 'Araguaína', 'Gurupi', 'Porto Nacional']
  };

  return {
    cep,
    logradouro: faker.helpers.arrayElement(logradouros),
    numero: faker.number.int({ min: 1, max: 9999 }).toString(),
    complemento: faker.datatype.boolean({ probability: 0.3 }) ? 
      faker.helpers.arrayElement(['Apto 101', 'Casa 2', 'Sala 205', 'Bloco A', 'Fundos']) : '',
    bairro: faker.helpers.arrayElement(bairros),
    cidade: faker.helpers.arrayElement(cidades[uf] || ['São Paulo']),
    uf
  };
}

/**
 * Gera um cliente brasileiro realista
 */
function gerarClienteBrasileiro(id: number): Cliente {
  const genero = faker.person.sexType() as 'male' | 'female';
  const estadosBrasileiros = [
    'SP', 'RJ', 'MG', 'RS', 'PR', 'SC', 'GO', 'MS', 'MT', 'DF',
    'BA', 'PE', 'CE', 'PA', 'MA', 'PB', 'RN', 'AL', 'SE', 'PI',
    'AC', 'AM', 'RO', 'RR', 'AP', 'TO'
  ];
  
  const uf = faker.helpers.arrayElement(estadosBrasileiros);
  const endereco = gerarEnderecoValido(uf);
  
  const categorias = ['Bronze', 'Regular', 'Premium', 'VIP'] as const;
  const estadosCivis = ['Solteiro', 'Casado', 'Divorciado', 'Viúvo', 'União Estável'] as const;
  
  const profissoesBrasileiras = [
    'Advogado', 'Médico', 'Engenheiro', 'Professor', 'Enfermeiro',
    'Contador', 'Arquiteto', 'Dentista', 'Psicólogo', 'Veterinário',
    'Farmacêutico', 'Fisioterapeuta', 'Nutricionista', 'Jornalista',
    'Designer', 'Programador', 'Comerciante', 'Empresário',
    'Funcionário Público', 'Estudante'
  ];

  const categoria = faker.helpers.arrayElement(categorias);
  const limiteCredito = {
    'Bronze': faker.number.float({ min: 500, max: 2000 }),
    'Regular': faker.number.float({ min: 2000, max: 5000 }),
    'Premium': faker.number.float({ min: 5000, max: 10000 }),
    'VIP': faker.number.float({ min: 10000, max: 20000 })
  }[categoria];

  const valorTotalCompras = faker.number.float({ min: 0, max: limiteCredito * 2 });
  const quantidadeCompras = faker.number.int({ min: 0, max: 200 });

  return {
    id: id.toString(),
    nome: faker.person.fullName({ sex: genero }),
    cpf: gerarCpfValido(),
    cpfValidado: faker.datatype.boolean({ probability: 0.9 }),
    rg: faker.string.numeric(9),
    rgOrgaoExpedidor: faker.helpers.arrayElement(['SSP', 'IFP', 'DETRAN']),
    rgUFExpedicao: uf,
    telefoneCelular: gerarTelefoneCelular(uf),
    telefoneValidado: faker.datatype.boolean({ probability: 0.8 }),
    email: faker.internet.email({ 
      firstName: faker.person.firstName(), 
      lastName: faker.person.lastName(),
      provider: faker.helpers.arrayElement(['gmail.com', 'hotmail.com', 'yahoo.com.br', 'outlook.com'])
    }).toLowerCase(),
    emailValidado: faker.datatype.boolean({ probability: 0.85 }),
    dataNascimento: faker.date.birthdate({ min: 18, max: 80, mode: 'age' }).toISOString().split('T')[0],
    genero: genero === 'male' ? 'M' as const : 'F' as const,
    profissao: faker.helpers.arrayElement(profissoesBrasileiras),
    estadoCivil: faker.helpers.arrayElement(estadosCivis),
    categoriaCliente: categoria,
    ativo: faker.datatype.boolean({ probability: 0.95 }),
    endereco,
    limiteCredito: Math.round(limiteCredito * 100) / 100,
    descontoPadrao: faker.number.int({ min: 0, max: 15 }),
    valorTotalCompras: Math.round(valorTotalCompras * 100) / 100,
    quantidadeCompras,
    dataUltimaCompra: quantidadeCompras > 0 ? 
      faker.date.recent({ days: 365 }).toISOString().split('T')[0] : undefined,
    pontuacaoFidelidade: Math.round(valorTotalCompras / 10),
    observacoes: faker.datatype.boolean({ probability: 0.4 }) ? 
      faker.helpers.arrayElement([
        'Cliente frequente, sempre pontual',
        'Preferência por produtos orgânicos',
        'Compras em grandes quantidades',
        'Cliente fiel, recomenda para amigos',
        'Interesse em produtos premium',
        'Compras sazonais, eventos especiais'
      ]) : undefined,
    consentimentoColeta: faker.datatype.boolean({ probability: 0.9 }),
    consentimentoMarketing: faker.datatype.boolean({ probability: 0.6 }),
    consentimentoCompartilhamento: faker.datatype.boolean({ probability: 0.2 }),
    direitoEsquecimento: faker.datatype.boolean({ probability: 0.05 }),
    finalidadeColeta: faker.helpers.arrayElement([
      'Gestão de relacionamento e vendas',
      'Atendimento ao cliente e suporte',
      'Marketing personalizado',
      'Controle de estoque e demanda',
      'Análise de comportamento de compra'
    ]),
    dataConsentimento: faker.date.past({ years: 2 }).toISOString().split('T')[0],
    ipConsentimento: faker.internet.ipv4(),
    dataCadastro: faker.date.past({ years: 3 }).toISOString().split('T')[0],
    dataUltimaAtualizacao: faker.date.recent({ days: 90 }).toISOString().split('T')[0],
    tenantId: 'demo-padaria-123'
  };
}

/**
 * Gera lista de clientes brasileiros
 */
export function gerarClientesBrasileiros(quantidade: number = 20): Cliente[] {
  const seed = parseInt(import.meta.env.VITE_FAKER_SEED || '123');
  faker.seed(seed); // Seed para dados consistentes durante desenvolvimento
  
  const clientes: Cliente[] = [];
  for (let i = 1; i <= quantidade; i++) {
    clientes.push(gerarClienteBrasileiro(i));
  }
  
  return clientes;
}

/**
 * Converte clientes para resumo
 */
export function clientesToResumo(clientes: Cliente[]): ClienteResumo[] {
  return clientes.map(cliente => ({
    id: cliente.id,
    nome: cliente.nome,
    cpf: cliente.cpf,
    telefoneCelular: cliente.telefoneCelular,
    email: cliente.email,
    categoriaCliente: cliente.categoriaCliente,
    valorTotalCompras: cliente.valorTotalCompras,
    dataUltimaCompra: cliente.dataUltimaCompra,
    ativo: cliente.ativo,
    consentimentoColeta: cliente.consentimentoColeta
  }));
}

/**
 * Calcula estatísticas dos clientes gerados
 */
export function calcularEstatisticas(clientes: Cliente[]) {
  const clientesAtivos = clientes.filter(c => c.ativo);
  const totalClientes = clientes.length;
  const clientesAtivosCount = clientesAtivos.length;
  const valorTotalVendas = clientes.reduce((acc, c) => acc + c.valorTotalCompras, 0);
  const ticketMedio = valorTotalVendas / totalClientes;

  return {
    totalClientes,
    clientesAtivos: clientesAtivosCount,
    valorTotalVendas,
    ticketMedio,
    crescimentoMensal: faker.number.float({ min: 5, max: 20 }),
    clientesInativos: totalClientes - clientesAtivosCount,
    categorias: {
      VIP: clientes.filter(c => c.categoriaCliente === 'VIP').length,
      Premium: clientes.filter(c => c.categoriaCliente === 'Premium').length,
      Regular: clientes.filter(c => c.categoriaCliente === 'Regular').length,
      Bronze: clientes.filter(c => c.categoriaCliente === 'Bronze').length,
    },
    distribuicaoCategoria: [
      { categoria: 'VIP', quantidade: clientes.filter(c => c.categoriaCliente === 'VIP').length },
      { categoria: 'Premium', quantidade: clientes.filter(c => c.categoriaCliente === 'Premium').length },
      { categoria: 'Regular', quantidade: clientes.filter(c => c.categoriaCliente === 'Regular').length },
      { categoria: 'Bronze', quantidade: clientes.filter(c => c.categoriaCliente === 'Bronze').length }
    ],
    topCidades: clientes.reduce((acc, c) => {
      const existing = acc.find(item => item.uf === c.endereco?.uf && item.cidade === c.endereco?.cidade);
      if (existing) {
        existing.quantidade++;
      } else if (c.endereco?.uf && c.endereco?.cidade) {
        acc.push({ uf: c.endereco.uf, cidade: c.endereco.cidade, quantidade: 1 });
      }
      return acc;
    }, [] as Array<{ uf: string; cidade: string; quantidade: number }>)
      .sort((a, b) => b.quantidade - a.quantidade)
      .slice(0, 5)
  };
}

// Gerar dados iniciais
export const clientesGerados = gerarClientesBrasileiros(20);
export default clientesGerados;