# 🏥 Farmácia API - Sistema de Configurações Dinâmicas

## 📋 Resumo

API .NET 9 que implementa **configurações dinâmicas** para substituir enums rígidos, conforme especificado no CLAUDE.md. O sistema permite que farmácias tenham configurações flexíveis que podem ser alteradas sem necessidade de deploy.

### ✨ Características Principais

- **🚫 Zero Enums** - Todas as configurações são dinâmicas em banco de dados
- **⚡ Cache Inteligente** - IMemoryCache com 30 min sliding expiration
- **🏢 Multi-tenant** - Suporte a múltiplas farmácias com isolamento de dados
- **🇧🇷 ANVISA Compliance** - Listas controladas A1-C5 oficiais
- **🔄 Fallback Hierárquico** - Cache → Database (Tenant → Global)

## 🏗️ Arquitetura

```
┌─────────────────┐    ┌──────────────┐    ┌─────────────────┐
│   Controllers   │───▶│   Services   │───▶│   Database      │
│                 │    │              │    │                 │
│ • Configurações │    │ • Config     │    │ • PostgreSQL    │
│ • Health        │    │ • Cache      │    │ • Multi-tenant  │
└─────────────────┘    └──────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌──────────────┐
                       │  IMemoryCache│
                       │              │
                       │ • 30min exp  │
                       │ • Auto-inval │
                       └──────────────┘
```

## 🚀 Endpoints Disponíveis

### Configurações ANVISA
- `GET /api/configuracoes/anvisa` - Lista todas as classificações
- `GET /api/configuracoes/anvisa/{codigo}` - Busca por código (A1, B1, C1, etc.)
- `POST /api/configuracoes/anvisa` - Cria/atualiza classificação
- `DELETE /api/configuracoes/anvisa/{id}` - Remove classificação
- `POST /api/configuracoes/cache/invalidate` - Invalida cache

### Health Checks
- `GET /api/health` - Status geral da API
- `GET /api/health/database` - Status do PostgreSQL
- `GET /api/health/cache` - Status do cache IMemoryCache

### Documentação
- `GET /swagger` - Interface Swagger UI

## 💾 Modelo de Dados

### ClassificacaoAnvisaEntity
```csharp
{
  "id": "guid",
  "tenantId": "string?",           // null = global
  "codigo": "string",              // A1, A2, B1, B2, C1-C5
  "nome": "string",
  "tipoReceita": "string",         // AZUL, BRANCA
  "requerRetencaoReceita": "bool",
  "diasValidadeReceita": "int",
  "isOficialAnvisa": "bool",       // proteção contra remoção
  "ativo": "bool",
  "ordem": "int"
}
```

## ⚡ Sistema de Cache

### Como Funciona
1. **Primeira consulta** → Cache vazio → Busca database → Cacheia resultado
2. **Consultas seguintes** → Retorna do cache (30 min expiration)
3. **Atualização/Remoção** → Invalida cache automaticamente
4. **Fallback Hierárquico** → Tenant específico → Global → Null

### Exemplo de Uso
```csharp
// Busca classificação A1 para farmacia "sp-001"
var classificacao = await _configService.GetByCodeAsync<ClassificacaoAnvisaEntity>("sp-001", "A1");

// 1ª chamada: Cache miss → Database → Cache hit
// 2ª chamada: Cache hit → Resposta instantânea
```

## 🏃‍♂️ Como Executar

### Pré-requisitos
- .NET 9 SDK
- PostgreSQL 12+

### Execução Local
```bash
# Clone e navegue para o diretório
cd FarmaciaApi

# Restaure dependências
dotnet restore

# Execute a aplicação
dotnet run

# Acesse Swagger
http://localhost:5004/swagger
```

### Com PostgreSQL
```bash
# Configure connection string no appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=5432;Database=farmacia_dev;User Id=admin;Password=senha123;"
}

# Execute - migrations são aplicadas automaticamente
dotnet run

# Seeds ANVISA são criados automaticamente na primeira execução
```

## 🧪 Testes

### Health Check
```bash
curl http://localhost:5004/api/health
# Resposta: { "status": "healthy", "features": [...] }
```

### Listar Classificações ANVISA
```bash
curl http://localhost:5004/api/configuracoes/anvisa
# Resposta: [{ "codigo": "A1", "nome": "Lista A1 - Entorpecentes", ... }]
```

### Criar Classificação Personalizada
```bash
curl -X POST http://localhost:5004/api/configuracoes/anvisa \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "CUSTOM",
    "nome": "Classificação Personalizada",
    "tipoReceita": "BRANCA",
    "tenantId": "farmacia-001"
  }'
```

## 📊 Benefícios

### ✅ Antes (Com Enums)
```csharp
public enum ClassificacaoAnvisa { A1, A2, A3, B1, B2, C1, C2, C3, C4, C5 }
// ❌ Mudanças requerem deploy
// ❌ Não permite personalização por farmácia
// ❌ Rígido e inflexível
```

### ✅ Depois (Com Configurações Dinâmicas)
```csharp
var classificacao = await _config.GetByCodeAsync<ClassificacaoAnvisaEntity>(tenantId, "A1");
// ✅ Mudanças via API, sem deploy
// ✅ Personalização por farmácia
// ✅ Flexível e extensível
// ✅ Cache inteligente para performance
```

## 🛡️ Compliance ANVISA

### Listas Controladas Oficiais
- **A1, A2, A3** → Receita Azul, Retenção obrigatória
- **B1, B2** → Receita Branca, Retenção obrigatória  
- **C1-C5** → Receita Branca, Retenção variável

### Proteções
- Classificações oficiais ANVISA não podem ser removidas (`IsOficialAnvisa = true`)
- Validação automática de tipos de receita
- Controle de dias de validade por classificação

## 🔧 Configuração

### Cache Settings
```csharp
services.AddMemoryCache(options => {
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});
```

### Multi-tenant
- Configurações globais (`TenantId = null`)
- Configurações específicas por farmácia (`TenantId = "farmacia-001"`)
- Fallback automático: Específico → Global

---

## 📝 Conclusão

Esta implementação demonstra como **substituir enums rígidos por configurações dinâmicas** mantendo alta performance através de cache inteligente, permitindo flexibilidade total para farmácias brasileiras sem comprometer compliance ANVISA.

**🎯 Resultado**: Sistema **100% dinâmico**, **performático** e **conforme regulamentações brasileiras**!