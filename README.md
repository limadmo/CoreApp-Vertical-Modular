# Sistema SAAS Multi-tenant Core (.NET 9 + Raspberry Pi PDV)

![Version](https://img.shields.io/badge/version-3.0.0--SAAS--BR-blue.svg)
![License](https://img.shields.io/badge/license-Proprietary-red.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-green.svg)
![React Admin](https://img.shields.io/badge/React%20Admin-4.16-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)
![Raspberry Pi](https://img.shields.io/badge/Raspberry%20Pi-4-red.svg)
![Docker](https://img.shields.io/badge/Docker-ready-brightgreen.svg)
![Traefik](https://img.shields.io/badge/Traefik-v3.0-purple.svg)
![Brasil](https://img.shields.io/badge/🇧🇷-100%25%20Brasileiro-green.svg)

## 🚀 Visão Geral

Sistema **SAAS multi-tenant 100% brasileiro** para gestão de múltiplos verticais (farmácias, padarias, supermercados, etc.) com **PDV Raspberry Pi** que funciona offline. Arquitetura híbrida **Cloud + Edge Computing** com **.NET 9**, **React Admin** e sincronização **C nativa**. Baseado em **Monólito Modular** com padrões **SOLID + DDD** para máxima modularidade comercial.

### ⭐ Principais Características

#### **🏢 Cloud SAAS Multi-tenant**
- **Isolamento total**: Dados de cada farmácia completamente separados
- **Escalabilidade infinita**: Traefik + Docker para milhares de farmácias
- **API versionada `/v1/`**: Padrão Rails para estabilidade
- **Preços acessíveis**: R$ 149,90/249,90/399,90 por mês

#### **🖥️ PDV Raspberry Pi Edge Computing**  
- **Nunca para de vender**: Funciona offline com bateria 10h
- **Custo baixo**: ~R$ 330 por farmácia (vs R$ 2.000+ concorrência)
- **Sincronização C nativa**: Performance ultra-rápida
- **Medicamentos controlados**: Receitas offline para validação posterior

#### **🇧🇷 100% Brasileiro**
- **Compliance total**: ANVISA, CFF, LGPD desde V1
- **Pagamentos nacionais**: PIX, Boleto, Mercado Pago
- **Impostos automáticos**: ICMS, PIS/COFINS por estado
- **Suporte português**: Documentação e interface PT-BR

## 🏗️ Arquitetura SAAS Brasileira

### 💰 Modelo de Negócio Nacional

#### Planos Comerciais (Todos Pagos)
| Plano | Preço Mensal | Preço Anual | Módulos | Farmácias |
|-------|--------------|-------------|---------|-----------|
| **Starter** | R$ 149,90 | R$ 1.499,00* | Core | Até 1 |
| **Professional** | R$ 249,90 | R$ 2.399,00* | Core + Extras | Até 3 |
| **Enterprise** | R$ 399,90 | R$ 3.599,00* | Todos | Ilimitadas |

_*Desconto de 2-3 meses no pagamento anual_

#### 🧩 Sistema de Módulos Comerciais

**Módulos Starter (R$ 149,90):**
- ✅ **Produtos**: Cadastro completo com classificação ANVISA
- ✅ **Vendas**: Sistema de vendas balcão e controladas
- ✅ **Estoque**: Controle com lotes, validade e alertas
- ✅ **Usuários**: Gestão completa com hierarquia farmacêutica

**Módulos Professional (+R$ 100,00):**
- 👥 **Clientes**: CRM farmacêutico com fidelidade
- 🎁 **Promoções**: Engine de descontos automáticos
- 📊 **Relatórios Básicos**: Analytics operacionais

**Módulos Enterprise (+R$ 150,00):**
- 📈 **Relatórios Avançados**: Dashboards executivos
- 🔍 **Auditoria ANVISA**: Logs compliance completos
- 🏪 **Fornecedores**: Gestão completa de compras
- 📱 **API Mobile**: Integração para apps próprios

### 🌐 Multi-tenant Brasileiro

#### Isolamento por Tenant
- **Database**: PostgreSQL 16 com tenant filtering automático
- **Subdomínios**: `farmacia1.seudominio.com.br`, `redefarmacia.seudominio.com.br`
- **Dados**: Isolamento total via EF Core Global Query Filters
- **Personalizações**: Temas e logos por farmácia/rede

#### Hierarquia Farmacêutica
```
Tenant (Rede de Farmácias)
├── Farmácia São Paulo (Centro)
├── Farmácia São Paulo (Vila Madalena) 
├── Farmácia Rio de Janeiro (Copacabana)
└── Farmácia Belo Horizonte (Savassi)
```

## 🛠️ Stack Tecnológica Brasileira

### Backend (.NET 9)
- **Runtime**: .NET 9.0 (Latest)
- **Framework**: ASP.NET Core 9 com Minimal APIs
- **ORM**: Entity Framework Core 9 + Global Query Filters
- **Database**: PostgreSQL 16 (multi-tenant)
- **Security**: ASP.NET Core Identity + JWT
- **OpenAPI**: Swagger integrado (documentação automática)
- **Architecture**: Monólito Modular + SOLID + DDD

### Frontend (React Admin)
- **Framework**: React Admin 4.16.x
- **UI Library**: React 18.3.x + Material-UI 5.x
- **Styling**: Tailwind CSS 3.4 LTS + Temas farmacêuticos
- **Language**: TypeScript 5.3.x
- **Build**: Vite (build ultrarrápido)
- **State**: TanStack Query + Context API

### Infraestrutura Nacional
- **Containers**: Docker + Docker Compose
- **Reverse Proxy**: Traefik v3.0 (roteamento multi-tenant)
- **Database**: PostgreSQL 16 (isolamento por tenant)
- **Cache**: IMemoryCache integrado do ASP.NET Core
- **Deploy**: Dokploy (auto-deploy brasileiro)
- **Monitoring**: Health Checks + OpenTelemetry

### Pagamentos Brasileiros
- **Gateway Principal**: Mercado Pago
- **Gateway Backup**: PagSeguro
- **PIX**: Pagamentos instantâneos
- **Boleto**: Para farmácias tradicionais
- **Cartão**: Débito/crédito com parcelamento

## 📁 Estrutura do Projeto Modular

```
core-saas/
├── backend/                    # ASP.NET Core 9
│   ├── src/
│   │   ├── Farmacia.Core/      # Módulos Starter (R$ 149,90)
│   │   │   ├── Products/       # Produtos e medicamentos ANVISA
│   │   │   ├── Sales/          # Sistema de vendas completo
│   │   │   ├── Stock/          # Controle de estoque avançado
│   │   │   └── Users/          # Gestão de usuários farmacêuticos
│   │   │
│   │   ├── Farmacia.Modules/   # Módulos Comerciais (Pagos)
│   │   │   ├── Customers/      # CRM + Fidelidade (Professional)
│   │   │   ├── Promotions/     # Engine descontos (Professional)
│   │   │   ├── BasicReports/   # Relatórios básicos (Professional)
│   │   │   ├── AdvancedReports/# Analytics avançados (Enterprise)
│   │   │   ├── Audit/          # Compliance ANVISA (Enterprise)
│   │   │   ├── Suppliers/      # Gestão fornecedores (Enterprise)
│   │   │   └── Mobile/         # API Mobile (Enterprise)
│   │   │
│   │   ├── Farmacia.Shared/    # Infraestrutura Compartilhada
│   │   │   ├── MultiTenant/    # Isolamento automático por tenant
│   │   │   ├── Security/       # JWT + Identity brasileiro
│   │   │   ├── Payments/       # Mercado Pago + PagSeguro + PIX
│   │   │   ├── Pricing/        # Sistema configuração de preços
│   │   │   └── ANVISA/         # APIs e validações ANVISA
│   │   │
│   │   └── Farmacia.Api/       # Entry Point ASP.NET Core
│   │       ├── Controllers/    # REST APIs + OpenAPI
│   │       ├── Middleware/     # Tenant resolution automático
│   │       └── Program.cs      # Configuração .NET 9
│   │
│   ├── Dockerfile              # .NET 9 optimized
│   └── Farmacia.sln           # Solution completa
│
├── frontend/                   # React Admin Application
│   ├── src/
│   │   ├── modules/           # Módulos por plano comercial
│   │   │   ├── core/          # Sempre disponível (Starter)
│   │   │   ├── professional/   # Professional + Enterprise
│   │   │   └── enterprise/     # Apenas Enterprise
│   │   ├── themes/            # Temas farmacêuticos brasileiros
│   │   │   ├── farmacia-moderna.ts    # Padrão (verde + azul)
│   │   │   ├── alto-contraste.ts      # Acessibilidade
│   │   │   ├── modo-escuro.ts         # Dark mode
│   │   │   └── daltonismo.ts          # Color-blind friendly
│   │   ├── components/        # Componentes React Admin
│   │   ├── providers/         # Auth + Data + Tenant providers
│   │   └── utils/             # Utilitários multi-tenant
│   ├── Dockerfile
│   └── package.json
│
├── traefik/                   # Reverse Proxy Multi-tenant
│   ├── traefik.yml           # Configuração principal
│   ├── dynamic.yml           # Roteamento dinâmico brasileiro
│   └── certs/                # Certificados SSL
│
├── scripts/                   # Scripts Brasileiros
│   ├── dev-start.sh          # Ambiente desenvolvimento
│   ├── prod-deploy.sh        # Deploy produção
│   ├── tenant-setup.sh       # Setup nova farmácia
│   └── backup-brasil.sh      # Backup dados brasileiros
│
├── docker-compose.yml        # Produção
├── docker-compose.dev.yml    # Desenvolvimento
└── README.md                 # Este arquivo
```

## 🚀 Como Executar - Desenvolvimento

### Pré-requisitos
- **Docker** 20.10+ e **Docker Compose** v2.0+
- **Git** para clonar o repositório
- **.NET 9 SDK** (opcional, para desenvolvimento sem Docker)

### 🐳 Execução Completa com Docker (Recomendado)

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd farmacia

# Deploy completo do sistema
./scripts/deploy.sh

# Verificar saúde do sistema
./scripts/health-check.sh
```

### 🌐 URLs de Desenvolvimento

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Backend** | http://localhost:8080/health | API REST .NET 9 - Health Check |
| **Swagger UI** | http://localhost:8080/swagger | Documentação Interativa da API |
| **Database Health** | http://localhost:8080/health/database | Status PostgreSQL + Seeds ANVISA |
| **Cache Health** | http://localhost:8080/health/cache | Status Cache em Memória |
| **PostgreSQL** | localhost:5432 | Banco de dados direto |

### 🐳 Comandos Docker

```bash
# Iniciar sistema
docker-compose up -d

# Parar sistema  
docker-compose down

# Limpar volumes e reiniciar do zero
docker-compose down -v
docker-compose up -d --build

# Ver logs em tempo real
docker-compose logs -f coreapi

# Status dos containers
docker-compose ps
```

### 🏥 URLs de Produção (Dokploy + Traefik)

| Ambiente | URL | Tipo |
|----------|-----|------|
| **API Produção** | https://api-dev.diegolima.dev | API REST em produção |
| **Multi-tenant** | https://{tenant}.farmacia-dev.diegolima.dev | URLs dinâmicas por farmácia |
| **Admin** | https://admin-dev.diegolima.dev | Administração geral |

## 🔐 Credenciais de Desenvolvimento

### Tenant Demo (Demonstração)
- **URL**: http://demo.localhost
- **Super Admin**: `admin@demo.com` / `admin123`
- **Gerente**: `gerente@demo.com` / `gerente123`
- **Farmacêutico**: `farmaceutico@demo.com` / `farm123`
- **Vendedor**: `vendedor@demo.com` / `vend123`
- **Caixa**: `caixa@demo.com` / `caixa123`

### Hierarquia de Usuários Farmacêutica
| Perfil | Módulos Acessíveis | Funcionalidades |
|--------|-------------------|-----------------|
| **Super Admin** | Todos + Admin | Gestão de tenants, preços |
| **Admin Tenant** | Todos do plano | Todos módulos contratados |
| **Gerente** | Operacionais | Produtos, estoque, vendas, relatórios |
| **Farmacêutico** | Compliance | Vendas controladas, receitas |
| **Vendedor** | Básicos | Vendas livres, cadastro clientes |
| **Caixa** | Mínimos | Finalização de vendas apenas |

## 🧩 Sistema de Módulos e Preços

### Validação Automática por Plano
```csharp
// Exemplo de proteção de módulo comercial
[HttpGet("/api/relatorios/avancados")]
[RequireModule("ADVANCED_REPORTS")] // Só Enterprise
public async Task<ActionResult> GetAdvancedReports()
{
    // Método só executa se tenant tem módulo ativo
    return Ok(await _reportsService.GetAdvancedReports());
}
```

### Interface de Configuração de Preços
- **Admin Dashboard**: Configurar preços dinamicamente
- **Histórico de Mudanças**: Log com justificativas obrigatórias
- **Preview de Impacto**: Visualizar efeito nos clientes
- **Notificação**: Email automático sobre alterações
- **A/B Testing**: Testar preços diferentes

### Gateways de Pagamento Brasileiros
```csharp
// Integração com múltiplos gateways nacionais
public class BrazilianPaymentService
{
    // Mercado Pago (principal)
    public async Task<PaymentResult> CreateMercadoPagoSubscription(decimal amountBRL)
    
    // PagSeguro (backup)  
    public async Task<PaymentResult> CreatePagSeguroSubscription(decimal amountBRL)
    
    // PIX (instantâneo)
    public async Task<PixQRCode> GeneratePixPayment(decimal amountBRL)
    
    // Boleto (tradicional)
    public async Task<BoletoData> GenerateBoleto(decimal amountBRL)
}
```

## 🎨 Sistema de Temas Brasileiros

### 🏥 Tema Padrão - "Farmácia Moderna"
```javascript
// Identidade visual farmacêutica brasileira
const farmaciaBrasileiraTheme = {
  primary: {
    main: '#2E7D32',    // Verde saúde (cor principal)
    dark: '#1B5E20',    // Verde escuro
  },
  secondary: {
    main: '#1976D2',    // Azul confiança
    dark: '#1565C0',    // Azul escuro
  },
  success: { main: '#4CAF50' },  // Verde sucesso
  warning: { main: '#FF9800' },  // Laranja alerta
  error: { main: '#F44336' },    // Vermelho erro
}
```

### Temas Alternativos Selecionáveis
1. **Alto Contraste**: Para acessibilidade (WCAG AAA)
2. **Modo Escuro**: Para uso noturno em plantões  
3. **Daltonismo**: Cores específicas para deficiência visual
4. **Farmácia Tradicional**: Cores mais conservadoras

### Personalização por Tenant
- **Logo customizado**: Upload da logo da farmácia
- **Cores principais**: Personalizar paleta de cores
- **Tipografia**: Escolher fonte (Roboto, Poppins, etc.)
- **Layout**: Sidebar, topbar, densidade de informações

## 🧪 Testes com Dados Farmacêuticos Reais

### Estratégia de Testes Brasileira
```csharp
// Sempre usar dados farmacêuticos reais do Brasil
[Test]
public async Task DeveCalcularImpostosSobreMedicamento_ComDadosReaisBrasil()
{
    // Arrange: Medicamento real brasileiro
    var dipirona = new Produto
    {
        Nome = "Dipirona Sódica 500mg",
        PrincipioAtivo = "Dipirona Sódica",
        ClassificacaoAnvisa = ClassificacaoAnvisa.ISENTO_PRESCRICAO,
        PrecoVenda = 12.50m,
        TenantId = "farmacia-teste-br"
    };
    
    // Act: Cálculo com impostos brasileiros
    var resultado = await _vendaService.CalcularVenda(dipirona, quantidade: 2);
    
    // Assert: Verificar cálculos brasileiros corretos
    Assert.Equal(25.00m, resultado.Subtotal);
    Assert.Equal(2.13m, resultado.ICMS);    // 8.5% ICMS farmácia
    Assert.Equal(1.60m, resultado.PIS);     // 6.4% PIS/COFINS
}
```

### TestContainers com Dados ANVISA
- **Database real**: PostgreSQL com seed farmacêutico
- **Medicamentos reais**: Base ANVISA atualizada
- **Regulamentações**: Testes compliance automáticos
- **Multi-tenant**: Isolamento testado entre farmácias

## 🚢 Deploy & Produção Brasileira

### Dokploy Deploy Automático
- **Branch**: `develop-csharp` → Deploy automático
- **Frontend**: https://farmacia.seudominio.com.br
- **API**: https://api.seudominio.com.br
- **Multi-tenant**: https://{tenant}.seudominio.com.br

### Configuração Produção Nacional
```yaml
# docker-compose.yml para produção brasileira
services:
  traefik:
    image: traefik:v3.0
    labels:
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
  
  farmacia-backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=postgresql://...
      - JWT__Secret=${JWT_SECRET}
      - MercadoPago__AccessToken=${MERCADOPAGO_TOKEN}
      - PagSeguro__Token=${PAGSEGURO_TOKEN}
    
  farmacia-frontend:
    environment:
      - REACT_APP_API_URL=https://api.seudominio.com.br
      - REACT_APP_MERCADOPAGO_PUBLIC_KEY=${MERCADOPAGO_PUBLIC_KEY}
```

## 📊 Monitoramento & Analytics Brasileiros

### Métricas SAAS
```csharp
// Health checks específicos para farmácias brasileiras
public class FarmaciaHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        // Verificar conectividade com ANVISA
        var anvisaOk = await _anvisaService.CheckConnectionAsync();
        
        // Verificar gateways de pagamento brasileiros
        var mercadoPagoOk = await _mercadoPagoService.HealthCheckAsync();
        
        // Verificar compliance LGPD
        var lgpdOk = await _lgpdService.ValidateComplianceAsync();
        
        return anvisaOk && mercadoPagoOk && lgpdOk ? 
            HealthCheckResult.Healthy() : 
            HealthCheckResult.Unhealthy();
    }
}
```

### Dashboard Financeiro
- **MRR (Monthly Recurring Revenue)**: Em reais brasileiros
- **Churn Rate**: Taxa de cancelamento por região
- **LTV (Lifetime Value)**: Valor médio por farmácia
- **CAC (Customer Acquisition Cost)**: Custo aquisição brasileiro
- **Conversão**: Funil de vendas por estado

## 🔧 Desenvolvimento Local

### Comandos Essenciais
```bash
# Ambiente completo
docker-compose -f docker-compose.dev.yml up -d

# Rebuild completo
docker-compose -f docker-compose.dev.yml up -d --build --force-recreate

# Logs em tempo real
docker-compose -f docker-compose.dev.yml logs -f backend
docker-compose -f docker-compose.dev.yml logs -f frontend

# Setup nova farmácia (tenant)
./scripts/tenant-setup.sh nova-farmacia-sp

# Backup desenvolvimento
./scripts/backup-dev.sh
```

### Desenvolvimento Sem Docker
```bash
# Backend .NET 9
cd backend
dotnet restore
dotnet run --project CoreApi

# Frontend React Admin
cd frontend  
npm install
npm start
```

## 🎯 Roadmap SAAS Brasileiro

### ✅ V3.0 - SAAS Multi-tenant (Atual)
- [x] Arquitetura .NET 9 monólito modular
- [x] Sistema comercial com preços brasileiros
- [x] Multi-tenancy com isolamento completo
- [x] React Admin com temas farmacêuticos
- [x] Pagamentos nacionais (Mercado Pago + PagSeguro + PIX)

### 🔄 V3.1 - Expansão Nacional (Q2 2025)
- [ ] Integração com mais redes farmacêuticas
- [ ] App mobile nativo (iOS/Android)
- [ ] Relatórios avançados com BI
- [ ] WhatsApp Business integrado

### 🚀 V3.2 - Escala Nacional (Q3 2025)
- [ ] Auto-scaling Kubernetes
- [ ] Inteligência artificial para vendas
- [ ] Marketplace de medicamentos
- [ ] Integração com planos de saúde

## 🏛️ Compliance & Regulamentação Brasileira

### Conformidade Total
- **ANVISA**: Classificação completa A1-C5, receituário digital
- **CFF**: Controle farmacêutico e responsabilidade técnica
- **LGPD**: Proteção de dados pessoais com consentimento
- **Receita Federal**: Emissão de NFCe para vendas
- **SNGPC**: Sistema Nacional de Gerenciamento de Produtos Controlados

### Auditoria e Fiscalização  
- **Logs ANVISA**: Todas as vendas controladas registradas
- **Relatórios CFF**: Movimento mensal para fiscalização
- **LGPD Reports**: Consentimentos e tratamento de dados
- **Backup Compliance**: Retenção de dados por 5 anos

## 💡 Diferenciais Competitivos

### Vantagens Técnicas
- **.NET 9 Performance**: Até 40% mais rápido que versões anteriores
- **Monólito Modular**: Simplicidade operacional + flexibilidade comercial
- **Multi-tenant Nativo**: Isolamento automático por farmácia
- **Temas Acessíveis**: WCAG AAA compliance + identidade farmacêutica

### Vantagens Comerciais
- **Preços Brasileiros**: A partir de R$ 149,90 (acessível)
- **Sem Setup**: Zero taxa de implementação
- **Pagamentos Nacionais**: PIX, boleto, cartão parcelado
- **Suporte Regional**: Horário comercial brasileiro

### Vantagens Regulatórias
- **ANVISA Nativo**: Desenvolvido para compliance total
- **Receituário Digital**: Controle automático de receitas
- **SNGPC Integrado**: Relatórios automáticos para Polícia Federal
- **LGPD Ready**: Consentimentos e direitos do titular

## 📞 Suporte e Comunidade

### Ambientes de Acesso
- **Desenvolvimento**: http://localhost (farmácia demo)
- **Homologação**: https://staging.farmacia.seudominio.com.br  
- **Produção**: https://app.farmacia.seudominio.com.br

### Canais de Suporte
- **Documentação**: Wiki técnica completa
- **WhatsApp**: Suporte comercial brasileiro
- **Email**: suporte@farmacia.com.br
- **Portal**: Central do cliente

### Comunidade
- **GitHub**: Issues e contribuições
- **Discord**: Comunidade de desenvolvedores
- **YouTube**: Tutoriais e treinamentos
- **Blog**: Novidades e casos de sucesso

---

## 🇧🇷 Desenvolvido com ❤️ para o Brasil

**Sistema SAAS que revoluciona a gestão farmacêutica brasileira com tecnologia de ponta, preços acessíveis e compliance total com a regulamentação nacional.**

*Orgulhosamente brasileiro - SAAS Farmácia v3.0 - .NET 9 + React Admin*

---

## 📋 Licença e Termos

**© 2024 Sistema Farmácia SAAS**  
Software proprietário. Todos os direitos reservados.  
Desenvolvido no Brasil para farmácias brasileiras.

**Política de Dados**: Todos os dados são processados e armazenados em território nacional, em conformidade com a LGPD.