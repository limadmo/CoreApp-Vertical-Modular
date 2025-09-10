using System.ComponentModel.DataAnnotations;
using CoreApp.Domain.Entities.Common;

namespace CoreApp.Domain.Entities.Configuration;

/// <summary>
/// Entidade de configuração para status de sincronização offline farmacêutica
/// Permite configurar diferentes estados de sincronização por farmácia
/// </summary>
/// <remarks>
/// Esta entidade substitui o enum StatusSincronizacao por um sistema flexível que permite:
/// - Configurações globais do sistema (padrões para operação offline)
/// - Customizações específicas por farmácia
/// - Controle de operações offline do PDV Raspberry Pi
/// - Cores e ícones customizáveis para interface
/// - Alterações sem necessidade de deploy
/// </remarks>
public class StatusSincronizacaoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do status
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (null = configuração global do sistema)
    /// </summary>
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único do status (usado na API e integrações)
    /// </summary>
    /// <example>PENDENTE, SINCRONIZADO, ERRO, CONFLITO, SINCRONIZANDO</example>
    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo exibido na interface
    /// </summary>
    /// <example>Pendente Sincronização, Sincronizado, Erro na Sincronização</example>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do status
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Categoria do status para agrupamento
    /// </summary>
    /// <example>PROCESSANDO, FINALIZADO, ERRO</example>
    [StringLength(30)]
    public string? Categoria { get; set; }

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    /// <example>#ffc107 (amarelo pendente), #28a745 (verde sincronizado), #dc3545 (vermelho erro)</example>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface (Font Awesome, Material Icons, etc.)
    /// </summary>
    /// <example>fa-clock, fa-sync-alt, fa-exclamation-triangle, fa-times-circle</example>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Ordem de exibição em listas (menor número aparece primeiro)
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Se é um status final (não pode mudar para outro status)
    /// </summary>
    public bool IsStatusFinal { get; set; } = false;

    /// <summary>
    /// Se permite retry automático
    /// </summary>
    public bool PermiteRetry { get; set; } = true;

    /// <summary>
    /// Número máximo de tentativas de sincronização
    /// </summary>
    [Range(1, 10)]
    public int? MaximoTentativas { get; set; }

    /// <summary>
    /// Intervalo em minutos entre tentativas
    /// </summary>
    [Range(1, 1440)] // Máximo 24 horas
    public int? IntervaloTentativasMinutos { get; set; }

    /// <summary>
    /// Se deve gerar alerta para operadores
    /// </summary>
    public bool GerarAlerta { get; set; } = false;

    /// <summary>
    /// Prioridade do alerta (1 = mais alta, 5 = mais baixa)
    /// </summary>
    [Range(1, 5)]
    public int? PrioridadeAlerta { get; set; }

    /// <summary>
    /// Se deve notificar administradores
    /// </summary>
    public bool NotificarAdministradores { get; set; } = false;

    /// <summary>
    /// Se deve bloquear novas operações offline
    /// </summary>
    public bool BloquearOperacoesOffline { get; set; } = false;

    /// <summary>
    /// Se deve aparecer no dashboard de monitoramento
    /// </summary>
    public bool AparecerDashboard { get; set; } = true;

    /// <summary>
    /// Tempo limite em minutos para considerar timeout
    /// </summary>
    [Range(1, 60)] // Máximo 1 hora
    public int? TempoLimiteMinutos { get; set; }

    /// <summary>
    /// Status para o qual deve transicionar após timeout
    /// </summary>
    [StringLength(30)]
    public string? StatusAposTimeout { get; set; }

    /// <summary>
    /// Mensagem padrão para este status
    /// </summary>
    [StringLength(200)]
    public string? MensagemPadrao { get; set; }

    /// <summary>
    /// Próximos status válidos (transições permitidas)
    /// </summary>
    [StringLength(500)]
    public string? ProximosStatusValidos { get; set; }

    /// <summary>
    /// Se deve sincronizar imediatamente quando possível
    /// </summary>
    public bool SincronizacaoImediata { get; set; } = false;

    /// <summary>
    /// Tipos de dados que podem estar neste status
    /// </summary>
    /// <example>VENDAS, MOVIMENTACOES, PRODUTOS, CLIENTES</example>
    [StringLength(200)]
    public string? TiposDadosAplicaveis { get; set; }

    /// <summary>
    /// Status ativo/inativo (permite desativar sem perder histórico)
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se é uma configuração padrão do sistema (não pode ser removida)
    /// </summary>
    public bool IsSistema { get; set; } = false;

    /// <summary>
    /// ID da configuração global pai (para herança)
    /// </summary>
    public Guid? ConfiguracaoGlobalId { get; set; }

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
    /// Configuração global pai (para herança de configurações)
    /// </summary>
    public virtual StatusSincronizacaoEntity? ConfiguracaoGlobal { get; set; }

    /// <summary>
    /// Configurações filhas que herdam desta
    /// </summary>
    public virtual ICollection<StatusSincronizacaoEntity> ConfiguracoesFilhas { get; set; } = new List<StatusSincronizacaoEntity>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se é uma configuração global (sistema)
    /// </summary>
    /// <returns>True se for global</returns>
    public bool IsGlobal()
    {
        return string.IsNullOrEmpty(TenantId);
    }

    /// <summary>
    /// Verifica se pode ser removido (não é sistema)
    /// </summary>
    /// <returns>True se pode ser removido</returns>
    public bool PodeSerRemovido()
    {
        return !IsSistema;
    }

    /// <summary>
    /// Verifica se pode transicionar para outro status
    /// </summary>
    /// <param name="novoStatus">Código do novo status</param>
    /// <returns>True se transição é permitida</returns>
    public bool PodeTransicionarPara(string novoStatus)
    {
        if (IsStatusFinal)
            return false;

        if (string.IsNullOrEmpty(ProximosStatusValidos))
            return true;

        var statusValidos = ProximosStatusValidos.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return statusValidos.Contains(novoStatus.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica se deve tentar sincronização novamente
    /// </summary>
    /// <param name="numeroTentativas">Número atual de tentativas</param>
    /// <returns>True se deve tentar novamente</returns>
    public bool DeveRetrySync(int numeroTentativas)
    {
        if (!PermiteRetry)
            return false;

        if (!MaximoTentativas.HasValue)
            return true;

        return numeroTentativas < MaximoTentativas.Value;
    }

    /// <summary>
    /// Calcula próximo horário para retry
    /// </summary>
    /// <param name="ultimaTentativa">Horário da última tentativa</param>
    /// <returns>Horário da próxima tentativa</returns>
    public DateTime CalcularProximasTentativas(DateTime ultimaTentativa)
    {
        if (!IntervaloTentativasMinutos.HasValue)
            return ultimaTentativa.AddMinutes(5); // Default 5 minutos

        return ultimaTentativa.AddMinutes(IntervaloTentativasMinutos.Value);
    }

    /// <summary>
    /// Verifica se o status expirou baseado no tempo limite
    /// </summary>
    /// <param name="dataInicio">Data de início do status</param>
    /// <returns>True se status expirou</returns>
    public bool StatusExpirou(DateTime dataInicio)
    {
        if (!TempoLimiteMinutos.HasValue)
            return false;

        var tempoLimite = dataInicio.AddMinutes(TempoLimiteMinutos.Value);
        return DateTime.Now > tempoLimite;
    }

    /// <summary>
    /// Verifica se um tipo de dado é aplicável para este status
    /// </summary>
    /// <param name="tipoDado">Tipo de dado a verificar</param>
    /// <returns>True se é aplicável</returns>
    public bool TipoDadoAplicavel(string tipoDado)
    {
        if (string.IsNullOrEmpty(TiposDadosAplicaveis))
            return true; // Se não especificado, aplica a todos

        var tipos = TiposDadosAplicaveis.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return tipos.Contains(tipoDado.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtém a cor padrão baseada no código do status
    /// </summary>
    /// <returns>Código da cor hexadecimal</returns>
    public string ObterCorPadrao()
    {
        if (!string.IsNullOrEmpty(Cor))
            return Cor;

        return Codigo switch
        {
            "PENDENTE" => "#FFC107",          // Amarelo
            "SINCRONIZADO" => "#28A745",      // Verde
            "ERRO" => "#DC3545",              // Vermelho
            "CONFLITO" => "#E83E8C",          // Rosa
            "SINCRONIZANDO" => "#007BFF",     // Azul
            "TIMEOUT" => "#6C757D",           // Cinza
            _ => "#6C757D"                    // Cinza padrão
        };
    }

    /// <summary>
    /// Obtém o ícone padrão baseado no código do status
    /// </summary>
    /// <returns>Classe do ícone</returns>
    public string ObterIconePadrao()
    {
        if (!string.IsNullOrEmpty(Icone))
            return Icone;

        return Codigo switch
        {
            "PENDENTE" => "fa-clock",
            "SINCRONIZADO" => "fa-check-circle",
            "ERRO" => "fa-times-circle",
            "CONFLITO" => "fa-exclamation-triangle",
            "SINCRONIZANDO" => "fa-sync-alt fa-spin",
            "TIMEOUT" => "fa-hourglass-end",
            _ => "fa-question-circle"
        };
    }

    /// <summary>
    /// Obtém mensagem padrão baseada no código do status
    /// </summary>
    /// <returns>Mensagem padrão</returns>
    public string ObterMensagemPadrao()
    {
        if (!string.IsNullOrEmpty(MensagemPadrao))
            return MensagemPadrao;

        return Codigo switch
        {
            "PENDENTE" => "Aguardando sincronização com o servidor",
            "SINCRONIZADO" => "Dados sincronizados com sucesso",
            "ERRO" => "Erro na sincronização - verifique conexão",
            "CONFLITO" => "Conflito detectado - intervenção necessária",
            "SINCRONIZANDO" => "Sincronização em andamento",
            "TIMEOUT" => "Timeout na sincronização",
            _ => "Status de sincronização atualizado"
        };
    }

    /// <summary>
    /// Obtém próximos status válidos como lista
    /// </summary>
    /// <returns>Lista de códigos de status válidos</returns>
    public List<string> ObterProximosStatusValidos()
    {
        if (string.IsNullOrEmpty(ProximosStatusValidos))
            return new List<string>();

        return ProximosStatusValidos
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    /// <summary>
    /// Obtém tipos de dados aplicáveis como lista
    /// </summary>
    /// <returns>Lista de tipos de dados</returns>
    public List<string> ObterTiposDadosAplicaveis()
    {
        if (string.IsNullOrEmpty(TiposDadosAplicaveis))
            return new List<string>();

        return TiposDadosAplicaveis
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    /// <summary>
    /// Atualiza o timestamp de modificação
    /// </summary>
    public void AtualizarTimestamp()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma cópia desta configuração para um tenant específico
    /// </summary>
    /// <param name="tenantId">ID do tenant de destino</param>
    /// <returns>Nova configuração personalizada</returns>
    public StatusSincronizacaoEntity CriarPersonalizacao(string tenantId)
    {
        return new StatusSincronizacaoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            Categoria = Categoria,
            Cor = Cor,
            Icone = Icone,
            Ordem = Ordem,
            IsStatusFinal = IsStatusFinal,
            PermiteRetry = PermiteRetry,
            MaximoTentativas = MaximoTentativas,
            IntervaloTentativasMinutos = IntervaloTentativasMinutos,
            GerarAlerta = GerarAlerta,
            PrioridadeAlerta = PrioridadeAlerta,
            NotificarAdministradores = NotificarAdministradores,
            BloquearOperacoesOffline = BloquearOperacoesOffline,
            AparecerDashboard = AparecerDashboard,
            TempoLimiteMinutos = TempoLimiteMinutos,
            StatusAposTimeout = StatusAposTimeout,
            MensagemPadrao = MensagemPadrao,
            ProximosStatusValidos = ProximosStatusValidos,
            SincronizacaoImediata = SincronizacaoImediata,
            TiposDadosAplicaveis = TiposDadosAplicaveis,
            Ativo = Ativo,
            IsSistema = false, // Personalizações nunca são do sistema
            ConfiguracaoGlobalId = Id, // Referencia a configuração original
            CriadoPor = "SISTEMA_HERANCA"
        };
    }

    /// <summary>
    /// Cria novo status de sincronização padrão
    /// </summary>
    /// <param name="codigo">Código do status</param>
    /// <param name="nome">Nome descritivo</param>
    /// <param name="categoria">Categoria</param>
    /// <param name="permiteRetry">Se permite retry</param>
    /// <returns>Nova instância de status</returns>
    public static StatusSincronizacaoEntity CriarStatusPadrao(
        string codigo,
        string nome,
        string categoria = "GERAL",
        bool permiteRetry = true)
    {
        return new StatusSincronizacaoEntity
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            Categoria = categoria,
            PermiteRetry = permiteRetry,
            IsSistema = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true,
            CriadoPor = "SISTEMA_PADRAO"
        };
    }
}