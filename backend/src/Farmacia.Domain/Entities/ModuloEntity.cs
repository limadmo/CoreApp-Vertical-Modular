using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um módulo funcional do sistema farmacêutico brasileiro
/// Define funcionalidades específicas disponíveis nos diferentes planos comerciais
/// </summary>
/// <remarks>
/// Módulos são funcionalidades específicas do sistema que podem ser ativadas/desativadas
/// conforme o plano comercial contratado pela farmácia (Starter, Professional, Enterprise)
/// </remarks>
[Table("Modulos")]
public class ModuloEntity
{
    /// <summary>
    /// Identificador único do módulo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código único do módulo (ex: PRODUCTS, SALES, STOCK, CUSTOMERS)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome amigável do módulo
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do módulo e suas funcionalidades
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Categoria do módulo (CORE, COMERCIAL, RELATORIOS, INTEGRACAO)
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Ícone para exibição na interface (nome do ícone)
    /// </summary>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Cor tema do módulo (hex)
    /// </summary>
    [StringLength(7)]
    public string? CorTema { get; set; }

    /// <summary>
    /// Se o módulo está ativo no sistema
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Se é um módulo essencial (não pode ser desabilitado)
    /// </summary>
    public bool ModuloEssencial { get; set; } = false;

    /// <summary>
    /// Ordem de exibição nos menus
    /// </summary>
    public int OrdemExibicao { get; set; } = 0;

    /// <summary>
    /// Módulos que são pré-requisitos para este
    /// </summary>
    [StringLength(200)]
    public string? ModulosPreRequisitos { get; set; }

    /// <summary>
    /// Versão mínima da API necessária
    /// </summary>
    [StringLength(10)]
    public string? VersaoMinimaApi { get; set; }

    /// <summary>
    /// Data de criação do módulo
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    // Navegação

    /// <summary>
    /// Planos que incluem este módulo
    /// </summary>
    public virtual ICollection<PlanoModuloEntity> Planos { get; set; } = new List<PlanoModuloEntity>();

    /// <summary>
    /// Tenants que têm este módulo ativo
    /// </summary>
    public virtual ICollection<TenantModuloEntity> Tenants { get; set; } = new List<TenantModuloEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se módulo tem todos os pré-requisitos satisfeitos
    /// </summary>
    /// <param name="modulosAtivos">Lista de códigos dos módulos ativos</param>
    /// <returns>True se todos os pré-requisitos estão ativos</returns>
    public bool TemPreRequisitosSatisfeitos(IEnumerable<string> modulosAtivos)
    {
        if (string.IsNullOrEmpty(ModulosPreRequisitos))
            return true;

        var preRequisitos = ModulosPreRequisitos.Split(',')
            .Select(m => m.Trim())
            .Where(m => !string.IsNullOrEmpty(m))
            .ToList();

        return preRequisitos.All(prereq => modulosAtivos.Contains(prereq));
    }

