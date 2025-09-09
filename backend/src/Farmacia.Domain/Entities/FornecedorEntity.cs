using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um fornecedor no sistema farmacêutico brasileiro
/// Controla fornecedores de medicamentos, cosméticos e produtos farmacêuticos
/// </summary>
/// <remarks>
/// Fornecedores podem ser distribuidoras, indústrias farmacêuticas, 
/// ou outros estabelecimentos autorizados pela ANVISA para distribuição
/// </remarks>
[Table("Fornecedores")]
public class FornecedorEntity : ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único do fornecedor
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    // Dados básicos identificação

    /// <summary>
    /// Razão social do fornecedor
    /// </summary>
    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// Nome fantasia do fornecedor
    /// </summary>
    [StringLength(200)]
    public string? NomeFantasia { get; set; }

    /// <summary>
    /// CNPJ do fornecedor
    /// </summary>
    [Required]
    [StringLength(18)]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Inscrição estadual do fornecedor
    /// </summary>
    [StringLength(20)]
    public string? InscricaoEstadual { get; set; }

    /// <summary>
    /// Inscrição municipal do fornecedor (para serviços)
    /// </summary>
    [StringLength(20)]
    public string? InscricaoMunicipal { get; set; }

    // Licenças e autorizações farmacêuticas brasileiras

    /// <summary>
    /// Autorização de funcionamento ANVISA (AFE)
    /// </summary>
    [StringLength(30)]
    public string? AutorizacaoFuncionamentoAnvisa { get; set; }

    /// <summary>
    /// Data de validade da autorização ANVISA
    /// </summary>
    public DateTime? ValidadeAutorizacaoAnvisa { get; set; }

    /// <summary>
    /// Licença sanitária do estabelecimento
    /// </summary>
    [StringLength(30)]
    public string? LicencaSanitaria { get; set; }

    /// <summary>
    /// Data de validade da licença sanitária
    /// </summary>
    public DateTime? ValidadeLicencaSanitaria { get; set; }

    /// <summary>
    /// Certificado de boas práticas de distribuição (quando aplicável)
    /// </summary>
    [StringLength(30)]
    public string? CertificadoBoasPraticas { get; set; }

    /// <summary>
    /// Se fornecedor é autorizado para medicamentos controlados
    /// </summary>
    public bool AutorizadoMedicamentosControlados { get; set; } = false;

    /// <summary>
    /// Classificações de medicamentos autorizadas (JSON array)
    /// Ex: ["A1","A2","B1","B2","C1","C2","C3","C4","C5"]
    /// </summary>
    [StringLength(200)]
    public string? ClassificacoesAutorizadas { get; set; }

    // Dados de contato e endereço

    /// <summary>
    /// Endereço completo do fornecedor
    /// </summary>
    [StringLength(200)]
    public string? Endereco { get; set; }

    /// <summary>
    /// Número do endereço
    /// </summary>
    [StringLength(20)]
    public string? Numero { get; set; }

    /// <summary>
    /// Complemento do endereço
    /// </summary>
    [StringLength(100)]
    public string? Complemento { get; set; }

    /// <summary>
    /// Bairro
    /// </summary>
    [StringLength(100)]
    public string? Bairro { get; set; }

    /// <summary>
    /// CEP (formato: 00000-000)
    /// </summary>
    [StringLength(10)]
    public string? Cep { get; set; }

    /// <summary>
    /// Cidade
    /// </summary>
    [StringLength(100)]
    public string? Cidade { get; set; }

    /// <summary>
    /// Estado (UF)
    /// </summary>
    [StringLength(2)]
    public string? Estado { get; set; }

    /// <summary>
    /// Telefone principal
    /// </summary>
    [StringLength(15)]
    public string? TelefonePrincipal { get; set; }

    /// <summary>
    /// Telefone alternativo
    /// </summary>
    [StringLength(15)]
    public string? TelefoneAlternativo { get; set; }

    /// <summary>
    /// Email principal para contato comercial
    /// </summary>
    [StringLength(150)]
    public string? EmailPrincipal { get; set; }

    /// <summary>
    /// Email para pedidos
    /// </summary>
    [StringLength(150)]
    public string? EmailPedidos { get; set; }

    /// <summary>
    /// Email para cobrança e financeiro
    /// </summary>
    [StringLength(150)]
    public string? EmailFinanceiro { get; set; }

    /// <summary>
    /// Website do fornecedor
    /// </summary>
    [StringLength(200)]
    public string? Website { get; set; }

    // Dados comerciais e operacionais

    /// <summary>
    /// Tipo de fornecedor (DISTRIBUIDOR, INDUSTRIA, IMPORTADOR, MANIPULACAO, OUTROS)
    /// </summary>
    [Required]
    [StringLength(30)]
    public string TipoFornecedor { get; set; } = "DISTRIBUIDOR";

    /// <summary>
    /// Se fornecedor está ativo para novos pedidos
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Categoria principal de produtos (MEDICAMENTOS, COSMETICOS, PERFUMARIA, etc.)
    /// </summary>
    [StringLength(50)]
    public string? CategoriaPrincipal { get; set; }

    /// <summary>
    /// Categorias secundárias (JSON array)
    /// </summary>
    [StringLength(500)]
    public string? CategoriasSecundarias { get; set; }

    /// <summary>
    /// Prazo de entrega padrão em dias
    /// </summary>
    public int PrazoEntregaDias { get; set; } = 7;

    /// <summary>
    /// Valor mínimo para pedido
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    public decimal ValorMinimoPedido { get; set; } = 0;

    /// <summary>
    /// Se oferece entrega gratuita
    /// </summary>
    public bool EntregaGratuita { get; set; } = false;

    /// <summary>
    /// Valor mínimo para entrega gratuita
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    public decimal ValorMinimoEntregaGratuita { get; set; } = 0;

    /// <summary>
    /// Condição de pagamento padrão
    /// </summary>
    [StringLength(100)]
    public string? CondicaoPagamento { get; set; }

    /// <summary>
    /// Se aceita cartão de crédito
    /// </summary>
    public bool AceitaCartaoCredito { get; set; } = false;

    /// <summary>
    /// Se aceita PIX
    /// </summary>
    public bool AceitaPix { get; set; } = true;

    /// <summary>
    /// Se aceita boleto bancário
    /// </summary>
    public bool AceitaBoleto { get; set; } = true;

    // Dados bancários e financeiros

    /// <summary>
    /// Banco principal
    /// </summary>
    [StringLength(100)]
    public string? BancoPrincipal { get; set; }

    /// <summary>
    /// Agência bancária
    /// </summary>
    [StringLength(10)]
    public string? AgenciaBancaria { get; set; }

    /// <summary>
    /// Conta bancária
    /// </summary>
    [StringLength(20)]
    public string? ContaBancaria { get; set; }

    /// <summary>
    /// Chave PIX
    /// </summary>
    [StringLength(100)]
    public string? ChavePix { get; set; }

    // Avaliação e relacionamento

    /// <summary>
    /// Avaliação média do fornecedor (0-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal AvaliacaoMedia { get; set; } = 0;

    /// <summary>
    /// Total de avaliações recebidas
    /// </summary>
    public int TotalAvaliacoes { get; set; } = 0;

    /// <summary>
    /// Prioridade do fornecedor (1-10, sendo 10 a maior)
    /// </summary>
    public int Prioridade { get; set; } = 5;

    /// <summary>
    /// Se é fornecedor preferencial
    /// </summary>
    public bool FornecedorPreferencial { get; set; } = false;

    /// <summary>
    /// Observações gerais sobre o fornecedor
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    // Controle de vendedor/representante

    /// <summary>
    /// Nome do vendedor/representante
    /// </summary>
    [StringLength(100)]
    public string? NomeVendedor { get; set; }

    /// <summary>
    /// Telefone do vendedor
    /// </summary>
    [StringLength(15)]
    public string? TelefoneVendedor { get; set; }

    /// <summary>
    /// Email do vendedor
    /// </summary>
    [StringLength(150)]
    public string? EmailVendedor { get; set; }

    /// <summary>
    /// Região de atuação do vendedor
    /// </summary>
    [StringLength(100)]
    public string? RegiaoVendedor { get; set; }

    // Timestamps

    /// <summary>
    /// Data de cadastro do fornecedor
    /// </summary>
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data do último pedido realizado
    /// </summary>
    public DateTime? DataUltimoPedido { get; set; }

    /// <summary>
    /// Usuário que cadastrou o fornecedor
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
    /// Se o fornecedor foi excluído (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que excluiu o fornecedor
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

    // Navegação

    /// <summary>
    /// Produtos fornecidos por este fornecedor
    /// </summary>
    public virtual ICollection<ProdutoEntity> Produtos { get; set; } = new List<ProdutoEntity>();

    /// <summary>
    /// Pedidos de compra realizados com este fornecedor
    /// </summary>
    public virtual ICollection<PedidoCompraEntity> PedidosCompra { get; set; } = new List<PedidoCompraEntity>();

    /// <summary>
    /// Avaliações recebidas por este fornecedor
    /// </summary>
    public virtual ICollection<FornecedorAvaliacaoEntity> Avaliacoes { get; set; } = new List<FornecedorAvaliacaoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se CNPJ é válido usando algoritmo brasileiro
    /// </summary>
    /// <returns>True se CNPJ é válido</returns>
    public bool ValidarCnpj()
    {
        if (string.IsNullOrEmpty(Cnpj))
            return false;

        // Remove formatação
        var cnpjLimpo = Cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

        if (cnpjLimpo.Length != 14)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cnpjLimpo.All(c => c == cnpjLimpo[0]))
            return false;

        // Validação do algoritmo de CNPJ brasileiro
        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string cnpjAux = cnpjLimpo.Substring(0, 12);
        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(cnpjAux[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        string digito = resto.ToString();
        cnpjAux += digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(cnpjAux[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cnpjLimpo.EndsWith(digito);
    }

    /// <summary>
    /// Verifica se fornecedor pode fornecer medicamentos controlados
    /// </summary>
    /// <param name="classificacaoAnvisa">Classificação ANVISA do medicamento</param>
    /// <returns>True se pode fornecer</returns>
    public bool PodeFornecerMedicamentoControlado(string classificacaoAnvisa)
    {
        if (!AutorizadoMedicamentosControlados)
            return false;

        if (string.IsNullOrEmpty(ClassificacoesAutorizadas))
            return false;

        // Verifica se tem autorização para a classificação específica
        var classificacoes = ClassificacoesAutorizadas.Split(',')
            .Select(c => c.Trim().Trim('"', '\''))
            .ToList();

        return classificacoes.Contains(classificacaoAnvisa.Trim());
    }

    /// <summary>
    /// Verifica se as licenças estão válidas
    /// </summary>
    /// <returns>True se licenças estão válidas</returns>
    public bool TemLicencasValidas()
    {
        var hoje = DateTime.UtcNow.Date;

        // ANVISA sempre obrigatória
        if (ValidadeAutorizacaoAnvisa == null || ValidadeAutorizacaoAnvisa.Value.Date < hoje)
            return false;

        // Licença sanitária obrigatória
        if (ValidadeLicencaSanitaria == null || ValidadeLicencaSanitaria.Value.Date < hoje)
            return false;

        return true;
    }

    /// <summary>
    /// Calcula quantos dias faltam para vencer as licenças
    /// </summary>
    /// <returns>Menor número de dias até vencer alguma licença</returns>
    public int DiasParaVencerLicencas()
    {
        var hoje = DateTime.UtcNow.Date;
        var diasVencimento = new List<int>();

        if (ValidadeAutorizacaoAnvisa != null)
            diasVencimento.Add((ValidadeAutorizacaoAnvisa.Value.Date - hoje).Days);

        if (ValidadeLicencaSanitaria != null)
            diasVencimento.Add((ValidadeLicencaSanitaria.Value.Date - hoje).Days);

        return diasVencimento.Any() ? diasVencimento.Min() : 0;
    }

    /// <summary>
    /// Verifica se pedido atende ao valor mínimo
    /// </summary>
    /// <param name="valorPedido">Valor total do pedido</param>
    /// <returns>True se atende ao valor mínimo</returns>
    public bool AtendaValorMinimoPedido(decimal valorPedido)
    {
        return valorPedido >= ValorMinimoPedido;
    }

    /// <summary>
    /// Verifica se pedido tem direito à entrega gratuita
    /// </summary>
    /// <param name="valorPedido">Valor total do pedido</param>
    /// <returns>True se tem direito à entrega gratuita</returns>
    public bool TemDireitoEntregaGratuita(decimal valorPedido)
    {
        return EntregaGratuita && valorPedido >= ValorMinimoEntregaGratuita;
    }

    /// <summary>
    /// Adiciona avaliação ao fornecedor
    /// </summary>
    /// <param name="nota">Nota de 1 a 5</param>
    public void AdicionarAvaliacao(decimal nota)
    {
        if (nota < 1 || nota > 5)
            throw new ArgumentException("Nota deve estar entre 1 e 5");

        var totalPontos = AvaliacaoMedia * TotalAvaliacoes;
        TotalAvaliacoes++;
        AvaliacaoMedia = Math.Round((totalPontos + nota) / TotalAvaliacoes, 2);
    }

    /// <summary>
    /// Atualiza data do último pedido
    /// </summary>
    public void RegistrarUltimoPedido()
    {
        DataUltimoPedido = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca fornecedor como excluído (soft delete)
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
    /// Formata CNPJ para exibição
    /// </summary>
    /// <returns>CNPJ formatado</returns>
    public string CnpjFormatado()
    {
        if (string.IsNullOrEmpty(Cnpj) || Cnpj.Length != 14)
            return Cnpj ?? string.Empty;

        return $"{Cnpj.Substring(0, 2)}.{Cnpj.Substring(2, 3)}.{Cnpj.Substring(5, 3)}/{Cnpj.Substring(8, 4)}-{Cnpj.Substring(12, 2)}";
    }

    /// <summary>
    /// Obtém endereço completo formatado
    /// </summary>
    /// <returns>Endereço formatado para exibição</returns>
    public string EnderecoCompleto()
    {
        var partes = new List<string>();

        if (!string.IsNullOrEmpty(Endereco))
        {
            var enderecoComNumero = Endereco;
            if (!string.IsNullOrEmpty(Numero))
                enderecoComNumero += $", {Numero}";
            if (!string.IsNullOrEmpty(Complemento))
                enderecoComNumero += $" - {Complemento}";
            partes.Add(enderecoComNumero);
        }

        if (!string.IsNullOrEmpty(Bairro))
            partes.Add(Bairro);

        if (!string.IsNullOrEmpty(Cidade) && !string.IsNullOrEmpty(Estado))
            partes.Add($"{Cidade}/{Estado}");

        if (!string.IsNullOrEmpty(Cep))
            partes.Add($"CEP {Cep}");

        return string.Join(" - ", partes);
    }

    /// <summary>
    /// Cria novo fornecedor com validações brasileiras
    /// </summary>
    /// <param name="razaoSocial">Razão social</param>
    /// <param name="cnpj">CNPJ do fornecedor</param>
    /// <param name="tenantId">ID do tenant (farmácia)</param>
    /// <param name="tipoFornecedor">Tipo do fornecedor</param>
    /// <returns>Nova instância de fornecedor</returns>
    public static FornecedorEntity CriarNovo(
        string razaoSocial,
        string cnpj,
        string tenantId,
        string tipoFornecedor = "DISTRIBUIDOR")
    {
        var fornecedor = new FornecedorEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RazaoSocial = razaoSocial,
            Cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", ""), // Armazena sem formatação
            TipoFornecedor = tipoFornecedor,
            DataCadastro = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true
        };

        if (!fornecedor.ValidarCnpj())
            throw new ArgumentException($"CNPJ '{cnpj}' não é válido");

        return fornecedor;
    }
}

/// <summary>
/// Entidade que representa uma avaliação de fornecedor
/// </summary>
/// <remarks>
/// Permite que farmácias avaliem seus fornecedores baseado em 
/// critérios como prazo de entrega, qualidade dos produtos, atendimento, etc.
/// </remarks>
[Table("FornecedorAvaliacoes")]
public class FornecedorAvaliacaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único da avaliação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do fornecedor avaliado
    /// </summary>
    [Required]
    public Guid FornecedorId { get; set; }

    /// <summary>
    /// Identificador do usuário que fez a avaliação
    /// </summary>
    [Required]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Nota geral (1-5)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(3,2)")]
    public decimal NotaGeral { get; set; }

    /// <summary>
    /// Nota para prazo de entrega (1-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal NotaPrazoEntrega { get; set; } = 0;

    /// <summary>
    /// Nota para qualidade dos produtos (1-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal NotaQualidadeProdutos { get; set; } = 0;

    /// <summary>
    /// Nota para atendimento (1-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal NotaAtendimento { get; set; } = 0;

    /// <summary>
    /// Nota para preços praticados (1-5)
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal NotaPrecos { get; set; } = 0;

    /// <summary>
    /// Comentários sobre o fornecedor
    /// </summary>
    [StringLength(1000)]
    public string? Comentarios { get; set; }

    /// <summary>
    /// Data da avaliação
    /// </summary>
    public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Se recomenda o fornecedor
    /// </summary>
    public bool Recomenda { get; set; } = true;

    // Navegação

    /// <summary>
    /// Fornecedor sendo avaliado
    /// </summary>
    public virtual FornecedorEntity? Fornecedor { get; set; }

    /// <summary>
    /// Usuário que fez a avaliação
    /// </summary>
    public virtual UsuarioEntity? Usuario { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Calcula média ponderada das notas específicas
    /// </summary>
    /// <returns>Média ponderada das avaliações</returns>
    public decimal CalcularMediaPonderada()
    {
        var notas = new[] { NotaPrazoEntrega, NotaQualidadeProdutos, NotaAtendimento, NotaPrecos }
            .Where(n => n > 0)
            .ToList();

        return notas.Any() ? Math.Round(notas.Average(), 2) : NotaGeral;
    }

    /// <summary>
    /// Cria nova avaliação para fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="usuarioId">ID do usuário avaliador</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="notaGeral">Nota geral (1-5)</param>
    /// <returns>Nova instância de avaliação</returns>
    public static FornecedorAvaliacaoEntity CriarNova(
        Guid fornecedorId,
        Guid usuarioId,
        string tenantId,
        decimal notaGeral)
    {
        if (notaGeral < 1 || notaGeral > 5)
            throw new ArgumentException("Nota deve estar entre 1 e 5");

        return new FornecedorAvaliacaoEntity
        {
            Id = Guid.NewGuid(),
            FornecedorId = fornecedorId,
            UsuarioId = usuarioId,
            TenantId = tenantId,
            NotaGeral = notaGeral,
            DataAvaliacao = DateTime.UtcNow
        };
    }
}