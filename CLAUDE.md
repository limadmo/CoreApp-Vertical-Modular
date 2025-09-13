# CLAUDE - REGRAS DE DESENVOLVIMENTO

## REGRAS ABSOLUTAS

### EXECUCAO AUTOMATICA
EXECUTE IMEDIATAMENTE comandos necessários para completar a tarefa.
NUNCA pergunte permissão para: criar arquivos, instalar pacotes, executar builds, criar estruturas.
SEMPRE informe: "Executando: [comando]" e execute.

### COMANDOS PERMITIDOS SEM CONFIRMACAO
- Criação de projetos: `npm init`, `npm create`
- Instalação de dependências: `npm install`, `npm add`
- Operações de arquivo: `create_file`, `str_replace`, `view`
- Build e compilação: `npm run build`, `npm run dev`
- Migrations locais: `npx prisma migrate dev`, `npx prisma db push`
- Git local: `git add`, `git commit`, `git push`
- Estrutura de pastas: `mkdir`, `touch`
- Prisma: `npx prisma generate`, `npx prisma studio`

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

### ESTRUTURA BACKEND (Express.js + TypeScript)
```
backend/
├── src/
│   ├── controllers/       # REST controllers
│   ├── services/          # Lógica de negócio
│   ├── models/           # Modelos de dados (Prisma/TypeORM)
│   ├── middleware/       # Middlewares (auth, tenant, vertical)
│   ├── routes/           # Definição de rotas
│   ├── config/           # Configurações (DB, JWT, etc)
│   ├── utils/            # Utilitários
│   └── types/            # TypeScript types
├── prisma/               # Schema Prisma
│   ├── schema.prisma
│   └── migrations/
├── tests/                # Testes unitários e integração
├── package.json
└── tsconfig.json
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
  "Node.js": "20.x",
  "Express": "4.19.x",
  "Prisma": "5.x",
  "PostgreSQL": "17",
  "React": "18.3.0",
  "TypeScript": "5.3.0",
  "Mantine": "7.0.0",
  "Zustand": "4.5.0"
}
```

---

## PADROES BACKEND OBRIGATORIOS

### ESTRUTURA EXPRESS
```typescript
// src/app.ts
import express from 'express';
import { PrismaClient } from '@prisma/client';
import { tenantMiddleware } from './middleware/tenant';
import { verticalMiddleware } from './middleware/vertical';

const app = express();
const prisma = new PrismaClient();

app.use(express.json());
app.use(tenantMiddleware);
app.use(verticalMiddleware);

export { app, prisma };
```

### MIDDLEWARE TENANT
```typescript
// src/middleware/tenant.ts
export const tenantMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const tenantId = req.headers['x-tenant-id'] || 'demo';
  req.tenant = { id: tenantId };
  next();
};
```

### PRISMA SCHEMA BASE
```prisma
generator client {
  provider = "prisma-client-js"
}

datasource db {
  provider = "postgresql"
  url      = env("DATABASE_URL")
}

model BaseEntity {
  id        String   @id @default(cuid())
  tenantId  String
  createdAt DateTime @default(now())
  updatedAt DateTime @updatedAt
  
  @@index([tenantId])
}
```

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
# Backend Express.js
cd backend && npm run dev

# Frontend  
cd frontend && npm run dev

# Database
PostgreSQL 17 em localhost:5432
Database: coreapp_dev
```

### CONFIGURACAO DEVELOPMENT
```typescript
// src/config/development.ts
export const developmentConfig = {
  cache: { enabled: false },
  logging: { 
    level: 'debug',
    sql: true 
  },
  sensitiveDataLogging: true,
  cors: {
    origin: ['http://localhost:3000'],
    credentials: true
  }
};
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
1. Implementar Frontend com hooks e Mantine
2. Adicionar testes com 80% cobertura
3. Implementar navegação teclado
4. Validar WCAG AAA
5. Documentar com TSDoc
6. Testar com dados mock
7. Testar vertical específico

---

## COMANDOS EMERGENCIA

### RESET DESENVOLVIMENTO
```bash
rm -rf node_modules dist .next
npm ci --cache /dev/null
npx prisma generate --force-update
```

### RESET DATABASE
```bash
npx prisma db push --force-reset
npx prisma db seed
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
Padrão que centraliza transações em uma única classe.
Services apenas preparam mudanças, UoW persiste com Prisma.$transaction.

### CQRS
Commands = operações que alteram estado.
Queries = operações que leem estado.
Separação total entre leitura e escrita.

### PRISMA ORM
ORM TypeScript nativo com schema declarativo.
Substituiu Entity Framework com melhor performance.
Auto-completion e type safety completos.

---

Este documento define regras não-negociáveis. Execute conforme especificado.