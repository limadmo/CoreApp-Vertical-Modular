/**
 * Servidor Principal CoreApp
 * Inicialização completa da API Multi-tenant
 */

import { app } from './app';

const PORT = process.env.PORT || 5000;

async function startServer() {
  try {
    console.log(`
╔══════════════════════════════════════════════════════════════╗
║                      🚀 COREAPP API v1.0                     ║
║                  Sistema SAAS Multi-tenant                   ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║  🏗️  Arquitetura: Multi-tenant + JWT + Cache Resiliente     ║
║  🔐  Autenticação: JWT com Refresh Tokens                   ║
║  👥  Permissões: Role-based por Tenant                      ║
║  📊  Cache: Fallback 2→30min + Vendas Safety                ║
║  🇧🇷  Compliance: LGPD + Padrões Brasileiros                ║
║  📖  Docs: Swagger UI Completo                              ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
    `);

    const server = app.listen(PORT, () => {
      console.log(`
🎯 Servidor iniciado com sucesso!

🌐 Endpoints Principais:
   • API Base:         http://localhost:${PORT}/
   • Health Check:     http://localhost:${PORT}/health
   • Documentação:     http://localhost:${PORT}/api/docs
   • Schema JSON:      http://localhost:${PORT}/api/swagger.json

🔑 Autenticação:
   • Login User:       POST /api/auth/login
   • Login Admin:      POST /api/auth/super-admin/login  
   • Refresh Token:    POST /api/auth/refresh
   • Perfil:           GET /api/auth/profile

📋 Formato Credenciais:
   • Login:            XX00000X (Ex: AB12345C)
   • Senha:            X0000X   (Ex: A1234B)

🏢 Multi-tenancy:
   • Header:           X-Tenant-ID: seu-tenant-id
   • Isolamento:       Completo por tenant

🛡️  Segurança:
   • Rate Limit:       100 req/min por usuário
   • JWT Expiry:       15min (access) / 7dias (refresh)
   • Cache Fallback:   2→5→7→10→12→15→20→30min
   • Sales Safety:     Desabilitadas após 30min sem cache

💡 Para começar:
   1. Acesse http://localhost:${PORT}/api/docs
   2. Teste os endpoints na documentação interativa
   3. Use as credenciais demo ou crie via Super Admin

🔧 Ambiente: ${process.env.NODE_ENV || 'development'}
      `);

      // Log adicional em desenvolvimento
      if (process.env.NODE_ENV === 'development') {
        console.log(`
🔍 Desenvolvimento:
   • Logs SQL: Habilitados
   • CORS: localhost:3000, localhost:5173
   • Cache: Desabilitado (conforme CLAUDE.md)
   • Hot Reload: Use 'npm run dev'
        `);
      }
    });

    // Graceful shutdown
    process.on('SIGTERM', () => {
      console.log('\n🛑 SIGTERM recebido. Encerrando servidor...');
      server.close(() => {
        console.log('✅ Servidor encerrado com sucesso.');
        process.exit(0);
      });
    });

    process.on('SIGINT', () => {
      console.log('\n🛑 SIGINT recebido. Encerrando servidor...');
      server.close(() => {
        console.log('✅ Servidor encerrado com sucesso.');
        process.exit(0);
      });
    });

  } catch (error) {
    console.error('❌ Erro ao inicializar servidor:', error);
    process.exit(1);
  }
}

// Inicializar servidor
startServer();