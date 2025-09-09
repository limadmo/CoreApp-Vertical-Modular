using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa o histórico de consentimentos LGPD de um cliente
/// Mantém auditoria completa de todas as alterações de consentimento para compliance
/// </summary>
/// <remarks>
/// Esta entidade é essencial para compliance LGPD, registrando toda mudança
/// de consentimento do cliente com timestamp e detalhes para auditoria
/// </remarks>
[Table("ClienteConsentimentos")]
public class ClienteConsentimentoEntity : ITenantEntity
{
    /// <summary>
    /// Identificador único do registro de consentimento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do cliente proprietário do consentimento
    /// </summary>
    [Required]
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Versão dos termos de privacidade aceitos
    /// </summary>
    [Required]
    [StringLength(20)]
    public string VersaoTermos { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora do consentimento
    /// </summary>
    public DateTime DataConsentimento { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tipo de ação realizada (CONSENTIMENTO, REVOGACAO, ATUALIZACAO, ANONIMIZACAO)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoAcao { get; set; } = string.Empty;

    /// <summary>
    /// Finalidades autorizadas neste consentimento (JSON)
    /// Ex: ["marketing", "historico_medico", "promocoes", "compartilhamento_plano"]
    /// </summary>
    [StringLength(1000)]
    public string? FinalidadesAutorizadas { get; set; }

    /// <summary>
    /// Se autorizou marketing direto
    /// </summary>
    public bool AutorizouMarketing { get; set; } = false;

    /// <summary>
    /// Se autorizou armazenamento de histórico médico
    /// </summary>
    public bool AutorizouHistoricoMedico { get; set; } = false;

    /// <summary>
    /// Se autorizou compartilhamento com planos de saúde
    /// </summary>
    public bool AutorizouCompartilhamentoPlanoSaude { get; set; } = false;

    /// <summary>
    /// Se aceitou receber notificações por email
    /// </summary>
    public bool AceitouNotificacaoEmail { get; set; } = false;

    /// <summary>
    /// Se aceitou receber notificações por SMS
    /// </summary>
    public bool AceitouNotificacaoSms { get; set; } = false;

    /// <summary>
    /// Se aceitou receber notificações por WhatsApp
    /// </summary>
    public bool AceitouNotificacaoWhatsapp { get; set; } = false;

    /// <summary>
    /// Se aceitou receber promoções e ofertas
    /// </summary>
    public bool AceitouPromocoes { get; set; } = false;

    // Dados de auditoria técnica

    /// <summary>
    /// IP de origem da ação de consentimento
    /// </summary>
    [StringLength(45)] // IPv6
    public string? IpOrigem { get; set; }

    /// <summary>
    /// User Agent do navegador/aplicativo
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Identificador da sessão/dispositivo
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Método de consentimento (WEB, MOBILE, PRESENCIAL, TELEFONE)
    /// </summary>
    [StringLength(20)]
    public string MetodoConsentimento { get; set; } = "WEB";

    /// <summary>
    /// Canal específico (SITE, APP, LOJA, CALL_CENTER)
    /// </summary>
    [StringLength(30)]
    public string? CanalConsentimento { get; set; }

    /// <summary>
    /// Usuário da farmácia que processou o consentimento (se aplicável)
    /// </summary>
    [StringLength(100)]
    public string? UsuarioProcessou { get; set; }

    /// <summary>
    /// Localização geográfica aproximada do consentimento (cidade/estado)
    /// </summary>
    [StringLength(100)]
    public string? LocalizacaoAproximada { get; set; }

    // Detalhes específicos da ação

    /// <summary>
    /// Motivo da alteração de consentimento
    /// </summary>
    [StringLength(500)]
    public string? MotivoAlteracao { get; set; }

    /// <summary>
    /// Observações adicionais sobre o consentimento
    /// </summary>
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Se o consentimento foi dado de forma livre e esclarecida
    /// </summary>
    public bool ConsentimentoLivreEsclarecido { get; set; } = true;

    /// <summary>
    /// Se o cliente teve acesso completo aos termos de privacidade
    /// </summary>
    public bool AcessoCompletosTermos { get; set; } = true;

    /// <summary>
    /// Tempo em segundos que o cliente levou para ler os termos (se aplicável)
    /// </summary>
    public int? TempoLeituraTermos { get; set; }

    /// <summary>
    /// Hash dos termos apresentados ao cliente (para verificar se não foram alterados)
    /// </summary>
    [StringLength(255)]
    public string? HashTermosApresentados { get; set; }

    // Para casos de revogação ou anonimização

    /// <summary>
    /// Data de efetivação da revogação/anonimização (se aplicável)
    /// </summary>
    public DateTime? DataEfetivacao { get; set; }

    /// <summary>
    /// Status do processamento da solicitação
    /// </summary>
    [StringLength(30)]
    public string StatusProcessamento { get; set; } = "EFETIVADO";

    /// <summary>
    /// Data limite para processamento de solicitações LGPD (15 dias úteis)
    /// </summary>
    public DateTime? DataLimiteProcessamento { get; set; }

    // Navegação

    /// <summary>
    /// Cliente proprietário deste consentimento
    /// </summary>
    public virtual ClienteEntity? Cliente { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Cria registro de novo consentimento LGPD
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="versaoTermos">Versão dos termos aceitos</param>
    /// <param name="finalidades">Finalidades autorizadas</param>
    /// <param name="marketing">Autorizou marketing</param>
    /// <param name="historicoMedico">Autorizou histórico médico</param>
    /// <param name="compartilhamentoPlano">Autorizou compartilhamento com plano</param>
    /// <param name="ipOrigem">IP de origem</param>
    /// <param name="userAgent">User agent</param>
    /// <returns>Nova instância de consentimento</returns>
    public static ClienteConsentimentoEntity CriarConsentimento(
        Guid clienteId,
        string tenantId,
        string versaoTermos,
        string? finalidades = null,
        bool marketing = false,
        bool historicoMedico = false,
        bool compartilhamentoPlano = false,
        string? ipOrigem = null,
        string? userAgent = null)
    {
        return new ClienteConsentimentoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            TenantId = tenantId,
            VersaoTermos = versaoTermos,
            TipoAcao = "CONSENTIMENTO",
            FinalidadesAutorizadas = finalidades,
            AutorizouMarketing = marketing,
            AutorizouHistoricoMedico = historicoMedico,
            AutorizouCompartilhamentoPlanoSaude = compartilhamentoPlano,
            IpOrigem = ipOrigem,
            UserAgent = userAgent?.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
            ConsentimentoLivreEsclarecido = true,
            AcessoCompletosTermos = true,
            StatusProcessamento = "EFETIVADO"
        };
    }

    /// <summary>
    /// Cria registro de revogação de consentimento
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="motivo">Motivo da revogação</param>
    /// <param name="ipOrigem">IP de origem</param>
    /// <returns>Nova instância de revogação</returns>
    public static ClienteConsentimentoEntity CriarRevogacao(
        Guid clienteId,
        string tenantId,
        string motivo,
        string? ipOrigem = null)
    {
        return new ClienteConsentimentoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            TenantId = tenantId,
            TipoAcao = "REVOGACAO",
            MotivoAlteracao = motivo,
            IpOrigem = ipOrigem,
            ConsentimentoLivreEsclarecido = true,
            StatusProcessamento = "EFETIVADO",
            
            // Todos os consentimentos revogados
            AutorizouMarketing = false,
            AutorizouHistoricoMedico = false,
            AutorizouCompartilhamentoPlanoSaude = false,
            AceitouNotificacaoEmail = false,
            AceitouNotificacaoSms = false,
            AceitouNotificacaoWhatsapp = false,
            AceitouPromocoes = false
        };
    }

    /// <summary>
    /// Cria registro de solicitação de anonimização (direito ao esquecimento)
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="motivo">Motivo da solicitação</param>
    /// <param name="ipOrigem">IP de origem</param>
    /// <returns>Nova instância de solicitação de anonimização</returns>
    public static ClienteConsentimentoEntity CriarSolicitacaoAnonimizacao(
        Guid clienteId,
        string tenantId,
        string motivo,
        string? ipOrigem = null)
    {
        var dataLimite = CalcularDataLimiteProcessamento(DateTime.UtcNow);

        return new ClienteConsentimentoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            TenantId = tenantId,
            TipoAcao = "ANONIMIZACAO",
            MotivoAlteracao = motivo,
            IpOrigem = ipOrigem,
            StatusProcessamento = "PENDENTE",
            DataLimiteProcessamento = dataLimite,
            ConsentimentoLivreEsclarecido = true
        };
    }

