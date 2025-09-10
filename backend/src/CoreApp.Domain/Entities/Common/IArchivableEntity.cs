using System.ComponentModel.DataAnnotations;

namespace CoreApp.Domain.Entities.Common;

/// <summary>
/// Interface para entidades que podem ser arquivadas para otimização de performance
/// Permite arquivamento automático de dados antigos conforme políticas de retenção
/// </summary>
public interface IArchivableEntity
{
    /// <summary>
    /// Indica se a entidade foi arquivada
    /// </summary>
    bool Arquivado { get; set; }

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    DateTime? DataArquivamento { get; set; }

    /// <summary>
    /// Data da última movimentação/atividade da entidade
    /// </summary>
    DateTime UltimaMovimentacao { get; set; }

    /// <summary>
    /// Atualiza a data da última movimentação
    /// </summary>
    void AtualizarUltimaMovimentacao();
}