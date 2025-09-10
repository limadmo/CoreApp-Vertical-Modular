#!/usr/bin/env python3
"""
Script para implementar automaticamente as interfaces ISoftDeletableEntity e IArchivableEntity
nas entidades que estão declarando as interfaces mas não têm as implementações
"""

import os
import re

# Template para ISoftDeletableEntity
SOFT_DELETE_TEMPLATE = '''
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
    }'''

# Template para IArchivableEntity
ARCHIVABLE_TEMPLATE = '''
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
    }'''

def process_entity_file(file_path):
    """Processa um arquivo de entidade adicionando as implementações necessárias"""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Verificar se precisa dos using statements
    if ('ISoftDeletableEntity' in content or 'IArchivableEntity' in content) and 'using CoreApp.Domain.Entities.Common;' not in content:
        # Adicionar using statement no início
        lines = content.split('\n')
        using_lines = []
        other_lines = []
        
        for line in lines:
            if line.strip().startswith('using ') and not line.strip().startswith('namespace'):
                using_lines.append(line)
            else:
                other_lines.append(line)
        
        # Adicionar novo using se não existir
        if not any('CoreApp.Domain.Entities.Common' in line for line in using_lines):
            using_lines.append('using CoreApp.Domain.Entities.Common;')
        
        # Reconstruir conteúdo
        content = '\n'.join(using_lines + other_lines)
    
    # Verificar se declara ISoftDeletableEntity mas não tem implementação
    if 'ISoftDeletableEntity' in content and 'public bool Excluido' not in content:
        # Encontrar última chave de fechamento da classe principal (antes dos enums)
        # Procurar por padrão de final de classe
        pattern = r'(\s*)\n(\s*)}(\s*\n\s*/// <summary>|\s*\n\s*public enum|\s*\n\s*$)'
        match = re.search(pattern, content)
        
        if match:
            # Inserir implementação antes do fechamento da classe
            insert_pos = match.start() + len(match.group(1))
            content = content[:insert_pos] + SOFT_DELETE_TEMPLATE + content[insert_pos:]
    
    # Verificar se declara IArchivableEntity mas não tem implementação
    if 'IArchivableEntity' in content and 'public DateTime UltimaMovimentacao' not in content:
        # Encontrar última chave de fechamento da classe principal
        pattern = r'(\s*)\n(\s*)}(\s*\n\s*/// <summary>|\s*\n\s*public enum|\s*\n\s*$)'
        match = re.search(pattern, content)
        
        if match:
            # Inserir implementação antes do fechamento da classe
            insert_pos = match.start() + len(match.group(1))
            content = content[:insert_pos] + ARCHIVABLE_TEMPLATE + content[insert_pos:]
    
    # Salvar se houve mudanças
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Processado: {file_path}")
        return True
    
    return False

def main():
    """Função principal"""
    entities_dir = 'CoreApp.Domain/Entities'
    
    if not os.path.exists(entities_dir):
        print(f"Diretório {entities_dir} não encontrado")
        return
    
    files_processed = 0
    
    # Processar todos os arquivos .cs no diretório Entities
    for root, dirs, files in os.walk(entities_dir):
        for file in files:
            if file.endswith('.cs'):
                file_path = os.path.join(root, file)
                if process_entity_file(file_path):
                    files_processed += 1
    
    print(f"\nProcessamento concluído! {files_processed} arquivos modificados.")

if __name__ == '__main__':
    main()