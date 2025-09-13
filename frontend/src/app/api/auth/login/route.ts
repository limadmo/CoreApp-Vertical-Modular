/**
 * API Route: Login - Next.js 15
 * Autenticação JWT com PostgreSQL
 */
import { NextRequest, NextResponse } from 'next/server';
import jwt from 'jsonwebtoken';

// Mock data até conectarmos com PostgreSQL
const MOCK_USERS = [
  {
    id: '1',
    nome: 'Admin Padaria',
    login: 'AB12345C',
    senha: 'A1234B',
    role: {
      nome: 'admin',
      permissoes: ['clientes:read', 'clientes:write', 'vendas:read', 'vendas:write'],
    },
  },
  {
    id: '2',
    nome: 'Vendedor Padaria',
    login: 'VE67890D',
    senha: 'V5678E',
    role: {
      nome: 'vendedor',
      permissoes: ['clientes:read', 'vendas:read', 'vendas:write'],
    },
  },
];

const JWT_SECRET = process.env.JWT_SECRET || 'coreapp-jwt-secret-dev';
const TENANT_ID = process.env.TENANT_ID || 'padaria-demo';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const { login, senha } = body;

    // Validação básica
    if (!login || !senha) {
      return NextResponse.json(
        { error: 'Login e senha são obrigatórios' },
        { status: 400 }
      );
    }

    console.log('🔐 Tentativa login Next.js:', { login });

    // Buscar usuário (mock por enquanto)
    const usuario = MOCK_USERS.find(u => u.login === login && u.senha === senha);
    
    if (!usuario) {
      console.log('❌ Login inválido:', { login });
      return NextResponse.json(
        { error: 'Credenciais inválidas' },
        { status: 401 }
      );
    }

    // Gerar tokens JWT
    const accessTokenPayload = {
      userId: usuario.id,
      login: usuario.login,
      nome: usuario.nome,
      role: usuario.role.nome,
      permissoes: usuario.role.permissoes,
      tenantId: TENANT_ID,
    };

    const refreshTokenPayload = {
      userId: usuario.id,
      tenantId: TENANT_ID,
      type: 'refresh',
    };

    const accessToken = jwt.sign(accessTokenPayload, JWT_SECRET, {
      expiresIn: '15m',
    });

    const refreshToken = jwt.sign(refreshTokenPayload, JWT_SECRET, {
      expiresIn: '7d',
    });

    const response = {
      accessToken,
      refreshToken,
      usuario: {
        id: usuario.id,
        nome: usuario.nome,
        login: usuario.login,
        role: usuario.role,
      },
    };

    console.log('✅ Login Next.js realizado:', usuario.nome);

    return NextResponse.json(response, {
      status: 200,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  } catch (error) {
    console.error('❌ Erro no login:', error);
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
      'Access-Control-Allow-Methods': 'POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization, X-Tenant-ID',
    },
  });
}