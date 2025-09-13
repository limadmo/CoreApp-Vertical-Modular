/**
 * API Route: Clientes - Next.js 15
 * CRUD de clientes com PostgreSQL
 */
import { NextRequest, NextResponse } from 'next/server';

// Mock data até conectarmos com PostgreSQL
const MOCK_CLIENTES = [
  {
    id: '1',
    nome: 'João Silva',
    email: 'joao@email.com',
    telefone: '(11) 98765-4321',
    cpf: '123.456.789-00',
    endereco: {
      logradouro: 'Rua das Flores, 123',
      bairro: 'Centro',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01234-567',
    },
    dataCadastro: '2024-01-15T10:30:00Z',
    ativo: true,
    observacoes: 'Cliente preferencial',
  },
  {
    id: '2',
    nome: 'Maria Oliveira',
    email: 'maria@email.com',
    telefone: '(11) 99888-7766',
    cpf: '987.654.321-00',
    endereco: {
      logradouro: 'Av. Principal, 456',
      bairro: 'Vila Nova',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '09876-543',
    },
    dataCadastro: '2024-02-20T14:15:00Z',
    ativo: true,
    observacoes: 'Compra pães artesanais',
  },
  {
    id: '3',
    nome: 'Pedro Santos',
    email: 'pedro@email.com',
    telefone: '(11) 91111-2222',
    cpf: '456.789.123-00',
    endereco: {
      logradouro: 'Rua da Padaria, 789',
      bairro: 'Jardim',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '54321-098',
    },
    dataCadastro: '2024-03-10T09:45:00Z',
    ativo: true,
    observacoes: 'Cliente corporativo',
  },
];

export async function GET(request: NextRequest) {
  try {
    const { searchParams } = new URL(request.url);
    const page = parseInt(searchParams.get('page') || '1');
    const pageSize = parseInt(searchParams.get('pageSize') || '10');
    const search = searchParams.get('search') || '';

    console.log('🔍 Buscando clientes Next.js:', { page, pageSize, search });

    // Filtrar por busca se fornecida
    let filteredClientes = MOCK_CLIENTES;
    if (search) {
      filteredClientes = MOCK_CLIENTES.filter(cliente => 
        cliente.nome.toLowerCase().includes(search.toLowerCase()) ||
        cliente.email.toLowerCase().includes(search.toLowerCase()) ||
        cliente.cpf.includes(search)
      );
    }

    // Paginação
    const total = filteredClientes.length;
    const totalPages = Math.ceil(total / pageSize);
    const offset = (page - 1) * pageSize;
    const clientes = filteredClientes.slice(offset, offset + pageSize);

    const response = {
      data: clientes,
      pagination: {
        page,
        pageSize,
        total,
        totalPages,
        hasNext: page < totalPages,
        hasPrevious: page > 1,
      },
      meta: {
        search,
        timestamp: new Date().toISOString(),
      },
    };

    console.log('✅ Clientes encontrados:', clientes.length);

    return NextResponse.json(response, {
      status: 200,
      headers: {
        'Content-Type': 'application/json',
        'X-Total-Count': total.toString(),
      },
    });
  } catch (error) {
    console.error('❌ Erro ao buscar clientes:', error);
    return NextResponse.json(
      { error: 'Erro interno do servidor' },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const { nome, email, telefone, cpf, endereco, observacoes } = body;

    // Validação básica
    if (!nome || !email || !telefone) {
      return NextResponse.json(
        { error: 'Nome, email e telefone são obrigatórios' },
        { status: 400 }
      );
    }

    console.log('➕ Criando cliente Next.js:', { nome, email });

    // Criar novo cliente (mock)
    const novoCliente = {
      id: (MOCK_CLIENTES.length + 1).toString(),
      nome,
      email,
      telefone,
      cpf: cpf || '',
      endereco: endereco || {},
      dataCadastro: new Date().toISOString(),
      ativo: true,
      observacoes: observacoes || '',
    };

    // Adicionar à lista mock
    MOCK_CLIENTES.push(novoCliente);

    console.log('✅ Cliente criado:', novoCliente.id);

    return NextResponse.json(novoCliente, {
      status: 201,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  } catch (error) {
    console.error('❌ Erro ao criar cliente:', error);
    return NextResponse.json(
      { error: 'Erro interno do servidor' },
      { status: 500 }
    );
  }
}

export async function OPTIONS(request: NextRequest) {
  return new Response(null, {
    status: 200,
    headers: {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization, X-Tenant-ID',
    },
  });
}