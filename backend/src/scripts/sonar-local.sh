#!/bin/bash

echo "🔍 Executando análise SonarQube local..."

# Carregar variáveis de ambiente
if [ -f "/home/diego/projetos/.env" ]; then
    echo "📄 Carregando configurações do .env..."
    source /home/diego/projetos/.env
else
    echo "⚠️ Arquivo .env não encontrado!"
    exit 1
fi

# Verificar se token existe
if [ -z "$SONAR_TOKEN" ]; then
    echo "❌ SONAR_TOKEN não encontrado no .env"
    echo "💡 Configure SONAR_TOKEN no arquivo .env"
    exit 1
fi

# Criar diretório de scripts se não existir
mkdir -p scripts

cd /home/diego/projetos/backend/src

echo "📦 Limpando projeto..."
dotnet clean

echo "🚀 Iniciando SonarScanner com autenticação..."
dotnet sonarscanner begin \
  /k:"CoreApp" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.coverage.exclusions="**/Migrations/**,**/bin/**,**/obj/**" \
  /d:sonar.exclusions="**/Migrations/**,**/bin/**,**/obj/**"

if [ $? -ne 0 ]; then
    echo "⚠️ SonarScanner falhou no início."
    echo "🔍 Verifique se o token está correto e o projeto 'CoreApp' existe no SonarQube"
    echo "🌐 Acesse: http://localhost:9000/projects"
    exit 1
fi

echo "🔨 Executando build do projeto..."
dotnet build --configuration Release

echo "🧪 Executando testes..."
dotnet test --no-build --configuration Release

echo "📊 Finalizando análise SonarQube..."
dotnet sonarscanner end

echo "✅ Análise concluída!"
echo "🌐 Acesse: http://localhost:9000/projects"