# Multi-stage Dockerfile for Node.js Backend
# Suporte para desenvolvimento e produção

# ====================================
# STAGE 1: BASE IMAGE
# ====================================
FROM node:20-alpine AS base

# Instalar dependências essenciais incluindo OpenSSL para Prisma
RUN apk add --no-cache \
    curl \
    bash \
    tzdata \
    openssl \
    openssl-dev \
    && cp /usr/share/zoneinfo/America/Sao_Paulo /etc/localtime \
    && echo "America/Sao_Paulo" > /etc/timezone

# Configurar timezone
ENV TZ=America/Sao_Paulo

# Definir diretório de trabalho
WORKDIR /app

# Criar usuário não-root para segurança
RUN addgroup -g 1001 -S nodejs
RUN adduser -S backend -u 1001

# ====================================
# STAGE 2: DEPENDENCIES
# ====================================
FROM base AS dependencies

# Copiar arquivos de dependências
COPY package*.json ./
COPY prisma ./prisma/

# Instalar dependências
RUN npm ci --omit=dev && npm cache clean --force

# Gerar cliente Prisma
RUN npx prisma generate

# ====================================
# STAGE 3: DEVELOPMENT
# ====================================
FROM base AS development

# Instalar dependências de desenvolvimento
COPY package*.json ./
RUN npm ci && npm cache clean --force

# Copiar código fonte
COPY . .

# Gerar cliente Prisma
RUN npx prisma generate

# Criar diretórios necessários
RUN mkdir -p logs temp uploads

# Ajustar permissões
RUN chown -R backend:nodejs /app
USER backend

# Expor porta
EXPOSE 3001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3001/health || exit 1

# Comando padrão
CMD ["npm", "run", "dev"]

# ====================================
# STAGE 4: BUILD
# ====================================
FROM dependencies AS build

# Copiar código fonte
COPY . .

# Executar build
RUN npm run build

# Remover arquivos desnecessários
RUN rm -rf src tests .env.example

# ====================================
# STAGE 5: PRODUCTION
# ====================================
FROM base AS production

# Copiar dependências
COPY --from=dependencies /app/node_modules ./node_modules

# Copiar build da aplicação
COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./
COPY --from=build /app/prisma ./prisma/

# Criar diretórios necessários
RUN mkdir -p logs temp uploads

# Ajustar permissões
RUN chown -R backend:nodejs /app
USER backend

# Expor porta
EXPOSE 3001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3001/health || exit 1

# Comando padrão
CMD ["npm", "start"]