/**
 * Configuração Swagger UI - Documentação API
 * Sistema completo de documentação interativa
 */

import swaggerJsdoc from 'swagger-jsdoc';
import { Express } from 'express';
import swaggerUi from 'swagger-ui-express';

/**
 * Configuração base do Swagger
 */
const swaggerOptions: swaggerJsdoc.Options = {
  definition: {
    openapi: '3.0.3',
    info: {
      title: 'CoreApp API - Sistema Multi-tenant',
      version: '1.0.0',
      description: `
## 🚀 CoreApp SAAS Multi-tenant API

Sistema completo de gestão comercial brasileiro com **arquitetura de verticais** e **multi-tenancy**.

### ⭐ Principais Características

- **🏗️ Multi-tenant**: Isolamento completo de dados por loja
- **🔐 JWT Authentication**: Sistema seguro com refresh tokens
- **👥 Roles & Permissions**: Sistema flexível de permissões por tenant
- **📊 Cache Resiliente**: Fallback automático com desabilitação de vendas
- **🇧🇷 100% Brasileiro**: PIX, LGPD, validações específicas

### 🔑 Autenticação

1. **Usuários Regulares**: Login via \`POST /api/auth/login\`
2. **Super Admin**: Login via \`POST /api/auth/super-admin/login\`
3. **Token Refresh**: Renovação via \`POST /api/auth/refresh\`

### 📋 Formato de Credenciais

- **Login**: \`XX00000X\` (2 letras + 5 números + 1 letra)
- **Senha**: \`X0000X\` (1 letra + 4 números + 1 letra)

### 🏢 Multi-tenancy

Todos os endpoints (exceto Super Admin) requerem identificação do tenant via:
- Header: \`X-Tenant-ID: seu-tenant-id\`
- Body: \`tenantId: "seu-tenant-id"\`

### 📊 Verticais Suportadas

- **PADARIA**: Produtos com validade em horas, ingredientes
- **FARMACIA**: Controle rigoroso de lotes e validades
- **SUPERMERCADO**: Gestão de categorias diversas
- **Customizável**: Sistema flexível sem enums

### 🔄 Cache Resiliente

O sistema implementa cache com fallback progressivo:
- Normal: 2 minutos
- Falha: 5 → 7 → 10 → 12 → 15 → 20 → 30 minutos
- **CRÍTICO**: Vendas desabilitadas após 30min sem cache

### 🛡️ Segurança

- Senhas com bcrypt (12 rounds)
- Rate limiting por usuário (100 req/min)
- Auditoria completa de ações
- Isolamento total entre tenants
- Bloqueio automático por tentativas falhadas
      `,
      contact: {
        name: 'CoreApp Support',
        email: 'suporte@coreapp.com.br',
        url: 'https://coreapp.com.br'
      },
      license: {
        name: 'Proprietário - CoreApp SAAS',
        url: 'https://coreapp.com.br/licenca'
      }
    },
    servers: [
      {
        url: 'http://localhost:5000',
        description: 'Desenvolvimento Local'
      },
      {
        url: 'https://api-dev.coreapp.com.br',
        description: 'Ambiente de Desenvolvimento'
      },
      {
        url: 'https://api.coreapp.com.br',
        description: 'Produção'
      }
    ],
    components: {
      securitySchemes: {
        bearerAuth: {
          type: 'http',
          scheme: 'bearer',
          bearerFormat: 'JWT',
          description: '**JWT Access Token**\n\nToken obtido via login que expira em 15 minutos.\n\n**Formato**: `Bearer <seu-jwt-token>`\n\n**Exemplo**: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`'
        },
        superAdminAuth: {
          type: 'http',
          scheme: 'bearer',
          bearerFormat: 'JWT',
          description: '**JWT Super Admin Token**\n\nToken especial para operações administrativas (criação de tenants, usuários, etc).\n\n**Obtido via**: `POST /api/auth/super-admin/login`\n\n**Permissões**: Acesso global a todos os tenants'
        }
      },
      parameters: {
        TenantHeader: {
          name: 'X-Tenant-ID',
          in: 'header',
          required: false,
          schema: {
            type: 'string',
            example: 'padaria-demo'
          },
          description: 'ID do tenant para isolamento de dados (obrigatório para usuários não Super Admin)'
        }
      },
      responses: {
        UnauthorizedError: {
          description: 'Token de autenticação ausente ou inválido',
          content: {
            'application/json': {
              schema: {
                type: 'object',
                properties: {
                  error: {
                    type: 'string',
                    example: 'Token de acesso não fornecido'
                  },
                  code: {
                    type: 'string',
                    example: 'MISSING_TOKEN'
                  }
                }
              }
            }
          }
        },
        ForbiddenError: {
          description: 'Permissões insuficientes',
          content: {
            'application/json': {
              schema: {
                type: 'object',
                properties: {
                  error: {
                    type: 'string',
                    example: 'Permissão necessária: vendas:criar'
                  },
                  code: {
                    type: 'string',
                    example: 'INSUFFICIENT_PERMISSIONS'
                  },
                  required: {
                    type: 'string',
                    example: 'vendas:criar'
                  },
                  userPermissions: {
                    type: 'array',
                    items: {
                      type: 'string'
                    },
                    example: ['produtos:visualizar', 'estoque:visualizar']
                  }
                }
              }
            }
          }
        },
        RateLimitError: {
          description: 'Limite de requisições excedido',
          content: {
            'application/json': {
              schema: {
                type: 'object',
                properties: {
                  error: {
                    type: 'string',
                    example: 'Muitas requisições, tente novamente em alguns segundos'
                  },
                  code: {
                    type: 'string',
                    example: 'RATE_LIMIT_EXCEEDED'
                  },
                  resetInSeconds: {
                    type: 'number',
                    example: 45
                  }
                }
              }
            }
          }
        },
        CacheFailureError: {
          description: 'Operação indisponível devido a falha no cache',
          content: {
            'application/json': {
              schema: {
                type: 'object',
                properties: {
                  error: {
                    type: 'string',
                    example: 'Vendas temporariamente desabilitadas devido a problemas de cache'
                  },
                  code: {
                    type: 'string',
                    example: 'SALES_DISABLED_CACHE_FAILURE'
                  },
                  retry: {
                    type: 'boolean',
                    example: true
                  },
                  retryAfter: {
                    type: 'number',
                    example: 60
                  }
                }
              }
            }
          }
        }
      }
    },
    tags: [
      {
        name: 'Autenticação',
        description: 'Endpoints de login, logout e gestão de tokens'
      },
      {
        name: 'Gestão de Usuários',
        description: 'CRUD de usuários e gestão de permissões (Super Admin)'
      },
      {
        name: 'Perfil',
        description: 'Gestão do perfil do usuário logado'
      },
      {
        name: 'Configuração',
        description: 'Configurações do sistema e regras de validação'
      },
      {
        name: 'Produtos',
        description: 'Gestão de produtos por vertical'
      },
      {
        name: 'Estoque',
        description: 'Controle de estoque e movimentações'
      },
      {
        name: 'Vendas',
        description: 'Sistema de vendas e PDV'
      },
      {
        name: 'Clientes',
        description: 'Gestão de clientes e CRM'
      },
      {
        name: 'Relatórios',
        description: 'Relatórios e dashboards'
      },
      {
        name: 'Sistema',
        description: 'Health check, logs e monitoramento'
      }
    ]
  },
  apis: [
    './src/controllers/*.ts', // Documentação nos controllers
    './src/routes/*.ts'       // Documentação adicional nas rotas
  ]
};

