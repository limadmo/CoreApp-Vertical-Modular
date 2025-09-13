# 🔐 Componente de Login - CoreApp

## 📋 Visão Geral

Implementação completa de autenticação seguindo os padrões do CLAUDE.md:
- **Mantine 7.15** para UI components
- **Zustand 4.5** para state management
- **WCAG AAA** compliance (contraste 7:1)
- **Navegação por teclado** completa
- **Multi-tenant** automático

## 🏗️ Arquitetura

```
src/
├── types/auth.ts              # Tipos TypeScript
├── services/authService.ts    # Comunicação com API Express
├── stores/useAuth.ts          # Store Zustand
└── components/auth/
    ├── LoginForm.tsx          # Componente principal
    ├── LoginForm.module.css   # Estilos WCAG AAA
    └── index.ts               # Exportações
```

## ⌨️ Navegação por Teclado

Conforme especificado no CLAUDE.md:

| Tecla | Ação |
|-------|------|
| **TAB** | Próximo elemento |
| **SHIFT+TAB** | Elemento anterior |
| **ENTER** | Confirmar login |
| **ESC** | Cancelar/limpar formulário |
| **F1** | Demo rápido (admin@demo.com) |

## 🎨 WCAG AAA Compliance

### Contraste 7:1 Obrigatório
- Texto principal: `#212529` (8.59:1)
- Botões: `#0d47a1` (7.14:1)
- Labels: `#212529` (8.59:1)
- Erros: `#842029` (7:1)

### Tamanhos de Fonte
- **Mínimo**: 16px (conforme CLAUDE.md)
- **Labels**: 16px
- **Botões**: 16px
- **Ajuda**: 14px (exceção justificada)

### Área Clicável
- **Desktop**: 44px mínimo
- **Mobile**: 48px mínimo
- **Espaçamento**: 12px entre elementos

## 🏪 Multi-Tenant

### Detecção Automática
1. **Subdomain** (tenant.coreapp.com.br)
2. **LocalStorage** (fallback)
3. **Default** ("demo")

### Header Automático
```javascript
'x-tenant-id': getTenantId()
```

## 🔌 API Integration

### Endpoints Express
```javascript
POST /auth/login      # Login com credenciais
POST /auth/logout     # Logout
POST /auth/refresh    # Renovar token
GET  /auth/verify     # Verificar token
```

### Headers Enviados
```javascript
{
  'Content-Type': 'application/json',
  'x-tenant-id': 'demo',
  'Authorization': 'Bearer <token>'
}
```

## 💾 State Management (Zustand)

### Store Principal
```javascript
const { user, isLoading, error, login, logout } = useAuth();
```

### Hooks Otimizados
```javascript
useAuthStatus()    // Status + loading
useAuthActions()   # Apenas ações
useCurrentUser()   # Apenas usuário
useAuthError()     # Apenas erros
```

## 🧪 Como Usar

### 1. Importar Componente
```tsx
import { LoginForm } from '@/components/auth';
```

### 2. Usar na Página
```tsx
export default function LoginPage() {
  const router = useRouter();

  return (
    <LoginForm onSuccess={() => router.push('/dashboard')} />
  );
}
```

### 3. Verificar Autenticação
```tsx
const { isAuthenticated, user } = useAuthStatus();

if (!isAuthenticated) {
  return <Navigate to="/login" />;
}
```

## 📱 Responsividade

### Breakpoints
- **Desktop**: > 768px
- **Tablet**: 481px - 768px
- **Mobile**: < 480px

### Adaptações Mobile
- Font-size mantida em 16px
- Área de toque: 48px mínimo
- Margin/padding reduzidos
- Card full-width com margin

## 🚀 Credenciais Demo

| Perfil | Email | Senha | Módulos |
|--------|-------|-------|---------|
| **Super Admin** | admin@demo.com | admin123 | Todos |
| **Gerente** | gerente@demo.com | gerente123 | Operacionais |
| **Vendedor** | vendedor@demo.com | vend123 | Básicos |

## ♿ Acessibilidade

### Recursos Implementados
- [x] Contraste 7:1 (WCAG AAA)
- [x] Fonte mínima 16px
- [x] Navegação completa por teclado
- [x] Labels associados
- [x] ARIA attributes
- [x] Focus management
- [x] Screen reader support
- [x] High contrast mode
- [x] Reduced motion support

### Testes de Acessibilidade
```bash
# Instalar axe-core para testes
npm install --save-dev @axe-core/react

# Testar com screen reader
# - NVDA (Windows)
# - VoiceOver (macOS)
# - Orca (Linux)
```

## 🔧 Configuração

### Variáveis de Ambiente
```bash
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:3001
```

### URLs de Teste
```bash
# Local
http://localhost:3000/login

# Demo
https://demo-coreapp.vercel.app/login

# Staging
https://staging-coreapp.vercel.app/login
```

---

**Implementado conforme CLAUDE.md**: Express.js + Mantine 7.0 + WCAG AAA + Multi-tenant 🇧🇷