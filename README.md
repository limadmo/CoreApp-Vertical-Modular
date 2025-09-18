# CoreApp - Sistema SAAS Multi-tenant (Express.js + Next.js)

![Node.js](https://img.shields.io/badge/Node.js-20.x-green.svg)
![Express.js](https://img.shields.io/badge/Express.js-4.19-blue.svg)
![Next.js](https://img.shields.io/badge/Next.js-15.5-black.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-blue.svg)
![Docker](https://img.shields.io/badge/Docker-ready-brightgreen.svg)
![Brasil](https://img.shields.io/badge/🇧🇷-100%25%20Brasileiro-green.svg)

## 🚀 Visão Geral

Sistema **SAAS multi-tenant brasileiro** com **arquitetura de verticais por composição** para gestão de comércios diversos (padarias, supermercados, farmácias, óticas, etc.). Baseado em **CoreApp base + Verticais específicos** com **SOLID principles** e **Unit of Work estado da arte**.

### ⭐ Principais Características

- **🏗️ Arquitetura de Verticais**: CoreApp genérico + especializações por setor
- **🏢 Multi-tenant**: Isolamento total de dados por loja
- **🇧🇷 100% Brasileiro**: PIX, Boleto, Mercado Pago, LGPD compliance
- **💰 Modelo Modular**: Starter + Módulos Adicionais opcionais

### 💰 Planos Comerciais

**Starter** (Inclusos)
- ✅ Produtos, Vendas, Estoque, Usuários

**Módulos Adicionais** (Opcionais)
- 👥 Clientes (CRM + Fidelidade)
- 🎁 Promoções (Descontos automáticos)
- 📊 Relatórios Básicos/Avançados
- 🔍 Auditoria (LGPD compliance)
- 🏪 Fornecedores, 📱 Mobile, 💰 Pagamentos

## 🛠️ Stack Tecnológica

### Backend (Express.js + TypeScript)
- **Framework**: Express.js 4.19.x + TypeScript 5.3.x
- **ORM**: Prisma 5.x + PostgreSQL 17
- **Database**: PostgreSQL 17 multi-tenant
- **Architecture**: Verticais + SOLID + Clean Architecture
- **Patterns**: Unit of Work + Repository + CQRS
- **Auth**: JWT + Role-based + Multi-tenant
- **Docs**: Swagger UI completo + OpenAPI 3.0

### Frontend (Next.js + Mantine)
- **Framework**: Next.js 15.5.x + React 19.x + TypeScript 5.3.x
- **UI Library**: Mantine 7.15 + Tailwind CSS 4+
- **State Management**: Zustand 4.5.0
- **Design**: Cards modernos, contraste melhorado, tipografia legível
- **Build**: Turbopack + App Router + RSC

### Infraestrutura
- **Containers**: Docker + Docker Compose
- **Database**: PostgreSQL 17 + Docker
- **Cache**: Node Cache + Fallback resiliente
- **Development**: Hot reload completo

## 📁 Estrutura do Projeto

```
CoreApp-Vertical-Modular/
├── backend/                    # Express.js + TypeScript + Prisma
│   ├── src/
│   │   ├── controllers/       # REST controllers
│   │   ├── services/          # Lógica de negócio + JWT + Auth
│   │   ├── repositories/      # Repository pattern + Unit of Work
│   │   ├── middleware/        # Auth + Tenant + Error handling
│   │   ├── routes/            # Definição de rotas
│   │   ├── config/            # Configurações (DB, JWT, Swagger)
│   │   ├── patterns/          # CQRS + Design patterns
│   │   └── types/             # TypeScript types
│   ├── prisma/                # Schema Prisma + Migrations
│   │   └── schema.prisma      # Multi-tenant + Soft delete + Auth
│   ├── package.json           # Express.js + Prisma + JWT
│   └── tsconfig.json
├── frontend/                   # Next.js 15 + Mantine 7
│   ├── src/
│   │   ├── app/               # Next.js App Router
│   │   ├── components/        # Componentes Mantine + Auth
│   │   ├── hooks/             # Custom hooks + useAuth
│   │   ├── stores/            # Zustand para estado
│   │   ├── services/          # API calls + Auth service
│   │   ├── lib/               # Utils + API client
│   │   └── types/             # TypeScript types
│   ├── package.json           # Next.js + Mantine + Zustand
│   └── tsconfig.json
├── docker-compose.yml         # PostgreSQL 17 + Services
└── package.json               # Root workspace
```

## 🚀 Como Executar

### Pré-requisitos
- Node.js 20.x+
- Docker 20.10+ e Docker Compose v2.0+
- Git

### Execução Desenvolvimento
```bash
# Clonar repositório
git clone <url-do-repositorio>
cd CoreApp-Vertical-Modular

# Instalar dependências
npm install
cd backend && npm install
cd ../frontend && npm install
cd ..

# Iniciar PostgreSQL
docker-compose up -d postgres

# Configurar banco de dados
cd backend
npx prisma db push
npx prisma db seed

# Executar desenvolvimento (paralelo)
# Terminal 1: Backend
cd backend && npm run dev

# Terminal 2: Frontend
cd frontend && npm run dev
```

### URLs de Desenvolvimento
| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Backend** | http://localhost:5000/health | Health Check Express.js |
| **Frontend Next.js** | http://localhost:3000 | Interface usuário |
| **API Docs** | http://localhost:5000/api/docs | Swagger UI completo |
| **Prisma Studio** | http://localhost:5555 | Admin banco dados |
| **PostgreSQL CoreApp** | localhost:5432 | Banco principal |

### 🌐 Links de Teste Online
| Ambiente | URL | Status | Credenciais |
|----------|-----|--------|-------------|
| **Demo** | https://demo-coreapp.vercel.app | 🟢 Online | admin@demo.com / admin123 |
| **Staging** | https://staging-coreapp.vercel.app | 🟡 Testing | gerente@demo.com / gerente123 |
| **API Demo** | https://api-demo-coreapp.vercel.app | 🟢 Online | Bearer token via login |
| **Prisma Studio** | https://studio-demo-coreapp.vercel.app | 🟢 Online | Somente leitura |

### Comandos Desenvolvimento
```bash
# Backend Express.js
cd backend
npm install
npm run dev
npm run build
npm test

# Frontend Next.js
cd frontend
npm install
npm run dev
npm run build

# Banco de dados Prisma
cd backend
npx prisma generate
npx prisma db push
npx prisma studio

# PostgreSQL via Docker
docker-compose up -d postgres
docker-compose down
```

## 🔐 Credenciais Demo

### Tenant Demo
- **URL**: http://demo.localhost
- **Super Admin**: `admin@demo.com` / `admin123`
- **Gerente**: `gerente@demo.com` / `gerente123`
- **Vendedor**: `vendedor@demo.com` / `vend123`

### Hierarquia de Usuários
| Perfil | Módulos Acessíveis | Funcionalidades |
|--------|-------------------|-----------------|
| **Super Admin** | Todos + Admin | Gestão tenants, configurações |
| **Admin Loja** | Starter + Contratados | Todos módulos da loja |
| **Gerente** | Operacionais | Produtos, estoque, vendas |
| **Vendedor** | Básicos | Vendas, cadastro clientes |

## 🧪 Desenvolvimento

### Comandos Essenciais
```bash
# Backend Express.js + TypeScript + Prisma
cd backend
npm install
npm run dev          # Desenvolvimento com hot reload
npm run build        # Compilar TypeScript
npm run start        # Produção
npm run db:push      # Sincronizar schema Prisma
npm run db:seed      # Popular dados demo
npm run db:studio    # Interface admin Prisma

# Frontend Next.js + Mantine
cd frontend
npm install
npm run dev          # Desenvolvimento com Turbopack
npm run build        # Build produção
npm run start        # Produção

# Desenvolvimento Completo (Paralelo)
# Terminal 1: cd backend && npm run dev
# Terminal 2: cd frontend && npm run dev
```

### Quality Gates
- **Cobertura**: ≥ 80%
- **SOLID Compliance**: Zero violações
- **Bugs/Vulnerabilidades**: Zero tolerância
- **Acessibilidade**: Navegação completa por teclado

### ⌨️ Acessibilidade por Teclado
**Navegação obrigatória:**
- **TAB**: Navega em ordem lógica entre elementos
- **SHIFT+TAB**: Navegação reversa
- **ENTER**: Confirma subação → ação completa
- **ESC**: Cancela ação atual (fecha modais, volta ao estado anterior)
- **Setas**: Navegação dentro de listas/menus/componentes

## 🚢 Deploy Produção

### URLs Produção
- **API**: https://api-coreapp.vercel.app
- **Frontend**: https://app-coreapp.vercel.app
- **Multi-tenant**: https://{tenant}.coreapp.com.br
- **Admin**: https://admin.coreapp.com.br

### 🧪 Endpoints de Teste
```bash
# Health Check
curl http://localhost:5000/health

# Login Demo (Super Admin)
curl -X POST http://localhost:5000/api/auth/super-admin/login \
  -H "Content-Type: application/json" \
  -d '{"login":"SA00001A","senha":"A1234B"}'

# Listar Clientes (Demo)
curl http://localhost:5000/api/clientes \
  -H "X-Tenant-ID: demo" \
  -H "Authorization: Bearer {seu-token}"

# Documentação Swagger UI
# Acesse: http://localhost:5000/api/docs
```

### Deploy Automático
- **Branch**: `develop` → Deploy automático
- **Platform**: Vercel (Frontend) + Railway/Render (Backend)
- **Database**: PostgreSQL 17 via Docker ou PaaS

## 🏛️ Compliance Brasileiro

### Conformidade
- **LGPD**: Proteção dados pessoais
- **Receita Federal**: NFCe automática
- **Código Defesa Consumidor**: Compliance comercial


### Ambientes
- **Desenvolvimento**: http://localhost
- **Produção**: https://app.seudominio.com.br

---
*CoreApp v4.0 - Express.js + Next.js*

---

## 📋 Licença

**© 2024 CoreApp SAAS**  
Software proprietário.  
Dados processados em território nacional (LGPD)