    /// <summary>
    /// Cria novo módulo do sistema
    /// </summary>
    /// <param name="codigo">Código único do módulo</param>
    /// <param name="nome">Nome amigável</param>
    /// <param name="categoria">Categoria do módulo</param>
    /// <param name="essencial">Se é módulo essencial</param>
    /// <returns>Nova instância de módulo</returns>
    public static ModuloEntity CriarNovo(
        string codigo,
        string nome,
        string categoria,
        bool essencial = false)
    {
        return new ModuloEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.ToUpper(),
            Nome = nome,
            Categoria = categoria.ToUpper(),
            ModuloEssencial = essencial,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Entidade que representa um plano comercial brasileiro para farmácias
/// Define pacotes de funcionalidades com preços específicos para o mercado nacional
/// </summary>
/// <remarks>
/// Planos definem quais módulos estão incluídos e os preços em reais brasileiros.
/// Starter (R$149,90), Professional (R$249,90), Enterprise (R$399,90)
/// </remarks>
[Table("PlanosComerciais")]
public class PlanoComercialEntity
{
    /// <summary>
    /// Identificador único do plano
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código único do plano (STARTER, PROFESSIONAL, ENTERPRISE)
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome comercial do plano
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição comercial do plano
    /// </summary>
    [StringLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Preço mensal em reais brasileiros
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoMensalBRL { get; set; }

    /// <summary>
    /// Preço anual em reais brasileiros (com desconto)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoAnualBRL { get; set; }

    /// <summary>
    /// Percentual de desconto anual
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal PercentualDescontoAnual { get; set; } = 0;

    /// <summary>
    /// Número máximo de usuários incluídos no plano
    /// </summary>
    public int MaximoUsuarios { get; set; } = 1;

    /// <summary>
    /// Número máximo de produtos cadastráveis
    /// </summary>
    public int MaximoProdutos { get; set; } = 10000;

    /// <summary>
    /// Espaço de armazenamento em GB
    /// </summary>
    public int EspacoArmazenamentoGB { get; set; } = 5;

    /// <summary>
    /// Número máximo de clientes cadastráveis
    /// </summary>
    public int MaximoClientes { get; set; } = 1000;

    /// <summary>
    /// Se permite múltiplas filiais
    /// </summary>
    public bool PermiteMultiplasFiliais { get; set; } = false;

    /// <summary>
    /// Número máximo de filiais
    /// </summary>
    public int MaximoFiliais { get; set; } = 1;

    /// <summary>
    /// Se inclui suporte técnico básico
    /// </summary>
    public bool SuporteTecnicoBasico { get; set; } = true;

    /// <summary>
    /// Se inclui suporte técnico premium
    /// </summary>
    public bool SuporteTecnicoPremium { get; set; } = false;

    /// <summary>
    /// Se inclui treinamento
    /// </summary>
    public bool IncluiTreinamento { get; set; } = false;

    /// <summary>
    /// Se permite customização de relatórios
    /// </summary>
    public bool CustomizacaoRelatorios { get; set; } = false;

    /// <summary>
    /// Se inclui backup automático
    /// </summary>
    public bool BackupAutomatico { get; set; } = true;

    /// <summary>
    /// Período de retenção de backup em dias
    /// </summary>
    public int PeriodoRetencaoBackupDias { get; set; } = 30;

    /// <summary>
    /// Se permite API externa
    /// </summary>
    public bool PermiteApiExterna { get; set; } = false;

    /// <summary>
    /// Limite de requisições API por minuto
    /// </summary>
    public int LimiteApiPorMinuto { get; set; } = 60;

    /// <summary>
    /// Se está disponível para contratação
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Se é plano promocional
    /// </summary>
    public bool PlanoPromocional { get; set; } = false;

    /// <summary>
    /// Data de validade do plano promocional
    /// </summary>
    public DateTime? ValidadePromocional { get; set; }

    /// <summary>
    /// Posição na lista de planos (para ordenação)
    /// </summary>
    public int OrdemExibicao { get; set; } = 0;

    /// <summary>
    /// Recursos destacados do plano (JSON array)
    /// </summary>
    [StringLength(1000)]
    public string? RecursosDestacados { get; set; }

    /// <summary>
    /// Limitações do plano (JSON array)
    /// </summary>
    [StringLength(1000)]
    public string? Limitacoes { get; set; }

    /// <summary>
    /// Data de criação do plano
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    // Navegação

    /// <summary>
    /// Módulos incluídos neste plano
    /// </summary>
    public virtual ICollection<PlanoModuloEntity> Modulos { get; set; } = new List<PlanoModuloEntity>();

    /// <summary>
    /// Tenants que contrataram este plano
    /// </summary>
    public virtual ICollection<TenantPlanoEntity> Tenants { get; set; } = new List<TenantPlanoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Calcula o valor do desconto anual em reais
    /// </summary>
    /// <returns>Valor do desconto anual</returns>
    public decimal CalcularDescontoAnual()
    {
        var valorMensal12x = PrecoMensalBRL * 12;
        return valorMensal12x - PrecoAnualBRL;
    }

    /// <summary>
    /// Calcula percentual real de desconto anual
    /// </summary>
    /// <returns>Percentual de desconto</returns>
    public decimal CalcularPercentualDescontoReal()
    {
        var valorMensal12x = PrecoMensalBRL * 12;
        if (valorMensal12x == 0) return 0;

        return Math.Round(((valorMensal12x - PrecoAnualBRL) / valorMensal12x) * 100, 2);
    }

    /// <summary>
    /// Verifica se plano está válido para contratação
    /// </summary>
    /// <returns>True se pode ser contratado</returns>
    public bool PodeSerContratado()
    {
        if (!Ativo) return false;

        if (PlanoPromocional && ValidadePromocional != null)
            return DateTime.UtcNow <= ValidadePromocional.Value;

        return true;
    }

    /// <summary>
    /// Obtém lista de códigos dos módulos incluídos
    /// </summary>
    /// <returns>Lista de códigos de módulos</returns>
    public List<string> ObterCodigosModulos()
    {
        return Modulos
            .Where(pm => pm.Ativo)
            .Select(pm => pm.Modulo?.Codigo ?? string.Empty)
            .Where(codigo => !string.IsNullOrEmpty(codigo))
            .ToList();
    }

    /// <summary>
    /// Verifica se plano inclui módulo específico
    /// </summary>
    /// <param name="codigoModulo">Código do módulo</param>
    /// <returns>True se módulo está incluído</returns>
    public bool IncluiModulo(string codigoModulo)
    {
        return Modulos.Any(pm => pm.Ativo && pm.Modulo?.Codigo == codigoModulo);
    }

    /// <summary>
    /// Cria plano comercial brasileiro
    /// </summary>
    /// <param name="codigo">Código do plano</param>
    /// <param name="nome">Nome comercial</param>
    /// <param name="precoMensal">Preço mensal em BRL</param>
    /// <param name="precoAnual">Preço anual em BRL</param>
    /// <returns>Nova instância de plano</returns>
    public static PlanoComercialEntity CriarNovo(
        string codigo,
        string nome,
        decimal precoMensal,
        decimal precoAnual)
    {
        var plano = new PlanoComercialEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.ToUpper(),
            Nome = nome,
            PrecoMensalBRL = precoMensal,
            PrecoAnualBRL = precoAnual,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        plano.PercentualDescontoAnual = plano.CalcularPercentualDescontoReal();

        return plano;
    }
}

/// <summary>
/// Entidade de associação entre plano comercial e módulos incluídos
/// </summary>
/// <remarks>
/// Define quais módulos estão incluídos em cada plano comercial
/// </remarks>
[Table("PlanoModulos")]
public class PlanoModuloEntity
{
    /// <summary>
    /// Identificador único da associação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do plano comercial
    /// </summary>
    [Required]
    public Guid PlanoId { get; set; }

    /// <summary>
    /// Identificador do módulo
    /// </summary>
    [Required]
    public Guid ModuloId { get; set; }

    /// <summary>
    /// Se o módulo está ativo neste plano
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data de inclusão do módulo no plano
    /// </summary>
    public DateTime DataInclusao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Se é módulo opcional (pode ser removido pelo admin)
    /// </summary>
    public bool Opcional { get; set; } = false;

    /// <summary>
    /// Observações sobre a inclusão do módulo
    /// </summary>
    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Navegação

    /// <summary>
    /// Plano comercial que inclui este módulo
    /// </summary>
    public virtual PlanoComercialEntity? Plano { get; set; }

    /// <summary>
    /// Módulo incluído no plano
    /// </summary>
    public virtual ModuloEntity? Modulo { get; set; }

    /// <summary>
    /// Cria associação plano-módulo
    /// </summary>
    /// <param name="planoId">ID do plano</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <param name="opcional">Se é módulo opcional</param>
    /// <returns>Nova instância da associação</returns>
    public static PlanoModuloEntity CriarNova(
        Guid planoId,
        Guid moduloId,
        bool opcional = false)
    {
        return new PlanoModuloEntity
        {
            Id = Guid.NewGuid(),
            PlanoId = planoId,
            ModuloId = moduloId,
            Opcional = opcional,
            DataInclusao = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Entidade que representa a contratação de um plano por um tenant (farmácia)
/// </summary>
/// <remarks>
/// Controla qual plano cada farmácia contratou, período de vigência, 
/// status de pagamento e histórico de mudanças de plano
/// </remarks>
[Table("TenantPlanos")]
public class TenantPlanoEntity
{
    /// <summary>
    /// Identificador único da contratação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do plano contratado
    /// </summary>
    [Required]
    public Guid PlanoId { get; set; }

    /// <summary>
    /// Data de início da vigência
    /// </summary>
    [Required]
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de fim da vigência
    /// </summary>
    public DateTime? DataFim { get; set; }

    /// <summary>
    /// Status da contratação (ATIVA, CANCELADA, SUSPENSA, VENCIDA)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ATIVA";

    /// <summary>
    /// Tipo de pagamento (MENSAL, ANUAL)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string TipoPagamento { get; set; } = "MENSAL";

    /// <summary>
    /// Valor contratado (pode ser diferente do valor atual do plano)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorContratado { get; set; }

    /// <summary>
    /// Data da próxima cobrança
    /// </summary>
    public DateTime? DataProximaCobranca { get; set; }

    /// <summary>
    /// Status do pagamento (EM_DIA, PENDENTE, ATRASADO)
    /// </summary>
    [StringLength(20)]
    public string StatusPagamento { get; set; } = "EM_DIA";

    /// <summary>
    /// Dias em atraso no pagamento
    /// </summary>
    public int DiasAtraso { get; set; } = 0;

    /// <summary>
    /// Se é período de teste gratuito
    /// </summary>
    public bool PeriodoTeste { get; set; } = false;

    /// <summary>
    /// Dias restantes do período de teste
    /// </summary>
    public int DiasTesteRestantes { get; set; } = 0;

    /// <summary>
    /// Se renovação automática está ativa
    /// </summary>
    public bool RenovacaoAutomatica { get; set; } = true;

    /// <summary>
    /// Motivo do cancelamento (se aplicável)
    /// </summary>
    [StringLength(500)]
    public string? MotivoCancelamento { get; set; }

    /// <summary>
    /// Data do cancelamento
    /// </summary>
    public DateTime? DataCancelamento { get; set; }

    /// <summary>
    /// Usuário que efetuou o cancelamento
    /// </summary>
    [StringLength(100)]
    public string? CanceladoPor { get; set; }

    /// <summary>
    /// Observações sobre a contratação
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    // Navegação

    /// <summary>
    /// Plano contratado
    /// </summary>
    public virtual PlanoComercialEntity? Plano { get; set; }

    /// <summary>
    /// Módulos específicos ativos para este tenant
    /// </summary>
    public virtual ICollection<TenantModuloEntity> ModulosAtivos { get; set; } = new List<TenantModuloEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se contratação está ativa
    /// </summary>
    /// <returns>True se está ativa</returns>
    public bool EstaAtiva()
    {
        return Status == "ATIVA" && 
               (DataFim == null || DataFim > DateTime.UtcNow) &&
               StatusPagamento != "ATRASADO";
    }

    /// <summary>
    /// Verifica se está vencida
    /// </summary>
    /// <returns>True se venceu</returns>
    public bool EstaVencida()
    {
        return DataFim != null && DataFim <= DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula dias até vencimento
    /// </summary>
    /// <returns>Número de dias até vencer</returns>
    public int DiasParaVencer()
    {
        if (DataFim == null) return int.MaxValue;
        return Math.Max(0, (DataFim.Value.Date - DateTime.UtcNow.Date).Days);
    }

    /// <summary>
    /// Renova contratação por período específico
    /// </summary>
    /// <param name="meses">Número de meses para renovar</param>
    public void Renovar(int meses)
    {
        var novaDataFim = DataFim?.AddMonths(meses) ?? DateTime.UtcNow.AddMonths(meses);
        DataFim = novaDataFim;
        
        if (TipoPagamento == "MENSAL")
            DataProximaCobranca = DateTime.UtcNow.AddMonths(1);
        else
            DataProximaCobranca = DateTime.UtcNow.AddYears(1);

        Status = "ATIVA";
        StatusPagamento = "EM_DIA";
        DiasAtraso = 0;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela contratação
    /// </summary>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <param name="usuario">Usuário que cancelou</param>
    public void Cancelar(string motivo, string usuario)
    {
        Status = "CANCELADA";
        MotivoCancelamento = motivo;
        DataCancelamento = DateTime.UtcNow;
        CanceladoPor = usuario;
        RenovacaoAutomatica = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspende contratação temporariamente
    /// </summary>
    /// <param name="motivo">Motivo da suspensão</param>
    public void Suspender(string motivo)
    {
        Status = "SUSPENSA";
        Observacoes = $"Suspensa em {DateTime.UtcNow:dd/MM/yyyy}: {motivo}. {Observacoes}";
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Reativa contratação suspensa
    /// </summary>
    public void Reativar()
    {
        Status = "ATIVA";
        Observacoes = $"Reativada em {DateTime.UtcNow:dd/MM/yyyy}. {Observacoes}";
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria nova contratação de plano
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="planoId">ID do plano</param>
    /// <param name="tipoPagamento">Tipo de pagamento</param>
    /// <param name="valorContratado">Valor contratado</param>
    /// <param name="periodoTeste">Se é período de teste</param>
    /// <returns>Nova instância de contratação</returns>
    public static TenantPlanoEntity CriarNova(
        string tenantId,
        Guid planoId,
        string tipoPagamento,
        decimal valorContratado,
        bool periodoTeste = false)
    {
        var contratacao = new TenantPlanoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanoId = planoId,
            TipoPagamento = tipoPagamento,
            ValorContratado = valorContratado,
            PeriodoTeste = periodoTeste,
            DataInicio = DateTime.UtcNow,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        // Define data de fim baseada no tipo de pagamento
        if (periodoTeste)
        {
            contratacao.DataFim = DateTime.UtcNow.AddDays(14); // 14 dias de teste
            contratacao.DiasTesteRestantes = 14;
        }
        else
        {
            if (tipoPagamento == "ANUAL")
                contratacao.DataFim = DateTime.UtcNow.AddYears(1);
            else
                contratacao.DataFim = DateTime.UtcNow.AddMonths(1);
        }

        // Define próxima cobrança
        if (!periodoTeste)
        {
            contratacao.DataProximaCobranca = tipoPagamento == "ANUAL" 
                ? DateTime.UtcNow.AddYears(1)
                : DateTime.UtcNow.AddMonths(1);
        }

        return contratacao;
    }
}

/// <summary>
/// Entidade que representa módulos ativos específicos de um tenant
/// Permite controle granular de módulos por farmácia
/// </summary>
/// <remarks>
/// Cada farmácia pode ter módulos específicos ativados/desativados,
/// mesmo dentro do mesmo plano comercial (para customizações)
/// </remarks>
[Table("TenantModulos")]
public class TenantModuloEntity
{
    /// <summary>
    /// Identificador único da ativação de módulo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do módulo
    /// </summary>
    [Required]
    public Guid ModuloId { get; set; }

    /// <summary>
    /// Se o módulo está ativo para este tenant
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data de ativação do módulo
    /// </summary>
    public DateTime DataAtivacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de desativação (se aplicável)
    /// </summary>
    public DateTime? DataDesativacao { get; set; }

    /// <summary>
    /// Configurações específicas do módulo para este tenant (JSON)
    /// </summary>
    [StringLength(2000)]
    public string? ConfiguracoesEspecificas { get; set; }

    /// <summary>
    /// Limitações específicas para este tenant (JSON)
    /// </summary>
    [StringLength(1000)]
    public string? LimitacoesEspecificas { get; set; }

    /// <summary>
    /// Usuário que ativou o módulo
    /// </summary>
    [StringLength(100)]
    public string? AtivadoPor { get; set; }

    /// <summary>
    /// Motivo da ativação/desativação
    /// </summary>
    [StringLength(500)]
    public string? Motivo { get; set; }

    // Navegação

    /// <summary>
    /// Módulo ativado
    /// </summary>
    public virtual ModuloEntity? Modulo { get; set; }

    /// <summary>
    /// Contratação de plano associada
    /// </summary>
    public virtual TenantPlanoEntity? TenantPlano { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Ativa módulo para tenant
    /// </summary>
    /// <param name="usuario">Usuário que está ativando</param>
    /// <param name="motivo">Motivo da ativação</param>
    public void Ativar(string usuario, string? motivo = null)
    {
        Ativo = true;
        DataAtivacao = DateTime.UtcNow;
        DataDesativacao = null;
        AtivadoPor = usuario;
        Motivo = motivo;
    }

    /// <summary>
    /// Desativa módulo para tenant
    /// </summary>
    /// <param name="usuario">Usuário que está desativando</param>
    /// <param name="motivo">Motivo da desativação</param>
    public void Desativar(string usuario, string motivo)
    {
        Ativo = false;
        DataDesativacao = DateTime.UtcNow;
        AtivadoPor = usuario;
        Motivo = motivo;
    }

    /// <summary>
    /// Cria ativação de módulo para tenant
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="moduloId">ID do módulo</param>
    /// <param name="usuario">Usuário que está ativando</param>
    /// <returns>Nova instância da ativação</returns>
    public static TenantModuloEntity CriarNova(
        string tenantId,
        Guid moduloId,
        string usuario)
    {
        return new TenantModuloEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ModuloId = moduloId,
            DataAtivacao = DateTime.UtcNow,
            AtivadoPor = usuario
        };
    }
}