/**
 * Aplicação Express Principal
 * Configuração completa da API CoreApp Multi-tenant
 */

import express, { Request, Response } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import { PrismaClient } from '@prisma/client';
import { setupSwagger } from './config/swaggerConfig';
import authRoutes from './routes/authRoutes';
import clientesRoutes from './routes/clientes';
import produtosRoutes from './routes/produtos';
import vendasRoutes from './routes/vendas';
import estoqueRoutes from './routes/estoque';
import { cacheService } from './services/cacheService';

// Inicialização
const app = express();
const prisma = new PrismaClient({
  log: process.env.NODE_ENV === 'development' ? ['query', 'info', 'warn', 'error'] : ['error']
});

// ====================================
// MIDDLEWARE GLOBAL
// ====================================

// Segurança
app.use(helmet({
  crossOriginEmbedderPolicy: false, // Para Swagger UI funcionar
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"], // Para Swagger UI CSS
      scriptSrc: ["'self'", "'unsafe-inline'"], // Para Swagger UI JS
      imgSrc: ["'self'", "data:", "https:"]
    }
  }
}));

// CORS configurado para desenvolvimento e produção
app.use(cors({
  origin: process.env.CORS_ORIGIN || ['http://localhost:3000', 'http://localhost:5173'],
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'X-Tenant-ID']
}));

// Parse JSON
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Headers informativos
app.use((req, res, next) => {
  res.setHeader('X-Powered-By', 'CoreApp-API-v1.0');
  res.setHeader('X-API-Version', '1.0.0');
  next();
});

// ====================================
// DOCUMENTAÇÃO SWAGGER
// ====================================

setupSwagger(app);

// ====================================
// HEALTH CHECK SYSTEM
// ====================================

/**
 * @swagger
 * /health:
 *   get:
 *     summary: Health Check Completo
 *     description: Verifica status de todos os serviços do sistema
 *     tags: [Sistema]
 *     responses:
 *       200:
 *         description: Sistema funcionando normalmente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 status:
 *                   type: string
 *                   example: "healthy"
 *                 timestamp:
 *                   type: string
 *                   format: date-time
 *                 version:
 *                   type: string
 *                   example: "1.0.0"
 *                 services:
 *                   type: object
 *                   properties:
 *                     database:
 *                       type: object
 *                       properties:
 *                         status:
 *                           type: string
 *                           example: "connected"
 *                         responseTime:
 *                           type: string
 *                           example: "2ms"
 *                     cache:
 *                       type: object
 *                       properties:
 *                         status:
 *                           type: string
 *                           example: "operational"
 *                         fallbackLevel:
 *                           type: number
 *                           example: 0
 *                         salesEnabled:
 *                           type: boolean
 *                           example: true
 *       503:
 *         description: Sistema com problemas
 */
app.get('/health', async (req: Request, res: Response) => {
  const startTime = Date.now();
  
  try {
    // Test database connection
    const dbStart = Date.now();
    await prisma.$queryRaw`SELECT 1`;
    const dbTime = Date.now() - dbStart;

    // Get cache status
    const cacheStats = cacheService.getStats();
    
    const healthData = {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      version: '1.0.0',
      uptime: process.uptime(),
      services: {
        database: {
          status: 'connected',
          responseTime: `${dbTime}ms`
        },
        cache: {
          status: cacheStats.status.enabled ? 'operational' : 'disabled',
          fallbackLevel: cacheStats.status.fallbackLevel,
          salesEnabled: cacheService.isSalesEnabled(),
          keys: cacheStats.keys,
          hits: cacheStats.hits,
          misses: cacheStats.misses
        },
        memory: {
          used: `${Math.round(process.memoryUsage().heapUsed / 1024 / 1024)}MB`,
          total: `${Math.round(process.memoryUsage().heapTotal / 1024 / 1024)}MB`
        }
      },
      environment: process.env.NODE_ENV || 'development'
    };

    res.status(200).json(healthData);

  } catch (error) {
    console.error('[HEALTH] Health check failed:', error);
    
    res.status(503).json({
      status: 'unhealthy',
      timestamp: new Date().toISOString(),
      error: 'Database connection failed',
      services: {
        database: {
          status: 'disconnected',
          error: 'Connection failed'
        },
        cache: {
          status: 'unknown'
        }
      }
    });
  }
});

