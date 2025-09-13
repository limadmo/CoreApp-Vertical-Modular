# CoreApp - Sistema SAAS Multi-tenant (.NET 9)

![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
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
- **ORM**: Prisma 5.x
- **Database**: PostgreSQL 17
- **Architecture**: Verticais + SOLID + Clean Architecture
- **Patterns**: Unit of Work + Repository + CQRS + Event Sourcing

### Frontend (React + Mantine)
- **Framework**: React 18.3.x + TypeScript 5.3.x
- **UI Library**: Mantine 7.0 + Tailwind CSS 4+
- **State Management**: Zustand 4.5.0
- **Design**: Cards modernos, contraste melhorado, tipografia legível
- **Build**: Vite + TSDoc documentation

### Infraestrutura
- **Containers**: Docker + Docker Compose
- **Reverse Proxy**: Traefik v3.1 (roteamento multi-tenant)
- **Cache**: IMemoryCache + Redis 7.2
- **Message Queue**: RabbitMQ 3.13

## 📁 Estrutura do Projeto

```
coreapp-saas/
├── backend/                    # Express.js + TypeScript
│   ├── src/
│   │   ├── controllers/       # REST controllers
│   │   ├── services/          # Lógica de negócio
│   │   ├── models/           # Modelos de dados (Prisma)
│   │   ├── middleware/       # Middlewares (auth, tenant, vertical)
│   │   ├── routes/           # Definição de rotas
│   │   ├── config/           # Configurações (DB, JWT, etc)
│   │   ├── utils/            # Utilitários
│   │   └── types/            # TypeScript types
│   ├── prisma/               # Schema Prisma
│   │   ├── schema.prisma
│   │   └── migrations/
│   ├── package.json
│   └── tsconfig.json
├── frontend/                   # React + Mantine
│   ├── src/
│   │   ├── components/        # Componentes Mantine 7
│   │   ├── hooks/             # Custom hooks com useVerticalEntity
│   │   ├── stores/            # Zustand para estado
│   │   ├── pages/             # Páginas por vertical
│   │   ├── services/          # API calls
│   │   └── types/             # TypeScript types
├── traefik/                   # Reverse Proxy Multi-tenant
└── scripts/                   # Scripts desenvolvimento/deploy
```

## 🚀 Como Executar

### Pré-requisitos
- Docker 20.10+ e Docker Compose v2.0+
- Git

### Execução Completa
```bash
# Clonar repositório
git clone <url-do-repositorio>
cd coreapp

# Configurar ambiente (development por padrão)
echo "ENV=development" > .env

# Deploy completo
docker-compose up -d

# Verificar saúde
curl http://localhost:8080/health
```

### URLs de Desenvolvimento
| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Backend** | http://localhost:3001/health | Health Check |
| **Frontend React** | http://localhost:3000 | Interface usuário |
| **API Docs** | http://localhost:3001/api-docs | Documentação API |
| **Prisma Studio** | http://localhost:5555 | Admin banco dados |
| **PostgreSQL CoreApp** | localhost:5432 | Banco principal |
| **SonarQube** | http://localhost:9000 | Análise código (dev) |

### 🌐 Links de Teste Online
| Ambiente | URL | Status | Credenciais |
|----------|-----|--------|-------------|
| **Demo** | https://demo-coreapp.vercel.app | 🟢 Online | admin@demo.com / admin123 |
| **Staging** | https://staging-coreapp.vercel.app | 🟡 Testing | gerente@demo.com / gerente123 |
| **API Demo** | https://api-demo-coreapp.vercel.app | 🟢 Online | Bearer token via login |
| **Prisma Studio** | https://studio-demo-coreapp.vercel.app | 🟢 Online | Somente leitura |

### Comandos Docker
```bash
# Iniciar sistema (produção/staging)
docker-compose up -d

# Iniciar sistema + SonarQube (desenvolvimento)
docker-compose --profile development up -d

# Parar sistema
docker-compose down

# Rebuild completo
docker-compose up -d --build --force-recreate

# Ver logs
docker-compose logs -f coreapp

# SonarQube (apenas desenvolvimento)
./scripts/sonar-dev.sh

# Trocar para produção
echo "ENV=production" > .env && docker-compose up -d
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
# Backend Express.js
cd backend
npm install
npm run dev
npm run build
npm test

# Frontend React + Mantine
cd frontend
npm install
npm run dev
npm run build

# Prisma
cd backend
npx prisma generate
npx prisma db push
npx prisma studio

# Análise qualidade
./scripts/sonar-local.sh
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
curl https://api-demo-coreapp.vercel.app/health

# Login Demo
curl -X POST https://api-demo-coreapp.vercel.app/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"admin123"}'

# Listar Produtos (Demo)
curl https://api-demo-coreapp.vercel.app/api/produtos \
  -H "x-tenant-id: demo" \
  -H "Authorization: Bearer {seu-token}"
```

### Deploy Automático
- **Branch**: `develop-csharp` → Deploy automático
- **Platform**: Dokploy + Traefik + PostgreSQL 17
- **Configuração**: Via variável ENV=production

## 🏛️ Compliance Brasileiro

### Conformidade
- **LGPD**: Proteção dados pessoais
- **Receita Federal**: NFCe automática
- **Código Defesa Consumidor**: Compliance comercial

### Pagamentos Nacionais
- **PIX**: Pagamentos instantâneos
- **Boleto**: Tradicional brasileiro
- **Mercado Pago**: Gateway principal
- **PagSeguro**: Gateway backup

## 📞 Suporte

### Canais
- **Email**: suporte@coreapp.com.br
- **WhatsApp**: Suporte comercial
- **GitHub**: Issues técnicas

### Ambientes
- **Desenvolvimento**: http://localhost
- **Produção**: https://app.seudominio.com.br

---

## 🇧🇷 Desenvolvido com ❤️ para o Brasil

**Sistema SAAS que revoluciona a gestão comercial brasileira com tecnologia de ponta e compliance total.**

*CoreApp v3.0 - .NET 9 + React Admin*

---

## 📋 Licença

**© 2024 CoreApp SAAS**  
Software proprietário. Desenvolvido no Brasil para comércios brasileiros.  
Dados processados em território nacional (LGPD).