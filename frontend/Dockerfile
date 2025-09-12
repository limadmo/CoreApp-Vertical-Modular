# Dockerfile determinístico para desenvolvimento
FROM node:20-alpine

WORKDIR /app

# Instalar serve globalmente
RUN npm install -g serve

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm install --legacy-peer-deps

# Copiar código fonte
COPY . .

# Expor porta
EXPOSE 3000

# Comando padrão para desenvolvimento com Vite
CMD ["npm", "run", "dev"]