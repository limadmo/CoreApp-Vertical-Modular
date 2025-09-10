namespace CoreApp.Domain.Interfaces;

/// <summary>
/// Interface para entidades que podem ser arquivadas automaticamente
/// Dados com soft delete há mais de 5 anos serão movidos para tabelas _log
/// Implementa compliance farmacêutico brasileiro e otimização de performance
/// </summary>
/// <remarks>
/// Esta interface é essencial para o sistema de arquivamento automático
/// que garante compliance ANVISA mantendo performance das tabelas principais
/// </remarks>
public interface IArchivableEntity : ISoftDeletableEntity
{
    /// <summary>
    /// Data da última movimentação relevante para determinar arquivamento
    /// Utilizada para calcular quando o registro pode ser arquivado
    /// </summary>
    DateTime UltimaMovimentacao { get; set; }
    
    /// <summary>
    /// Indica se o registro já foi arquivado para tabela _log
    /// Evita arquivamento duplicado e permite rastreamento
    /// </summary>
    bool Arquivado { get; set; }
    
    /// <summary>
    /// Data do arquivamento para auditoria e compliance
    /// Registra quando o dado foi movido para tabela de arquivo
    /// </summary>
    DateTime? DataArquivamento { get; set; }
    
    /// <summary>
    /// Método para atualizar automaticamente a última movimentação
    /// Deve ser chamado sempre que a entidade for modificada
    /// </summary>
    void AtualizarUltimaMovimentacao();
}