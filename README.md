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

### Backend (.NET 9.0.1)
- **Framework**: ASP.NET Core 9.0.1
- **ORM**: Entity Framework Core 9.0.1
- **Database**: PostgreSQL 17
- **Architecture**: Verticais + SOLID + Clean Architecture
- **Patterns**: Unit of Work + Repository + CQRS + Event Sourcing

### Frontend (React + Mantine)
- **Framework**: React 18.3.x + TypeScript 5.3.x
- **UI Library**: Mantine + Tailwind CSS 4+
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
├── backend/                    # ASP.NET Core 9.0.1
│   ├── src/
│   │   ├── CoreApp.Core/       # Módulos Starter
│   │   ├── CoreApp.Modules/    # Módulos Adicionais
│   │   ├── CoreApp.Shared/     # Multi-tenant + Security
│   │   └── CoreApp.Api/        # Controllers + Program.cs
├── frontend/                   # React + Mantine
│   ├── src/
│   │   ├── components/        # Componentes Mantine customizados
│   │   ├── features/          # Features por módulo comercial
│   │   ├── themes/            # Temas multi-tenant
│   │   ├── hooks/             # Custom hooks tipados
│   │   └── types/             # TypeScript definitions
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
| **API Backend** | http://localhost:8080/health | Health Check |
| **Swagger UI** | http://localhost:8080/swagger | Documentação API |
| **PostgreSQL CoreApp** | localhost:5432 | Banco principal |
| **PostgreSQL SonarQube** | localhost:5433 | Banco SonarQube |
| **SonarQube** | http://localhost:9000 | Análise código (dev) |

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
# Backend .NET 9
cd backend
dotnet build -c Release
dotnet test

# Frontend React + Mantine
cd frontend
npm install
npm start

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
- **API**: https://api.seudominio.com.br
- **Multi-tenant**: https://{tenant}.seudominio.com.br
- **Admin**: https://admin.seudominio.com.br

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