    /// <summary>
    /// Marca solicitação como efetivada
    /// </summary>
    public void MarcarComoEfetivada()
    {
        StatusProcessamento = "EFETIVADO";
        DataEfetivacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se solicitação está dentro do prazo LGPD
    /// </summary>
    /// <returns>True se ainda está no prazo</returns>
    public bool EstaNoPrazo()
    {
        return DataLimiteProcessamento == null || 
               DateTime.UtcNow <= DataLimiteProcessamento.Value;
    }

    /// <summary>
    /// Calcula data limite para processamento de solicitações LGPD (15 dias úteis)
    /// </summary>
    /// <param name="dataSolicitacao">Data da solicitação</param>
    /// <returns>Data limite considerando dias úteis</returns>
    public static DateTime CalcularDataLimiteProcessamento(DateTime dataSolicitacao)
    {
        var diasUteis = 0;
        var dataAtual = dataSolicitacao.Date;

        while (diasUteis < 15)
        {
            dataAtual = dataAtual.AddDays(1);
            
            // Pular fins de semana
            if (dataAtual.DayOfWeek != DayOfWeek.Saturday && 
                dataAtual.DayOfWeek != DayOfWeek.Sunday)
            {
                diasUteis++;
            }
        }

        return dataAtual;
    }

    /// <summary>
    /// Gera resumo do consentimento para auditoria
    /// </summary>
    /// <returns>Objeto com resumo sanitizado</returns>
    public object GerarResumoAuditoria()
    {
        return new
        {
            Id = Id,
            ClienteId = ClienteId,
            TenantId = TenantId,
            TipoAcao = TipoAcao,
            DataConsentimento = DataConsentimento,
            VersaoTermos = VersaoTermos,
            StatusProcessamento = StatusProcessamento,
            MetodoConsentimento = MetodoConsentimento,
            ConsentimentoLivreEsclarecido = ConsentimentoLivreEsclarecido,
            
            // Resumo das autorizações
            Autorizacoes = new
            {
                Marketing = AutorizouMarketing,
                HistoricoMedico = AutorizouHistoricoMedico,
                CompartilhamentoPlano = AutorizouCompartilhamentoPlanoSaude,
                NotificacaoEmail = AceitouNotificacaoEmail,
                NotificacaoSms = AceitouNotificacaoSms,
                NotificacaoWhatsapp = AceitouNotificacaoWhatsapp,
                Promocoes = AceitouPromocoes
            },
            
            // Dados técnicos sanitizados (sem dados pessoais completos)
            DadosTecnicos = new
            {
                IpOrigemAnonimizado = IpOrigem?.Length > 4 ? 
                    $"{IpOrigem.Substring(0, IpOrigem.LastIndexOf('.'))}.*" : IpOrigem,
                UserAgentResumido = UserAgent?.Length > 50 ? 
                    UserAgent.Substring(0, 50) + "..." : UserAgent,
                LocalizacaoAproximada = LocalizacaoAproximada
            }
        };
    }
}