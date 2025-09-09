using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;
using Farmacia.Domain.Entities.Configuration;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um produto farmacêutico no sistema brasileiro
/// Inclui medicamentos, cosméticos, perfumaria e produtos de higiene pessoal
/// </summary>
/// <remarks>
/// Esta entidade implementa todas as regulamentações brasileiras:
/// - Classificação ANVISA para medicamentos controlados
/// - Código de barras EAN-13 padrão brasileiro
/// - Preços com impostos brasileiros (ICMS, PIS/COFINS)
/// - Controle de lotes e validade obrigatórios
/// </remarks>
[Table("Produtos")]
public class ProdutoEntity : ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único do produto
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    // Dados básicos do produto

    /// <summary>
    /// Nome comercial completo do produto
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Nome genérico ou princípio ativo
    /// </summary>
    [StringLength(200)]
    public string? NomeGenerico { get; set; }

    /// <summary>
    /// Descrição detalhada do produto
    /// </summary>
    [StringLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Princípio ativo principal
    /// </summary>
    [StringLength(200)]
    public string? PrincipioAtivo { get; set; }

    /// <summary>
    /// Concentração/dosagem (ex: "500mg", "10ml")
    /// </summary>
    [StringLength(50)]
    public string? Concentracao { get; set; }

    /// <summary>
    /// Forma farmacêutica (comprimido, xarope, pomada, etc.)
    /// </summary>
    [StringLength(50)]
    public string? FormaFarmaceutica { get; set; }

    /// <summary>
    /// Apresentação comercial (caixa com X unidades)
    /// </summary>
    [StringLength(100)]
    public string? Apresentacao { get; set; }

    // Identificação e códigos

    /// <summary>
    /// Código de barras EAN-13 (padrão brasileiro)
    /// </summary>
    [StringLength(13)]
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// Código interno da farmácia
    /// </summary>
    [StringLength(50)]
    public string? CodigoInterno { get; set; }

    /// <summary>
    /// Código do fornecedor para este produto
    /// </summary>
    [StringLength(50)]
    public string? CodigoFornecedor { get; set; }

    /// <summary>
    /// Registro ANVISA do produto (quando aplicável)
    /// </summary>
    [StringLength(30)]
    public string? RegistroAnvisa { get; set; }

    /// <summary>
    /// Código NCM para classificação fiscal
    /// </summary>
    [StringLength(10)]
    public string? CodigoNCM { get; set; }

    /// <summary>
    /// Código CEST (Código Especificador da Substituição Tributária)
    /// </summary>
    [StringLength(10)]
    public string? CodigoCEST { get; set; }

    // Classificação ANVISA e controle

    /// <summary>
    /// Código da classificação ANVISA (null para isentos, ou código da lista: A1, A2, A3, B1, B2, C1, C2, C3, C4, C5)
    /// </summary>
    /// <remarks>
    /// Se NULL ou vazio = medicamento isento (não precisa de receita especial)
    /// Se preenchido = medicamento controlado que requer receita conforme lista ANVISA
    /// </remarks>
    [StringLength(10)]
    public string? ClassificacaoAnvisaCodigo { get; set; }

    /// <summary>
    /// Se é medicamento controlado (listas A, B, C da ANVISA)
    /// </summary>
    public bool MedicamentoControlado { get; set; } = false;

    /// <summary>
    /// Navegação para a configuração da classificação ANVISA
    /// </summary>
    public virtual ClassificacaoAnvisaEntity? ClassificacaoAnvisa { get; set; }

    /// <summary>
    /// Tipo de receita necessária (quando controlado)
    /// </summary>
    [StringLength(30)]
    public string? TipoReceitaNecessaria { get; set; }

    /// <summary>
    /// Se requer retenção de receita
    /// </summary>
    public bool RequerRetencaoReceita { get; set; } = false;

    /// <summary>
    /// Quantidade máxima permitida por receita (medicamentos controlados)
    /// </summary>
    public int? QuantidadeMaximaPorReceita { get; set; }

    /// <summary>
    /// Prazo de validade da receita em dias
    /// </summary>
    public int? PrazoValidadeReceita { get; set; }

    // Dados do fabricante/laboratório

    /// <summary>
    /// Nome do laboratório/fabricante
    /// </summary>
    [StringLength(100)]
    public string? Laboratorio { get; set; }

    /// <summary>
    /// CNPJ do laboratório
    /// </summary>
    [StringLength(18)]
    public string? LaboratorioCnpj { get; set; }

    /// <summary>
    /// País de origem do produto
    /// </summary>
    [StringLength(50)]
    public string PaisOrigem { get; set; } = "Brasil";

    /// <summary>
    /// Se é medicamento genérico
    /// </summary>
    public bool MedicamentoGenerico { get; set; } = false;

    /// <summary>
    /// Se é medicamento similar
    /// </summary>
    public bool MedicamentoSimilar { get; set; } = false;

    /// <summary>
    /// Medicamento de referência (quando genérico/similar)
    /// </summary>
    [StringLength(200)]
    public string? MedicamentoReferencia { get; set; }

    // Categoria e classificação

    /// <summary>
    /// Categoria principal (MEDICAMENTO, COSMETICO, PERFUMARIA, HIGIENE, SUPLEMENTO)
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Categoria { get; set; } = "MEDICAMENTO";

    /// <summary>
    /// Subcategoria específica
    /// </summary>
    [StringLength(50)]
    public string? Subcategoria { get; set; }

    /// <summary>
    /// Classe terapêutica (para medicamentos)
    /// </summary>
    [StringLength(100)]
    public string? ClasseTerapeutica { get; set; }

    /// <summary>
    /// Se produto está ativo para venda
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Se produto está em destaque/promoção
    /// </summary>
    public bool EmDestaque { get; set; } = false;

    // Dados de estoque

    /// <summary>
    /// Quantidade atual em estoque
    /// </summary>
    [Column(TypeName = "decimal(15,3)")]
    public decimal EstoqueAtual { get; set; } = 0;

    /// <summary>
    /// Estoque mínimo para reposição
    /// </summary>
    [Column(TypeName = "decimal(15,3)")]
    public decimal EstoqueMinimo { get; set; } = 0;

    /// <summary>
    /// Estoque máximo ideal
    /// </summary>
    [Column(TypeName = "decimal(15,3)")]
    public decimal EstoqueMaximo { get; set; } = 0;

    /// <summary>
    /// Ponto de ressuprimento
    /// </summary>
    [Column(TypeName = "decimal(15,3)")]
    public decimal PontoRessuprimento { get; set; } = 0;

    /// <summary>
    /// Unidade de medida (UN, CX, FR, CP, etc.)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string UnidadeMedida { get; set; } = "UN";

    /// <summary>
    /// Se controla lote obrigatoriamente
    /// </summary>
    public bool ControlaLote { get; set; } = false;

    /// <summary>
    /// Se controla validade obrigatoriamente
    /// </summary>
    public bool ControlaValidade { get; set; } = false;

    /// <summary>
    /// Localização física no estoque
    /// </summary>
    [StringLength(100)]
    public string? LocalizacaoEstoque { get; set; }

    // Preços e valores

    /// <summary>
    /// Preço de custo unitário (última compra)
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    public decimal PrecoCusto { get; set; } = 0;

    /// <summary>
    /// Preço de venda unitário
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    public decimal PrecoVenda { get; set; } = 0;

    /// <summary>
    /// Preço de venda promocional
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    public decimal? PrecoPromocional { get; set; }

    /// <summary>
    /// Margem de lucro percentual
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal MargemLucro { get; set; } = 0;

    /// <summary>
    /// Markup aplicado sobre o custo
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal Markup { get; set; } = 0;

    // Impostos brasileiros

    /// <summary>
    /// Alíquota de ICMS (%)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal AliquotaICMS { get; set; } = 0;

    /// <summary>
    /// Alíquota de PIS (%)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal AliquotaPIS { get; set; } = 0;

    /// <summary>
    /// Alíquota de COFINS (%)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal AliquotaCOFINS { get; set; } = 0;

    /// <summary>
    /// CST (Código de Situação Tributária)
    /// </summary>
    [StringLength(5)]
    public string? CST { get; set; }

    /// <summary>
    /// CSOSN (Código de Situação da Operação no Simples Nacional)
    /// </summary>
    [StringLength(5)]
    public string? CSOSN { get; set; }

    // Dados de fornecedor

    /// <summary>
    /// Fornecedor principal do produto
    /// </summary>
    public Guid? FornecedorId { get; set; }

    /// <summary>
    /// Fornecedor principal (navegação)
    /// </summary>
    public virtual FornecedorEntity? Fornecedor { get; set; }

    /// <summary>
    /// Prazo de entrega padrão em dias
    /// </summary>
    public int PrazoEntregaDias { get; set; } = 7;

    /// <summary>
    /// Quantidade mínima para compra
    /// </summary>
    [Column(TypeName = "decimal(15,3)")]
    public decimal QuantidadeMinimaCompra { get; set; } = 1;

    // Dados de controle de qualidade

    /// <summary>
    /// Temperatura de armazenamento (em °C)
    /// </summary>
    [StringLength(50)]
    public string? TemperaturaArmazenamento { get; set; }

    /// <summary>
    /// Condições especiais de armazenamento
    /// </summary>
    [StringLength(500)]
    public string? CondicoesArmazenamento { get; set; }

    /// <summary>
    /// Se requer refrigeração
    /// </summary>
    public bool RequerRefrigeracao { get; set; } = false;

    /// <summary>
    /// Prazo de validade padrão em meses
    /// </summary>
    public int? PrazoValidadeMeses { get; set; }

    // Dados de venda e uso

    /// <summary>
    /// Indicações terapêuticas principais
    /// </summary>
    [StringLength(1000)]
    public string? Indicacoes { get; set; }

    /// <summary>
    /// Contraindicações principais
    /// </summary>
    [StringLength(1000)]
    public string? Contraindicacoes { get; set; }

    /// <summary>
    /// Posologia recomendada
    /// </summary>
    [StringLength(500)]
    public string? Posologia { get; set; }

    /// <summary>
    /// Se permite venda fracionada
    /// </summary>
    public bool PermiteVendaFracionada { get; set; } = false;

    /// <summary>
    /// Unidade de venda fracionada
    /// </summary>
    [StringLength(10)]
    public string? UnidadeVendaFracionada { get; set; }

    /// <summary>
    /// Observações gerais sobre o produto
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    // Dados de auditoria

    /// <summary>
    /// Data de cadastro do produto
    /// </summary>
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última venda
    /// </summary>
    public DateTime? DataUltimaVenda { get; set; }

    /// <summary>
    /// Data da última entrada no estoque
    /// </summary>
    public DateTime? DataUltimaEntrada { get; set; }

    /// <summary>
    /// Usuário que cadastrou o produto
    /// </summary>
    [StringLength(100)]
    public string? CadastradoPor { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Implementação ISoftDeletableEntity

    /// <summary>
    /// Se o produto foi excluído (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que excluiu o produto
    /// </summary>
    [StringLength(100)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Motivo da exclusão
    /// </summary>
    [StringLength(500)]
    public string? DeleteReason { get; set; }

    // Implementação IArchivableEntity

    /// <summary>
    /// Se os dados devem ser arquivados
    /// </summary>
    public bool ShouldArchive { get; set; } = false;

    /// <summary>
    /// Data de arquivamento automático (5 anos após inativação)
    /// </summary>
    public DateTime? ArchiveDate { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Verifica se é medicamento controlado pela ANVISA
    /// </summary>
    /// <returns>True se é controlado</returns>
    public bool IsControlado()
    {
        return MedicamentoControlado || !string.IsNullOrEmpty(ClassificacaoAnvisaCodigo);
    }

    /// <summary>
    /// Verifica se requer prescrição médica
    /// </summary>
    /// <returns>True se requer prescrição</returns>
    public bool RequerPrescricao()
    {
        // Se tem classificação ANVISA = controlado = precisa de receita
        return !string.IsNullOrEmpty(ClassificacaoAnvisaCodigo);
    }

    /// <summary>
    /// Obtém o tipo de receita necessária baseado na classificação ANVISA
    /// </summary>
    /// <returns>Tipo de receita necessária</returns>
    public string? ObterTipoReceitaNecessaria()
    {
        if (string.IsNullOrEmpty(ClassificacaoAnvisaCodigo))
            return null; // Isento

        return ClassificacaoAnvisaCodigo switch
        {
            "A1" or "A2" or "A3" => "AZUL",
            "B1" or "B2" => "BRANCA",
            "C1" => "BRANCA_2VIAS",
            "C2" or "C3" or "C4" or "C5" => "BRANCA",
            _ => null
        };
    }

    /// <summary>
    /// Verifica se produto está com estoque baixo
    /// </summary>
    /// <returns>True se estoque está abaixo do mínimo</returns>
    public bool EstoqueAbaixoMinimo()
    {
        return EstoqueAtual <= EstoqueMinimo;
    }

    /// <summary>
    /// Verifica se produto precisa de ressuprimento
    /// </summary>
    /// <returns>True se chegou no ponto de ressuprimento</returns>
    public bool PrecisaRessuprimento()
    {
        return EstoqueAtual <= PontoRessuprimento;
    }

    /// <summary>
    /// Calcula margem de lucro atual
    /// </summary>
    /// <returns>Margem de lucro percentual</returns>
    public decimal CalcularMargemLucro()
    {
        if (PrecoCusto <= 0) return 0;
        return Math.Round(((PrecoVenda - PrecoCusto) / PrecoCusto) * 100, 2);
    }

    /// <summary>
    /// Calcula markup atual
    /// </summary>
    /// <returns>Markup percentual</returns>
    public decimal CalcularMarkup()
    {
        if (PrecoCusto <= 0) return 0;
        return Math.Round(((PrecoVenda - PrecoCusto) / PrecoVenda) * 100, 2);
    }

    /// <summary>
    /// Obtém preço de venda considerando promoção
    /// </summary>
    /// <returns>Menor preço entre normal e promocional</returns>
    public decimal ObterPrecoVendaFinal()
    {
        if (PrecoPromocional.HasValue && PrecoPromocional.Value > 0 && PrecoPromocional.Value < PrecoVenda)
            return PrecoPromocional.Value;
        
        return PrecoVenda;
    }

    /// <summary>
    /// Verifica se produto está em promoção
    /// </summary>
    /// <returns>True se tem preço promocional ativo</returns>
    public bool EstaEmPromocao()
    {
        return PrecoPromocional.HasValue && 
               PrecoPromocional.Value > 0 && 
               PrecoPromocional.Value < PrecoVenda;
    }

    /// <summary>
    /// Calcula desconto percentual da promoção
    /// </summary>
    /// <returns>Percentual de desconto</returns>
    public decimal CalcularDescontoPromocao()
    {
        if (!EstaEmPromocao()) return 0;
        
        var desconto = PrecoVenda - PrecoPromocional!.Value;
        return Math.Round((desconto / PrecoVenda) * 100, 2);
    }

    /// <summary>
    /// Valida código de barras EAN-13
    /// </summary>
    /// <returns>True se código de barras é válido</returns>
    public bool ValidarCodigoBarras()
    {
        if (string.IsNullOrEmpty(CodigoBarras) || CodigoBarras.Length != 13)
            return false;

        if (!CodigoBarras.All(char.IsDigit))
            return false;

        // Algoritmo de validação EAN-13
        var soma = 0;
        for (int i = 0; i < 12; i++)
        {
            var digito = int.Parse(CodigoBarras[i].ToString());
            soma += (i % 2 == 0) ? digito : digito * 3;
        }

        var digitoVerificador = (10 - (soma % 10)) % 10;
        return digitoVerificador == int.Parse(CodigoBarras[12].ToString());
    }

    /// <summary>
    /// Atualiza estoque atual
    /// </summary>
    /// <param name="novaQuantidade">Nova quantidade em estoque</param>
    /// <param name="usuario">Usuário que está atualizando</param>
    public void AtualizarEstoque(decimal novaQuantidade, string usuario)
    {
        EstoqueAtual = novaQuantidade;
        DataAtualizacao = DateTime.UtcNow;
        AtualizadoPor = usuario;

        if (novaQuantidade > 0)
            DataUltimaEntrada = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra venda do produto
    /// </summary>
    /// <param name="usuario">Usuário que registrou a venda</param>
    public void RegistrarVenda(string usuario)
    {
        DataUltimaVenda = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
        AtualizadoPor = usuario;
    }

    /// <summary>
    /// Marca produto como excluído (soft delete)
    /// </summary>
    /// <param name="motivo">Motivo da exclusão</param>
    /// <param name="usuario">Usuário que está excluindo</param>
    public void MarcarComoExcluido(string motivo, string usuario)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = usuario;
        DeleteReason = motivo;
        Ativo = false;

        // Define data de arquivamento (5 anos após exclusão)
        ArchiveDate = DateTime.UtcNow.AddYears(5);
        ShouldArchive = true;
    }

    /// <summary>
    /// Obtém cor da classificação ANVISA para interface
    /// </summary>
    /// <returns>Código da cor (CSS)</returns>
    public string ObterCorClassificacao()
    {
        if (string.IsNullOrEmpty(ClassificacaoAnvisaCodigo))
            return "#28a745"; // Verde - isento

        return ClassificacaoAnvisaCodigo switch
        {
            "A1" or "A2" or "A3" => "#dc3545", // Vermelho (psicotrópicos)
            "B1" or "B2" => "#fd7e14", // Laranja (entorpecentes)
            "C1" or "C2" or "C3" or "C4" or "C5" => "#6f42c1", // Roxo (outras controladas)
            _ => "#6c757d" // Cinza (padrão)
        };
    }

    /// <summary>
    /// Cria novo produto com validações brasileiras
    /// </summary>
    /// <param name="nome">Nome do produto</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="categoria">Categoria do produto</param>
    /// <param name="precoVenda">Preço de venda</param>
    /// <returns>Nova instância de produto</returns>
    public static ProdutoEntity CriarNovo(
        string nome,
        string tenantId,
        string categoria,
        decimal precoVenda)
    {
        return new ProdutoEntity
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            TenantId = tenantId,
            Categoria = categoria,
            PrecoVenda = precoVenda,
            DataCadastro = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true
        };
    }
}