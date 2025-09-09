using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade de associação entre promoções e produtos específicos
/// Permite configurar promoções direcionadas a produtos individuais
/// </summary>
/// <remarks>
/// Esta entidade implementa o relacionamento many-to-many entre promoções e produtos,
/// permitindo que uma promoção seja aplicada a produtos específicos e que um produto
/// possa participar de múltiplas promoções com prioridades diferentes.
/// </remarks>
public class PromocaoProdutoEntity : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador único da associação promoção-produto
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) - isolamento automático
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da promoção
    /// </summary>
    [Required]
    public Guid PromocaoId { get; set; }

    /// <summary>
    /// Promoção relacionada
    /// </summary>
    public PromocaoEntity Promocao { get; set; } = null!;

    /// <summary>
    /// Identificador do produto
    /// </summary>
    [Required]
    public Guid ProdutoId { get; set; }

    /// <summary>
    /// Produto relacionado
    /// </summary>
    public ProdutoEntity Produto { get; set; } = null!;

    /// <summary>
    /// Valor específico de desconto para este produto (sobrescreve o da promoção)
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor de desconto deve ser positivo")]
    public decimal? ValorDescontoEspecifico { get; set; }

    /// <summary>
    /// Percentual específico de desconto para este produto
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal? PercentualDescontoEspecifico { get; set; }

    /// <summary>
    /// Quantidade mínima deste produto para aplicar a promoção
    /// </summary>
    [Range(1, 9999, ErrorMessage = "Quantidade mínima deve ser positiva")]
    public int QuantidadeMinimaEspecifica { get; set; } = 1;

    /// <summary>
    /// Quantidade máxima deste produto que pode ter desconto
    /// </summary>
    [Range(1, 9999, ErrorMessage = "Quantidade máxima deve ser positiva")]
    public int? QuantidadeMaximaDesconto { get; set; }

    /// <summary>
    /// Preço mínimo do produto para aplicar a promoção
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Preço mínimo deve ser positivo")]
    public decimal? PrecoMinimo { get; set; }

    /// <summary>
    /// Preço máximo do produto para aplicar a promoção
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Preço máximo deve ser positivo")]
    public decimal? PrecoMaximo { get; set; }

    /// <summary>
    /// Indica se este produto é obrigatório na promoção
    /// </summary>
    public bool IsProdutoObrigatorio { get; set; } = false;

    /// <summary>
    /// Indica se este produto é um brinde na promoção
    /// </summary>
    public bool IsBrinde { get; set; } = false;

    /// <summary>
    /// Prioridade específica deste produto na promoção
    /// </summary>
    [Range(0, 100, ErrorMessage = "Prioridade deve estar entre 0 e 100")]
    public int Prioridade { get; set; } = 0;

    /// <summary>
    /// Condições especiais para este produto (JSON)
    /// </summary>
    [StringLength(1000)]
    public string? CondicoesEspeciais { get; set; }

    /// <summary>
    /// Observações específicas sobre este produto na promoção
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades de auditoria
    /// <summary>
    /// Data de criação do registro
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou o registro
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Verifica se o produto atende aos critérios de preço da promoção
    /// </summary>
    /// <param name="precoAtual">Preço atual do produto</param>
    /// <returns>True se atende aos critérios</returns>
    public bool AtendePreco(decimal precoAtual)
    {
        if (PrecoMinimo.HasValue && precoAtual < PrecoMinimo.Value)
            return false;

        if (PrecoMaximo.HasValue && precoAtual > PrecoMaximo.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se a quantidade especificada atende aos critérios mínimos
    /// </summary>
    /// <param name="quantidade">Quantidade do produto</param>
    /// <returns>True se atende aos critérios</returns>
    public bool AtendeQuantidade(int quantidade)
    {
        return quantidade >= QuantidadeMinimaEspecifica;
    }

    /// <summary>
    /// Calcula a quantidade que pode ter desconto aplicado
    /// </summary>
    /// <param name="quantidadeTotal">Quantidade total do produto</param>
    /// <returns>Quantidade que terá desconto</returns>
    public int CalcularQuantidadeComDesconto(int quantidadeTotal)
    {
        if (!AtendeQuantidade(quantidadeTotal))
            return 0;

        if (QuantidadeMaximaDesconto.HasValue)
            return Math.Min(quantidadeTotal, QuantidadeMaximaDesconto.Value);

        return quantidadeTotal;
    }

    /// <summary>
    /// Obtém o valor de desconto específico ou da promoção
    /// </summary>
    /// <returns>Valor de desconto a ser aplicado</returns>
    public decimal ObterValorDesconto()
    {
        return ValorDescontoEspecifico ?? Promocao?.ValorDesconto ?? 0;
    }

    /// <summary>
    /// Obtém o percentual de desconto específico ou da promoção
    /// </summary>
    /// <returns>Percentual de desconto a ser aplicado</returns>
    public decimal ObterPercentualDesconto()
    {
        return PercentualDescontoEspecifico ?? Promocao?.PercentualDesconto ?? 0;
    }

    /// <summary>
    /// Calcula o desconto para este produto específico
    /// </summary>
    /// <param name="valorProduto">Valor total do produto</param>
    /// <param name="quantidade">Quantidade do produto</param>
    /// <returns>Valor do desconto calculado</returns>
    public decimal CalcularDesconto(decimal valorProduto, int quantidade)
    {
        if (!AtendeQuantidade(quantidade) || !AtendePreco(valorProduto / quantidade))
            return 0;

        var quantidadeComDesconto = CalcularQuantidadeComDesconto(quantidade);
        var valorComDesconto = (valorProduto / quantidade) * quantidadeComDesconto;

        decimal desconto = 0;

        if (ValorDescontoEspecifico.HasValue)
        {
            desconto = ValorDescontoEspecifico.Value * quantidadeComDesconto;
        }
        else if (PercentualDescontoEspecifico.HasValue)
        {
            desconto = valorComDesconto * (PercentualDescontoEspecifico.Value / 100);
        }
        else if (Promocao != null)
        {
            desconto = Promocao.CalcularDesconto(valorComDesconto);
        }

        return Math.Min(desconto, valorProduto);
    }

    /// <summary>
    /// Verifica se este produto tem configurações específicas
    /// </summary>
    /// <returns>True se tem configurações específicas</returns>
    public bool TemConfiguracaoEspecifica()
    {
        return ValorDescontoEspecifico.HasValue ||
               PercentualDescontoEspecifico.HasValue ||
               QuantidadeMinimaEspecifica > 1 ||
               QuantidadeMaximaDesconto.HasValue ||
               PrecoMinimo.HasValue ||
               PrecoMaximo.HasValue;
    }

    /// <summary>
    /// Obtém resumo das configurações específicas
    /// </summary>
    /// <returns>String com resumo das configurações</returns>
    public string ObterResumoConfiguracoes()
    {
        var configs = new List<string>();

        if (ValorDescontoEspecifico.HasValue)
            configs.Add($"Desconto: R$ {ValorDescontoEspecifico.Value:F2}");

        if (PercentualDescontoEspecifico.HasValue)
            configs.Add($"Desconto: {PercentualDescontoEspecifico.Value}%");

        if (QuantidadeMinimaEspecifica > 1)
            configs.Add($"Qtd mín: {QuantidadeMinimaEspecifica}");

        if (QuantidadeMaximaDesconto.HasValue)
            configs.Add($"Qtd máx desconto: {QuantidadeMaximaDesconto.Value}");

        if (PrecoMinimo.HasValue)
            configs.Add($"Preço mín: R$ {PrecoMinimo.Value:F2}");

        if (PrecoMaximo.HasValue)
            configs.Add($"Preço máx: R$ {PrecoMaximo.Value:F2}");

        if (IsProdutoObrigatorio)
            configs.Add("Obrigatório");

        if (IsBrinde)
            configs.Add("Brinde");

        return configs.Any() ? string.Join(" | ", configs) : "Configuração padrão";
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da associação</returns>
    public override string ToString()
    {
        var produto = Produto?.Nome ?? $"Produto {ProdutoId}";
        var promocao = Promocao?.Nome ?? $"Promoção {PromocaoId}";
        return $"{promocao} -> {produto}";
    }
}

/// <summary>
/// Entidade de associação entre promoções e categorias de produtos
/// Permite aplicar promoções a categorias inteiras
/// </summary>
public class PromocaoCategoriaEntity : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador único da associação promoção-categoria
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) - isolamento automático
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da promoção
    /// </summary>
    [Required]
    public Guid PromocaoId { get; set; }

    /// <summary>
    /// Promoção relacionada
    /// </summary>
    public PromocaoEntity Promocao { get; set; } = null!;

    /// <summary>
    /// Identificador da categoria
    /// </summary>
    [Required]
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Categoria relacionada
    /// </summary>
    public CategoriaEntity Categoria { get; set; } = null!;

    /// <summary>
    /// Valor específico de desconto para produtos desta categoria
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "Valor de desconto deve ser positivo")]
    public decimal? ValorDescontoEspecifico { get; set; }

    /// <summary>
    /// Percentual específico de desconto para produtos desta categoria
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentual de desconto deve estar entre 0 e 100")]
    public decimal? PercentualDescontoEspecifico { get; set; }

    /// <summary>
    /// Indica se inclui subcategorias
    /// </summary>
    public bool IncluiSubcategorias { get; set; } = true;

    /// <summary>
    /// Filtros específicos para produtos desta categoria (JSON)
    /// </summary>
    [StringLength(1000)]
    public string? FiltrosEspecificos { get; set; }

    /// <summary>
    /// Observações sobre esta categoria na promoção
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades de auditoria
    /// <summary>
    /// Data de criação do registro
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou o registro
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Obtém o valor de desconto específico ou da promoção
    /// </summary>
    /// <returns>Valor de desconto a ser aplicado</returns>
    public decimal ObterValorDesconto()
    {
        return ValorDescontoEspecifico ?? Promocao?.ValorDesconto ?? 0;
    }

    /// <summary>
    /// Obtém o percentual de desconto específico ou da promoção
    /// </summary>
    /// <returns>Percentual de desconto a ser aplicado</returns>
    public decimal ObterPercentualDesconto()
    {
        return PercentualDescontoEspecifico ?? Promocao?.PercentualDesconto ?? 0;
    }

    /// <summary>
    /// Verifica se um produto pertence a esta categoria (considerando subcategorias se configurado)
    /// </summary>
    /// <param name="produto">Produto a ser verificado</param>
    /// <returns>True se o produto pertence à categoria</returns>
    public bool ProdutoPertenceCategoria(ProdutoEntity produto)
    {
        if (produto.CategoriaId == CategoriaId)
            return true;

        // Se inclui subcategorias, verificar hierarquia
        if (IncluiSubcategorias && produto.Categoria != null)
        {
            // Implementar lógica de hierarquia de categorias se necessário
            // Por exemplo, verificar se a categoria do produto é filha desta categoria
        }

        return false;
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string da associação</returns>
    public override string ToString()
    {
        var categoria = Categoria?.Nome ?? $"Categoria {CategoriaId}";
        var promocao = Promocao?.Nome ?? $"Promoção {PromocaoId}";
        return $"{promocao} -> Categoria: {categoria}";
    }
}