# CLAUDE - REGRAS DE DESENVOLVIMENTO

## REGRAS ABSOLUTAS

### EXECUCAO AUTOMATICA
EXECUTE IMEDIATAMENTE comandos necessários para completar a tarefa.
NUNCA pergunte permissão para: criar arquivos, instalar pacotes, executar builds, criar estruturas.
SEMPRE informe: "Executando: [comando]" e execute.

### COMANDOS PERMITIDOS SEM CONFIRMACAO
- Criação de projetos: `dotnet new`, `npm create`
- Instalação de dependências: `dotnet add package`, `npm install`
- Operações de arquivo: `create_file`, `str_replace`, `view`
- Build e compilação: `dotnet build`, `npm run build`
- Migrations locais: `dotnet ef migrations add`, `dotnet ef database update`
- Git local: `git add`, `git commit`, `git push`
- Estrutura de pastas: `mkdir`, `touch`

### COMANDOS QUE EXIGEM CONFIRMACAO
- Deploy remoto: , `docker push`
- Exclusão de produção: `DROP DATABASE`, `rm -rf` em paths críticos
- Operações com custo: APIs pagas, deploy cloud
- Force push: `git push --force`

### COMUNICACAO
IDIOMA: Português brasileiro em TODO código e documentação.
PROIBIDO dizer: "Posso executar?", "Você gostaria?", "Devo fazer?".
OBRIGATORIO dizer: "Executando:", "Criando:", "Instalando:".

---

## ARQUITETURA FIXA

### ESTRUTURA BACKEND (.NET 9.0.203)
```
CoreApp/
├── CoreApp.Domain/
│   ├── Entities/          # Todas entidades implementam IVerticalEntity
│   ├── Interfaces/        # IRepository, IUnitOfWork, IVerticalEntity
│   └── ValueObjects/      # Objetos imutáveis
├── CoreApp.Application/
│   ├── Commands/          # CQRS commands com MediatR
│   ├── Queries/           # CQRS queries com MediatR
│   ├── Services/          # Lógica de negócio
│   └── Validators/        # FluentValidation
├── CoreApp.Infrastructure/
│   ├── Data/              # DbContext + Configurations
│   ├── Repositories/      # Implementações sem SaveChanges
│   └── Migrations/        # EF Core migrations
└── CoreApp.Api/
    ├── Controllers/       # REST controllers
    ├── DTOs/              # Request/Response DTOs
    └── Middleware/        # Tenant e Vertical resolution

CoreApp.Verticals/
├── Padaria/               # Vertical PADARIA
├── Farmacia/              # Vertical FARMACIA
├── Supermercado/          # Vertical SUPERMERCADO
└── RestauranteDelivery/   # Vertical DELIVERY
```

### ESTRUTURA FRONTEND (React 18.3)
```
frontend/
├── src/
│   ├── components/        # Componentes Mantine 7
│   ├── hooks/             # Custom hooks com useVerticalEntity
│   ├── stores/            # Zustand para estado
│   ├── pages/             # Páginas por vertical
│   ├── services/          # API calls
│   └── types/             # TypeScript types
```

### VERSOES OBRIGATORIAS
```json
{
  ".NET": "9.0.203",
  "Entity Framework Core": "9.0.0",
  "PostgreSQL": "17",
  "React": "18.3.0",
  "TypeScript": "5.3.0",
  "Mantine": "7.0.0",
  "Zustand": "4.5.0"
}
```

---

## PADROES OBRIGATORIOS

### UNIT OF WORK
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    IVerticalRepository<T> VerticalRepository<T>() where T : class, IVerticalEntity;
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

REGRA: Repositories NUNCA chamam SaveChanges. Apenas UnitOfWork.

### IVERTICALENTITY
```csharp
public interface IVerticalEntity
{
    string VerticalType { get; }
    string VerticalProperties { get; set; }
    bool ValidateVerticalRules();
    Dictionary<string, object> GetVerticalConfiguration();
}
```

