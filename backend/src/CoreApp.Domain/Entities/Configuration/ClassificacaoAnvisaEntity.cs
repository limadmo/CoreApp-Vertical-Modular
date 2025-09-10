using System.ComponentModel.DataAnnotations;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para classificações ANVISA de medicamentos controlados
/// Contém apenas as listas que EXIGEM controle especial (A1-A3, B1-B2, C1-C5)
/// </summary>
/// <remarks>
/// Esta entidade cadastra APENAS as classificações que aparecem nas listas oficiais da ANVISA.
/// Produtos que não estão em nenhuma lista são considerados "isentos" por padrão.
/// 
/// A ANVISA define diferentes tipos de receita baseados na classificação:
/// - Lista A (A1, A2, A3): Receita azul (psicotrópicos/entorpecentes)
/// - Lista B (B1, B2): Receita branca (psicotrópicos/anorexígenos) 
/// - Lista C (C1-C5): Receita branca ou branca 2 vias (outras substâncias controladas)
/// 
/// Sistema hierárquico permite:
/// - Configurações globais do sistema (ANVISA oficial)
/// - Customizações específicas por farmácia (quando permitido)
/// - Alterações sem necessidade de deploy
/// </remarks>
public class ClassificacaoAnvisaEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da classificação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global ANVISA)
    /// </summary>
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código da lista ANVISA (A1, A2, A3, B1, B2, C1, C2, C3, C4, C5)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo da classificação
    /// </summary>
    /// <example>Lista A1 - Entorpecentes, Lista B1 - Psicotrópicos</example>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da classificação e suas implicações
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Tipo de receita exigida para medicamentos desta classificação
    /// </summary>
    /// <example>AZUL, BRANCA, BRANCA_2VIAS</example>
    [Required]
    [StringLength(30)]
    public string TipoReceita { get; set; } = string.Empty;

    /// <summary>
    /// Cor da receita em formato hexadecimal
    /// </summary>
    /// <example>#0000FF (azul), #FFFFFF (branco)</example>
    [StringLength(7)]
    public string? CorReceita { get; set; }

    /// <summary>
    /// Se a receita deve ser retida pela farmácia
    /// </summary>
    public bool RequerRetencaoReceita { get; set; } = true;

    /// <summary>
    /// Prazo de validade da receita em dias
    /// </summary>
    public int DiasValidadeReceita { get; set; } = 30;

    /// <summary>
    /// Quantidade máxima que pode ser dispensada por receita
    /// </summary>
    /// <remarks>
    /// null = sem limite específico, segue regras gerais da classificação
    /// </remarks>
    public int? QuantidadeMaximaPorReceita { get; set; }

    /// <summary>
    /// Se requer autorização especial ou notificação para dispensação
    /// </summary>
    public bool RequerAutorizacaoEspecial { get; set; } = true;

    /// <summary>
    /// Se deve ser reportado no Sistema Nacional de Gerenciamento de Produtos Controlados (SNGPC)
    /// </summary>
    public bool ReportarSNGPC { get; set; } = true;

    /// <summary>
    /// Categoria da classificação para agrupamento
    /// </summary>
    /// <example>ENTORPECENTE, PSICOTROPICO, IMUNOSSUPRESSOR</example>
    [StringLength(50)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-prescription-bottle, fa-shield-alt</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Cor de destaque para exibição na interface
    /// </summary>
    /// <example>#dc3545 (vermelho para controlados), #fd7e14 (laranja)</example>
    [StringLength(7)]
    public string? CorDestaque { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Nível de permissão mínimo para dispensar medicamentos desta classificação
    /// </summary>
    /// <example>FARMACEUTICO, FARMACEUTICO_RESPONSAVEL</example>
    [StringLength(50)]
    public string? NivelPermissaoMinimo { get; set; }

    /// <summary>
    /// Regras específicas de validação em formato JSON
    /// </summary>
    /// <example>{"requer_rg_comprador": true, "idade_minima": 18}</example>
    [StringLength(1000)]
    public string? RegrasValidacao { get; set; }

    /// <summary>
    /// Observações específicas para a classificação
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Status ativo/inativo (permite desativar sem perder histórico)
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se é uma configuração oficial da ANVISA (não pode ser removida)
    /// </summary>
    public bool IsOficialAnvisa { get; set; } = true;

    /// <summary>
    /// ID da configuração ANVISA oficial pai (para customizações por tenant)
    /// </summary>
    public Guid? ConfiguracaoAnvisaOficialId { get; set; }

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário responsável pela criação
    /// </summary>
    [StringLength(100)]
    public string? CriadoPor { get; set; }

    /// <summary>
    /// Usuário responsável pela última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Relacionamentos de navegação

    /// <summary>
    /// Configuração ANVISA oficial pai (para customizações)
    /// </summary>
    public virtual ClassificacaoAnvisaEntity? ConfiguracaoAnvisaOficial { get; set; }

    /// <summary>
    /// Configurações personalizadas que herdam desta
    /// </summary>
    public virtual ICollection<ClassificacaoAnvisaEntity> ConfiguracoesPersonalizadas { get; set; } = new List<ClassificacaoAnvisaEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se é uma configuração global da ANVISA
    /// </summary>
    /// <returns>True se for global</returns>
    public bool IsGlobal()
    {
        return string.IsNullOrEmpty(TenantId);
    }

    /// <summary>
    /// Verifica se pode ser removido (não é oficial ANVISA e não tem produtos associados)
    /// </summary>
    /// <returns>True se pode ser removido</returns>
    public bool PodeSerRemovido()
    {
        return !IsOficialAnvisa;
    }

    /// <summary>
    /// Obtém o tipo de receita com formatação para exibição
    /// </summary>
    /// <returns>Tipo de receita formatado</returns>
    public string ObterTipoReceitaFormatado()
    {
        return TipoReceita switch
        {
            "AZUL" => "Receita Especial Azul",
            "BRANCA" => "Receita Especial Branca",
            "BRANCA_2VIAS" => "Receita Especial Branca (2 vias)",
            _ => TipoReceita
        };
    }

    /// <summary>
    /// Verifica se a classificação é da Lista A (entorpecentes/psicotrópicos)
    /// </summary>
    /// <returns>True se é Lista A</returns>
    public bool IsListaA()
    {
        return Codigo.StartsWith("A");
    }

    /// <summary>
    /// Verifica se a classificação é da Lista B (psicotrópicos/anorexígenos)
    /// </summary>
    /// <returns>True se é Lista B</returns>
    public bool IsListaB()
    {
        return Codigo.StartsWith("B");
    }

    /// <summary>
    /// Verifica se a classificação é da Lista C (outras substâncias controladas)
    /// </summary>
    /// <returns>True se é Lista C</returns>
    public bool IsListaC()
    {
        return Codigo.StartsWith("C");
    }

    /// <summary>
    /// Obtém a cor da receita baseada no tipo
    /// </summary>
    /// <returns>Código da cor hexadecimal</returns>
    public string ObterCorReceitaPadrao()
    {
        if (!string.IsNullOrEmpty(CorReceita))
            return CorReceita;

        return TipoReceita switch
        {
            "AZUL" => "#0066CC",
            "BRANCA" or "BRANCA_2VIAS" => "#FFFFFF",
            _ => "#CCCCCC"
        };
    }

    /// <summary>
    /// Obtém a cor de destaque baseada na classificação
    /// </summary>
    /// <returns>Código da cor hexadecimal</returns>
    public string ObterCorDestaquePadrao()
    {
        if (!string.IsNullOrEmpty(CorDestaque))
            return CorDestaque;

        return Codigo[0] switch
        {
            'A' => "#DC3545", // Vermelho (mais restritivo)
            'B' => "#FD7E14", // Laranja (médio)
            'C' => "#6F42C1", // Roxo (controlado especial)
            _ => "#6C757D"     // Cinza (padrão)
        };
    }

    /// <summary>
    /// Valida se uma quantidade está dentro do limite permitido
    /// </summary>
    /// <param name="quantidade">Quantidade a ser validada</param>
    /// <returns>True se quantidade é válida</returns>
    public bool ValidarQuantidade(decimal quantidade)
    {
        if (!QuantidadeMaximaPorReceita.HasValue)
            return true;

        return quantidade <= QuantidadeMaximaPorReceita.Value;
    }

    /// <summary>
    /// Verifica se uma receita ainda está válida baseada na data de emissão
    /// </summary>
    /// <param name="dataEmissao">Data de emissão da receita</param>
    /// <returns>True se receita ainda está válida</returns>
    public bool ReceitaValida(DateTime dataEmissao)
    {
        var dataLimite = dataEmissao.AddDays(DiasValidadeReceita);
        return DateTime.Now <= dataLimite;
    }

    /// <summary>
    /// Atualiza o timestamp de modificação
    /// </summary>
    public void AtualizarTimestamp()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma personalização desta classificação para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant de destino</param>
    /// <returns>Nova configuração personalizada</returns>
    public ClassificacaoAnvisaEntity CriarPersonalizacao(string tenantId)
    {
        return new ClassificacaoAnvisaEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            TipoReceita = TipoReceita,
            CorReceita = CorReceita,
            RequerRetencaoReceita = RequerRetencaoReceita,
            DiasValidadeReceita = DiasValidadeReceita,
            QuantidadeMaximaPorReceita = QuantidadeMaximaPorReceita,
            RequerAutorizacaoEspecial = RequerAutorizacaoEspecial,
            ReportarSNGPC = ReportarSNGPC,
            Categoria = Categoria,
            Icone = Icone,
            CorDestaque = CorDestaque,
            Ordem = Ordem,
            NivelPermissaoMinimo = NivelPermissaoMinimo,
            RegrasValidacao = RegrasValidacao,
            Observacoes = Observacoes,
            Ativo = Ativo,
            IsOficialAnvisa = false, // Personalizações nunca são oficiais
            ConfiguracaoAnvisaOficialId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_PERSONALIZACAO"
        };
    }

    /// <summary>
    /// Cria nova classificação ANVISA controlada
    /// </summary>
    /// <param name="codigo">Código da lista (A1, B1, C1, etc.)</param>
    /// <param name="nome">Nome descritivo</param>
    /// <param name="tipoReceita">Tipo de receita necessária</param>
    /// <returns>Nova instância de classificação</returns>
    public static ClassificacaoAnvisaEntity CriarClassificacaoOficial(
        string codigo,
        string nome,
        string tipoReceita)
    {
        return new ClassificacaoAnvisaEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            TipoReceita = tipoReceita,
            IsOficialAnvisa = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true,
            CriadoPor = "SISTEMA_ANVISA"
        };
    }
}