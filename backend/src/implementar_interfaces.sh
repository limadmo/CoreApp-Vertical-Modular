#!/bin/bash

# Script para implementar ISoftDeletableEntity e IArchivableEntity nas entidades
# Adiciona as propriedades e métodos necessários ao final de cada classe

INTERFACE_IMPLEMENTATION='
    // Implementação de ISoftDeletableEntity

    /// <summary>
    /// Indica se a entidade foi excluída logicamente
    /// </summary>
    public bool Excluido { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DataExclusao { get; set; }

    /// <summary>
    /// Usuário responsável pela exclusão
    /// </summary>
    [StringLength(100)]
    public string? UsuarioExclusao { get; set; }

    /// <summary>
    /// Motivo da exclusão para auditoria
    /// </summary>
    [StringLength(500)]
    public string? MotivoExclusao { get; set; }

    /// <summary>
    /// Marca a entidade como excluída logicamente
    /// </summary>
    public void MarkAsDeleted(string? usuarioId = null, string? motivo = null)
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        UsuarioExclusao = usuarioId;
        MotivoExclusao = motivo;
    }

    /// <summary>
    /// Restaura uma entidade excluída logicamente
    /// </summary>
    public void Restore()
    {
        Excluido = false;
        DataExclusao = null;
        UsuarioExclusao = null;
        MotivoExclusao = null;
    }'

ARCHIVABLE_IMPLEMENTATION='
    // Implementação de IArchivableEntity

    /// <summary>
    /// Indica se a entidade foi arquivada
    /// </summary>
    public bool Arquivado { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? DataArquivamento { get; set; }

    /// <summary>
    /// Data da última movimentação/atividade da entidade
    /// </summary>
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Atualiza a data da última movimentação
    /// </summary>
    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
    }'

# Lista de arquivos a processar (exceto ClienteEntity que já foi feito)
FILES=(
    "CoreApp.Domain/Entities/FormaPagamentoVendaEntity.cs"
    "CoreApp.Domain/Entities/FornecedorEntity.cs"
    "CoreApp.Domain/Entities/ItemVendaEntity.cs"
    "CoreApp.Domain/Entities/LoteEntity.cs"
    "CoreApp.Domain/Entities/MovimentacaoEstoqueEntity.cs"
    "CoreApp.Domain/Entities/ProdutoEntity.cs"
    "CoreApp.Domain/Entities/PromocaoEntity.cs"
    "CoreApp.Domain/Entities/TipoMovimentacaoEntity.cs"
    "CoreApp.Domain/Entities/UsuarioEntity.cs"
    "CoreApp.Domain/Entities/VendaEntity.cs"
)

for file in "${FILES[@]}"; do
    echo "Processando $file..."
    
    # Verificar se arquivo existe
    if [ ! -f "$file" ]; then
        echo "Arquivo $file não encontrado"
        continue
    fi
    
    # Verificar se já tem as implementações
    if grep -q "DataExclusao\|UsuarioExclusao" "$file"; then
        echo "Arquivo $file já tem implementação de ISoftDeletableEntity"
    else
        # Adicionar using se necessário
        if ! grep -q "using CoreApp.Domain.Entities.Common;" "$file"; then
            sed -i '/using CoreApp.Domain.Enums;/a using CoreApp.Domain.Entities.Common;' "$file"
        fi
        
        # Encontrar a última linha antes do fechamento da classe principal
        # e adicionar as implementações
        if grep -q "ISoftDeletableEntity" "$file"; then
            # Encontrar linha com fechamento de classe (última }) antes dos enums
            LAST_BRACE=$(grep -n "^}" "$file" | head -1 | cut -d: -f1)
            if [ -n "$LAST_BRACE" ]; then
                # Inserir implementação antes do fechamento
                sed -i "${LAST_BRACE}i\\${INTERFACE_IMPLEMENTATION}" "$file"
            fi
        fi
    fi
    
    # Verificar IArchivableEntity
    if grep -q "UltimaMovimentacao\|DataArquivamento" "$file"; then
        echo "Arquivo $file já tem implementação de IArchivableEntity"
    else
        if grep -q "IArchivableEntity" "$file"; then
            # Encontrar linha com fechamento de classe e adicionar implementação
            LAST_BRACE=$(grep -n "^}" "$file" | head -1 | cut -d: -f1)
            if [ -n "$LAST_BRACE" ]; then
                sed -i "${LAST_BRACE}i\\${ARCHIVABLE_IMPLEMENTATION}" "$file"
            fi
        fi
    fi
    
    echo "Concluído: $file"
done

echo "Script concluído!"