/**
 * @swagger
 * /api/status:
 *   get:
 *     summary: Status Resumido da API
 *     description: Status básico para monitoramento externo
 *     tags: [Sistema]
 *     responses:
 *       200:
 *         description: API operacional
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 status:
 *                   type: string
 *                   example: "ok"
 *                 message:
 *                   type: string
 *                   example: "CoreApp API is running"
 */
app.get('/api/status', (req: Request, res: Response) => {
  res.json({
    status: 'ok',
    message: 'CoreApp API is running',
    timestamp: new Date().toISOString()
  });
});

// ====================================
// ROTAS DA API
// ====================================

// Autenticação
app.use('/api/auth', authRoutes);

// Módulos de negócio
app.use('/api/clientes', clientesRoutes);
app.use('/api/produtos', produtosRoutes);
app.use('/api/vendas', vendasRoutes);
app.use('/api/estoque', estoqueRoutes);

// Rota raiz com informações da API
app.get('/', (req: Request, res: Response) => {
  res.json({
    name: 'CoreApp API',
    version: '1.0.0',
    description: 'Sistema SAAS Multi-tenant para gestão comercial',
    documentation: '/api/docs',
    status: '/health',
    endpoints: {
      auth: '/api/auth',
      clientes: '/api/clientes',
      produtos: '/api/produtos',
      vendas: '/api/vendas',
      estoque: '/api/estoque',
      docs: '/api/docs',
      health: '/health'
    },
    features: [
      'Multi-tenant Architecture',
      'JWT Authentication', 
      'Role-based Permissions',
      'Cache Resiliente',
      'Swagger Documentation',
      'LGPD Compliance'
    ]
  });
});

// ====================================
// ERROR HANDLING
// ====================================

// Handler para rotas não encontradas
app.use('*', (req: Request, res: Response) => {
  res.status(404).json({
    error: 'Endpoint não encontrado',
    method: req.method,
    path: req.originalUrl,
    suggestion: 'Verifique a documentação em /api/docs'
  });
});

// Global error handler
app.use((error: any, req: Request, res: Response, next: any) => {
  console.error('[GLOBAL_ERROR]', {
    error: error.message,
    stack: error.stack,
    url: req.url,
    method: req.method,
    timestamp: new Date().toISOString()
  });

  // Erro de validação do Prisma
  if (error.code === 'P2002') {
    return res.status(400).json({
      error: 'Violação de restrição única',
      details: 'Um registro com estes dados já existe'
    });
  }

  // Erro de JSON malformado
  if (error instanceof SyntaxError && 'body' in error) {
    return res.status(400).json({
      error: 'JSON inválido no corpo da requisição'
    });
  }

  // Erro genérico
  res.status(500).json({
    error: 'Erro interno do servidor',
    ...(process.env.NODE_ENV === 'development' && {
      details: error.message,
      stack: error.stack
    })
  });
});

// ====================================
// GRACEFUL SHUTDOWN
// ====================================

process.on('SIGTERM', async () => {
  console.log('[SHUTDOWN] SIGTERM recebido, encerrando graciosamente...');
  
  // Fechar conexões do Prisma
  await prisma.$disconnect();
  
  // Limpar cache se necessário
  cacheService.flush();
  
  process.exit(0);
});

process.on('SIGINT', async () => {
  console.log('[SHUTDOWN] SIGINT recebido, encerrando graciosamente...');
  
  await prisma.$disconnect();
  cacheService.flush();
  
  process.exit(0);
});

export { app, prisma };