REGRA: TODA entidade de negócio DEVE implementar IVerticalEntity.

### MULTI-TENANT
```csharp
modelBuilder.Entity<BaseEntity>()
    .HasQueryFilter(e => e.TenantId == _tenantContext.GetCurrentTenantId());
```

REGRA: Global query filter SEMPRE ativo. Sem exceções.

---

## FRONTEND OBRIGATORIO

### NAVEGACAO TECLADO
```typescript
const TECLAS_ACAO = {
  'TAB': 'Próximo elemento',
  'SHIFT+TAB': 'Element anterior',
  'ENTER': 'Confirmar',
  'ESC': 'Cancelar',
  'F1': 'Vendas',
  'F2': 'Clientes',
  'F3': 'Produtos',
  'F4': 'Estoque',
  'F5': 'Fornecedores'
};
```

REGRA: TODOS componentes DEVEM implementar navegação por teclado.

### ACESSIBILIDADE WCAG AAA
```typescript
const PADROES = {
  fonteMinima: '16px',
  contrasteMinimo: '7:1',
  areaClicavel: '44px',
  espacamentoTouch: '12px'
};
```

REGRA: Contraste 7:1 obrigatório. Fonte menor que 16px proibida.

### HOOK PADRAO VERTICAL
```typescript
export function useVerticalEntity<T extends IVerticalEntity>(
  entityName: string,
  verticalType: string
) {
  // Implementação obrigatória com:
  // - Multi-tenant automático
  // - Vertical filtering
  // - Error handling
  // - Loading states
}
```

REGRA: SEMPRE usar useVerticalEntity para CRUD.

---

## DESENVOLVIMENTO LOCAL

### AMBIENTE
```bash
# Backend
cd CoreApp.Api && dotnet run --environment Development

# Frontend  
cd frontend && npm run dev

# Database
PostgreSQL 17 em localhost:5432
Database: coreapp_dev
```

### CONFIGURACAO DEVELOPMENT
```json
{
  "Cache": { "Enabled": false },
  "Logging": { "EntityFramework": "Information" },
  "SensitiveDataLogging": true
}
```

REGRA: Cache SEMPRE desabilitado em desenvolvimento.

---

## QUALITY GATES

### METRICAS MINIMAS
- Cobertura código: 80% mínimo, 90% em código crítico
- Bugs: 0 tolerância
- Vulnerabilidades: 0 tolerância  
- Duplicação: máximo 3%
- Complexidade ciclomática: máximo 10 por método
- Performance API: máximo 200ms por chamada

### CHECKLIST FEATURE
1. Implementar Domain com IVerticalEntity
2. Implementar Application com CQRS e validação
3. Implementar Infrastructure com UoW
4. Implementar API com DTOs e Swagger
5. Implementar Frontend com hooks e Mantine
6. Adicionar testes com 80% cobertura
7. Implementar navegação teclado
8. Validar WCAG AAA
9. Documentar com TSDoc/XML
10. Testar multi-tenant
11. Testar vertical específico

---

## COMANDOS EMERGENCIA

### RESET DESENVOLVIMENTO
```bash
rm -rf */bin */obj node_modules dist
dotnet clean && dotnet restore --no-cache
npm ci --cache /dev/null
```

### RESET DATABASE
```bash
dotnet ef database drop --force
dotnet ef database update
```

---

## DEFINICOES PRECISAS

### VERTICAL
Vertical = módulo de negócio específico (PADARIA, FARMACIA, SUPERMERCADO, DELIVERY).
Cada vertical tem regras próprias mas compartilha core.

### TENANT
Tenant = empresa/cliente usando o sistema.
Isolamento total de dados entre tenants.

### UNITOFWORK
Padrão que centraliza SaveChanges em uma única classe.
Repositories apenas preparam mudanças, UoW persiste.

### CQRS
Commands = operações que alteram estado.
Queries = operações que leem estado.
Separação total entre leitura e escrita.

---

Este documento define regras não-negociáveis. Execute conforme especificado.