/**
 * Gera especificação Swagger
 */
export const swaggerSpec = swaggerJsdoc(swaggerOptions);

/**
 * Configuração do Swagger UI
 */
const swaggerUiOptions: swaggerUi.SwaggerUiOptions = {
  customCss: `
    .swagger-ui .topbar { display: none; }
    .swagger-ui .info { margin: 20px 0; }
    .swagger-ui .info .title { color: #2563eb; font-size: 2.5em; }
    .swagger-ui .scheme-container { background: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0; }
    .swagger-ui .btn.authorize { background-color: #059669; border-color: #059669; }
    .swagger-ui .btn.authorize:hover { background-color: #047857; border-color: #047857; }
    .swagger-ui .info .description p { font-size: 16px; line-height: 1.6; }
    .swagger-ui .opblock.opblock-post { border-color: #059669; }
    .swagger-ui .opblock.opblock-post .opblock-summary { border-color: #059669; background: rgba(5, 150, 105, 0.1); }
    .swagger-ui .opblock.opblock-get { border-color: #3b82f6; }
    .swagger-ui .opblock.opblock-get .opblock-summary { border-color: #3b82f6; background: rgba(59, 130, 246, 0.1); }
    .swagger-ui .opblock.opblock-put { border-color: #f59e0b; }
    .swagger-ui .opblock.opblock-put .opblock-summary { border-color: #f59e0b; background: rgba(245, 158, 11, 0.1); }
    .swagger-ui .opblock.opblock-delete { border-color: #ef4444; }
    .swagger-ui .opblock.opblock-delete .opblock-summary { border-color: #ef4444; background: rgba(239, 68, 68, 0.1); }
  `,
  customSiteTitle: 'CoreApp API - Documentação',
  customfavIcon: 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y=".9em" font-size="90">🏪</text></svg>',
  swaggerOptions: {
    persistAuthorization: true,
    docExpansion: 'none',
    filter: true,
    showRequestDuration: true,
    tryItOutEnabled: true,
    requestSnippetsEnabled: true,
    requestSnippets: {
      generators: {
        curl_bash: {
          title: 'cURL (Bash)',
          syntax: 'bash'
        },
        curl_powershell: {
          title: 'cURL (PowerShell)',
          syntax: 'powershell'
        },
        javascript_fetch: {
          title: 'JavaScript (Fetch)',
          syntax: 'javascript'
        }
      }
    }
  }
};

/**
 * Configura Swagger UI no Express app
 * @param app - Instância do Express
 */
export function setupSwagger(app: Express): void {
  // Servir especificação JSON
  app.get('/api/swagger.json', (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    res.send(swaggerSpec);
  });

  // Interface Swagger UI
  app.use('/api/docs', swaggerUi.serve);
  app.get('/api/docs', swaggerUi.setup(swaggerSpec, swaggerUiOptions));

  // Redirecionamentos convenientes
  app.get('/docs', (req, res) => {
    res.redirect('/api/docs');
  });

  app.get('/swagger', (req, res) => {
    res.redirect('/api/docs');
  });

  console.log(`
🚀 Swagger UI configurado com sucesso!

📖 Documentação disponível em:
   • http://localhost:5000/api/docs
   • http://localhost:5000/docs
   • http://localhost:5000/swagger

📋 JSON Schema disponível em:
   • http://localhost:5000/api/swagger.json

🔧 Recursos ativados:
   ✅ Autenticação JWT persistente
   ✅ Try It Out habilitado
   ✅ Filtros de endpoints
   ✅ Code snippets (cURL, JS, PowerShell)
   ✅ Request duration timing
   ✅ Interface customizada brasileira

💡 Para testar a API:
   1. Acesse /api/docs
   2. Clique em "Authorize" 
   3. Faça login via /api/auth/login
   4. Cole o token recebido
   5. Teste os endpoints!
  `);
}