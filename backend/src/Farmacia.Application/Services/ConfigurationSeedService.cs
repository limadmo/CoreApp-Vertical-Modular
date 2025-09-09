using Microsoft.Extensions.Logging;
using Farmacia.Infrastructure.Data.Context;
using Farmacia.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço responsável por criar seeds das configurações farmacêuticas brasileiras
/// Cria apenas dados oficiais da ANVISA e configurações padrões do sistema
/// </summary>
/// <remarks>
/// Este serviço implementa seeds para:
/// - Classificações ANVISA oficiais (apenas listas controladas A1-C5)
/// - Status de estoque padrões
/// - Formas de pagamento brasileiras
/// - Status de pagamento e sincronização
/// </remarks>
public class ConfigurationSeedService : IConfigurationSeedService
{
    private readonly FarmaciaDbContext _context;
    private readonly ILogger<ConfigurationSeedService> _logger;

    public ConfigurationSeedService(
        FarmaciaDbContext context,
        ILogger<ConfigurationSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Executa todos os seeds de configuração
    /// </summary>
    public async Task SeedAllConfigurationsAsync()
    {
        _logger.LogInformation("🌱 Iniciando seed das configurações farmacêuticas brasileiras");

        await SeedClassificacoesAnvisaAsync();
        await SeedStatusEstoqueAsync();
        await SeedFormasPagamentoAsync();
        await SeedStatusPagamentoAsync();
        await SeedStatusSincronizacaoAsync();

        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ Seeds de configuração concluídos com sucesso");
    }

    /// <summary>
    /// Cria seeds das classificações ANVISA oficiais (apenas listas controladas)
    /// </summary>
    public async Task SeedClassificacoesAnvisaAsync()
    {
        _logger.LogInformation("🏥 Criando seeds das classificações ANVISA controladas");

        // Verifica se já existem classificações
        if (await _context.ClassificacoesAnvisa.AnyAsync(c => c.IsOficialAnvisa))
        {
            _logger.LogDebug("Classificações ANVISA já existem, pulando seed");
            return;
        }

        var classificacoes = new[]
        {
            // LISTA A - ENTORPECENTES (Receita Azul)
            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "A1", 
                "Lista A1 - Entorpecentes", 
                "AZUL"
            ).ConfigurarDetalhes(
                "Substâncias entorpecentes de uso proibido no Brasil",
                "#0066CC", // Cor azul da receita
                "fa-ban",
                true, // Requer retenção
                30, // 30 dias de validade
                null, // Sem limite específico de quantidade
                true, // Requer autorização especial
                true, // Reportar SNGPC
                "ENTORPECENTE",
                1, // Ordem
                "FARMACEUTICO_RESPONSAVEL"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "A2", 
                "Lista A2 - Entorpecentes de uso permitido", 
                "AZUL"
            ).ConfigurarDetalhes(
                "Substâncias entorpecentes de uso permitido somente em casos excepcionais",
                "#0066CC",
                "fa-prescription-bottle",
                true,
                30,
                null,
                true,
                true,
                "ENTORPECENTE",
                2,
                "FARMACEUTICO_RESPONSAVEL"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "A3", 
                "Lista A3 - Psicotrópicos", 
                "AZUL"
            ).ConfigurarDetalhes(
                "Substâncias psicotrópicas com alto potencial de dependência",
                "#0066CC",
                "fa-brain",
                true,
                30,
                null,
                true,
                true,
                "PSICOTROPICO",
                3,
                "FARMACEUTICO_RESPONSAVEL"
            ),

            // LISTA B - PSICOTRÓPICOS (Receita Branca)
            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "B1", 
                "Lista B1 - Psicotrópicos", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias psicotrópicas (benzodiazepínicos, barbitúricos, etc.)",
                "#FFFFFF",
                "fa-tablets",
                true,
                30,
                60, // Máximo 60 unidades por receita
                true,
                true,
                "PSICOTROPICO",
                4,
                "FARMACEUTICO"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "B2", 
                "Lista B2 - Anorexígenos", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias psicotrópicas anorexígenas",
                "#FFFFFF",
                "fa-weight",
                true,
                30,
                30, // Máximo 30 unidades por receita
                true,
                true,
                "ANOREXIGENO",
                5,
                "FARMACEUTICO"
            ),

            // LISTA C - OUTRAS SUBSTÂNCIAS CONTROLADAS
            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "C1", 
                "Lista C1 - Outras substâncias sujeitas a controle especial", 
                "BRANCA_2VIAS"
            ).ConfigurarDetalhes(
                "Outras substâncias que causam dependência física ou psíquica",
                "#FFFFFF",
                "fa-pills",
                true,
                30,
                null,
                false, // Não requer autorização especial
                true,
                "CONTROLADO_ESPECIAL",
                6,
                "FARMACEUTICO"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "C2", 
                "Lista C2 - Retinóicas", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias retinóicas para tratamento de acne",
                "#FFFFFF",
                "fa-user-md",
                false, // Não requer retenção
                30,
                null,
                false,
                false, // Não reportar SNGPC
                "RETINOICO",
                7,
                "FARMACEUTICO"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "C3", 
                "Lista C3 - Imunossupressoras", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias imunossupressoras",
                "#FFFFFF",
                "fa-shield-virus",
                false,
                30,
                null,
                false,
                false,
                "IMUNOSSUPRESSOR",
                8,
                "FARMACEUTICO"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "C4", 
                "Lista C4 - Anti-retrovirais", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias anti-retrovirais",
                "#FFFFFF",
                "fa-ribbon",
                false,
                30,
                null,
                false,
                false,
                "ANTIRRETROVIRAL",
                9,
                "FARMACEUTICO"
            ),

            ClassificacaoAnvisaEntity.CriarClassificacaoOficial(
                "C5", 
                "Lista C5 - Anabolizantes", 
                "BRANCA"
            ).ConfigurarDetalhes(
                "Substâncias anabolizantes",
                "#FFFFFF",
                "fa-dumbbell",
                true,
                30,
                null,
                true,
                true,
                "ANABOLIZANTE",
                10,
                "FARMACEUTICO_RESPONSAVEL"
            )
        };

        await _context.ClassificacoesAnvisa.AddRangeAsync(classificacoes);
        _logger.LogInformation($"✅ {classificacoes.Length} classificações ANVISA criadas");
    }

    /// <summary>
    /// Cria seeds dos status de estoque padrões
    /// </summary>
    public async Task SeedStatusEstoqueAsync()
    {
        _logger.LogInformation("📦 Criando seeds dos status de estoque");

        if (await _context.StatusEstoque.AnyAsync(s => s.IsSistema))
        {
            _logger.LogDebug("Status de estoque já existem, pulando seed");
            return;
        }

        var statusEstoque = new[]
        {
            StatusEstoqueEntity.CriarStatusPadrao("NORMAL", "Estoque Normal", 100, null)
                .ConfigurarAlertas(false, null, false, false, "#28A745", "fa-check-circle", 1),

            StatusEstoqueEntity.CriarStatusPadrao("BAIXO", "Estoque Baixo", 20, 100)
                .ConfigurarAlertas(true, 3, true, false, "#FFC107", "fa-exclamation-triangle", 2),

            StatusEstoqueEntity.CriarStatusPadrao("CRITICO", "Estoque Crítico", 0.1m, 20)
                .ConfigurarAlertas(true, 1, true, true, "#DC3545", "fa-times-circle", 3),

            StatusEstoqueEntity.CriarStatusPadrao("ZERADO", "Sem Estoque", 0, 0.1m)
                .ConfigurarAlertas(true, 1, true, true, "#6C757D", "fa-ban", 4)
                .ConfigurarBloqueios(true),

            StatusEstoqueEntity.CriarStatusPadrao("EXCESSIVO", "Estoque Excessivo", 200, null)
                .ConfigurarAlertas(true, 4, false, false, "#17A2B8", "fa-arrow-up", 5)
        };

        await _context.StatusEstoque.AddRangeAsync(statusEstoque);
        _logger.LogInformation($"✅ {statusEstoque.Length} status de estoque criados");
    }

    /// <summary>
    /// Cria seeds das formas de pagamento brasileiras
    /// </summary>
    public async Task SeedFormasPagamentoAsync()
    {
        _logger.LogInformation("💳 Criando seeds das formas de pagamento brasileiras");

        if (await _context.FormasPagamento.AnyAsync(f => f.IsSistema))
        {
            _logger.LogDebug("Formas de pagamento já existem, pulando seed");
            return;
        }

        var formasPagamento = new[]
        {
            FormaPagamentoEntity.CriarFormaPagamentoPadrao("PIX", "PIX", "DIGITAL")
                .ConfigurarPIX(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("DINHEIRO", "Dinheiro", "FISICO")
                .ConfigurarDinheiro(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("DEBITO", "Cartão de Débito", "CARTAO")
                .ConfigurarDebito(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("CREDITO_VISTA", "Cartão de Crédito à Vista", "CARTAO")
                .ConfigurarCreditoVista(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("CREDITO_PARCELADO", "Cartão de Crédito Parcelado", "CARTAO")
                .ConfigurarCreditoParcelado(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("BOLETO", "Boleto Bancário", "DIGITAL")
                .ConfigurarBoleto(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("TRANSFERENCIA", "Transferência Bancária", "DIGITAL")
                .ConfigurarTransferencia(),

            FormaPagamentoEntity.CriarFormaPagamentoPadrao("CONVENIO", "Convênio", "ESPECIAL")
                .ConfigurarConvenio()
        };

        await _context.FormasPagamento.AddRangeAsync(formasPagamento);
        _logger.LogInformation($"✅ {formasPagamento.Length} formas de pagamento criadas");
    }

    /// <summary>
    /// Cria seeds dos status de pagamento padrões
    /// </summary>
    public async Task SeedStatusPagamentoAsync()
    {
        _logger.LogInformation("💰 Criando seeds dos status de pagamento");

        if (await _context.StatusPagamento.AnyAsync(s => s.IsSistema))
        {
            _logger.LogDebug("Status de pagamento já existem, pulando seed");
            return;
        }

        var statusPagamento = new[]
        {
            StatusPagamentoEntity.CriarStatusPadrao("PENDENTE", "Pagamento Pendente", "PROCESSANDO")
                .ConfigurarStatus(false, true, false, false, false, false, false, 15, "REJEITADO", "PROCESSANDO,PAGO,CANCELADO", 1),

            StatusPagamentoEntity.CriarStatusPadrao("PAGO", "Pagamento Confirmado", "FINALIZADO", true)
                .ConfigurarStatus(true, false, true, true, true, true, true, null, null, null, 2),

            StatusPagamentoEntity.CriarStatusPadrao("CANCELADO", "Pagamento Cancelado", "FINALIZADO", true)
                .ConfigurarStatus(true, false, false, false, false, false, false, null, null, null, 3),

            StatusPagamentoEntity.CriarStatusPadrao("PROCESSANDO", "Em Processamento", "PROCESSANDO")
                .ConfigurarStatus(false, true, false, false, false, false, false, 5, "REJEITADO", "PAGO,REJEITADO,CANCELADO", 4),

            StatusPagamentoEntity.CriarStatusPadrao("REJEITADO", "Pagamento Rejeitado", "FINALIZADO", true)
                .ConfigurarStatus(true, false, false, false, false, false, false, null, null, "PENDENTE", 5),

            StatusPagamentoEntity.CriarStatusPadrao("PARCIAL", "Pagamento Parcial", "PROCESSANDO")
                .ConfigurarStatus(false, true, false, false, false, false, true, null, null, "PAGO,CANCELADO", 6)
        };

        await _context.StatusPagamento.AddRangeAsync(statusPagamento);
        _logger.LogInformation($"✅ {statusPagamento.Length} status de pagamento criados");
    }

    /// <summary>
    /// Cria seeds dos status de sincronização offline
    /// </summary>
    public async Task SeedStatusSincronizacaoAsync()
    {
        _logger.LogInformation("🔄 Criando seeds dos status de sincronização");

        if (await _context.StatusSincronizacao.AnyAsync(s => s.IsSistema))
        {
            _logger.LogDebug("Status de sincronização já existem, pulando seed");
            return;
        }

        var statusSync = new[]
        {
            StatusSincronizacaoEntity.CriarStatusPadrao("PENDENTE", "Pendente Sincronização", "PROCESSANDO")
                .ConfigurarSync(false, true, 3, 5, false, 1, false, false, true, 30, "ERRO", "VENDAS,MOVIMENTACOES,PRODUTOS", 1),

            StatusSincronizacaoEntity.CriarStatusPadrao("SINCRONIZADO", "Sincronizado", "FINALIZADO")
                .ConfigurarSync(true, false, null, null, false, null, false, false, true, null, null, null, 2),

            StatusSincronizacaoEntity.CriarStatusPadrao("ERRO", "Erro na Sincronização", "ERRO")
                .ConfigurarSync(false, true, 5, 10, true, 2, true, false, true, null, null, "PENDENTE,CONFLITO", 3),

            StatusSincronizacaoEntity.CriarStatusPadrao("CONFLITO", "Conflito de Dados", "ERRO")
                .ConfigurarSync(false, false, null, null, true, 1, true, true, true, null, null, "SINCRONIZADO,ERRO", 4),

            StatusSincronizacaoEntity.CriarStatusPadrao("SINCRONIZANDO", "Sincronização em Andamento", "PROCESSANDO")
                .ConfigurarSync(false, true, null, null, false, null, false, false, true, 10, "ERRO", "SINCRONIZADO,ERRO", 5)
        };

        await _context.StatusSincronizacao.AddRangeAsync(statusSync);
        _logger.LogInformation($"✅ {statusSync.Length} status de sincronização criados");
    }
}

/// <summary>
/// Interface para o serviço de seed de configurações
/// </summary>
public interface IConfigurationSeedService
{
    Task SeedAllConfigurationsAsync();
    Task SeedClassificacoesAnvisaAsync();
    Task SeedStatusEstoqueAsync();
    Task SeedFormasPagamentoAsync();
    Task SeedStatusPagamentoAsync();
    Task SeedStatusSincronizacaoAsync();
}

// Extension methods para configurar entidades de forma fluente
public static class ConfigurationSeedExtensions
{
    public static ClassificacaoAnvisaEntity ConfigurarDetalhes(
        this ClassificacaoAnvisaEntity entity,
        string descricao,
        string corReceita,
        string icone,
        bool requerRetencao,
        int diasValidade,
        int? quantidadeMaxima,
        bool requerAutorizacao,
        bool reportarSNGPC,
        string categoria,
        int ordem,
        string nivelPermissao)
    {
        entity.Descricao = descricao;
        entity.CorReceita = corReceita;
        entity.Icone = icone;
        entity.RequerRetencaoReceita = requerRetencao;
        entity.DiasValidadeReceita = diasValidade;
        entity.QuantidadeMaximaPorReceita = quantidadeMaxima;
        entity.RequerAutorizacaoEspecial = requerAutorizacao;
        entity.ReportarSNGPC = reportarSNGPC;
        entity.Categoria = categoria;
        entity.Ordem = ordem;
        entity.NivelPermissaoMinimo = nivelPermissao;
        return entity;
    }

    public static StatusEstoqueEntity ConfigurarAlertas(
        this StatusEstoqueEntity entity,
        bool gerarAlerta,
        int? prioridade,
        bool notificarEmail,
        bool notificarWhatsApp,
        string cor,
        string icone,
        int ordem)
    {
        entity.GerarAlerta = gerarAlerta;
        entity.PrioridadeAlerta = prioridade;
        entity.NotificarEmail = notificarEmail;
        entity.NotificarWhatsApp = notificarWhatsApp;
        entity.Cor = cor;
        entity.Icone = icone;
        entity.Ordem = ordem;
        return entity;
    }

    public static StatusEstoqueEntity ConfigurarBloqueios(this StatusEstoqueEntity entity, bool bloquearVendas)
    {
        entity.BloquearVendas = bloquearVendas;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarPIX(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "INSTANTANEO";
        entity.Cor = "#00BC9A";
        entity.Icone = "fa-qrcode";
        entity.Ordem = 1;
        entity.PermiteDesconto = true;
        entity.PercentualDesconto = 5; // 5% desconto para PIX
        entity.DisponivelOnline = true;
        entity.PrazoCompensacao = 0; // Instantâneo
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarDinheiro(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "INSTANTANEO";
        entity.Cor = "#28A745";
        entity.Icone = "fa-money-bill";
        entity.Ordem = 2;
        entity.PermiteTroco = true;
        entity.PermiteDesconto = true;
        entity.PercentualDesconto = 3; // 3% desconto para dinheiro
        entity.DisponivelOnline = false;
        entity.PrazoCompensacao = 0;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarDebito(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "INSTANTANEO";
        entity.Cor = "#007BFF";
        entity.Icone = "fa-credit-card";
        entity.Ordem = 3;
        entity.TaxaPercentual = 2.5m; // 2.5% taxa
        entity.RequerAutenticacao = true;
        entity.DisponivelOnline = true;
        entity.PrazoCompensacao = 1;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarCreditoVista(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "INSTANTANEO";
        entity.Cor = "#FFC107";
        entity.Icone = "fa-credit-card";
        entity.Ordem = 4;
        entity.TaxaPercentual = 3.5m; // 3.5% taxa
        entity.RequerAutenticacao = true;
        entity.DisponivelOnline = true;
        entity.PrazoCompensacao = 1;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarCreditoParcelado(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "PARCELADO";
        entity.Cor = "#FD7E14";
        entity.Icone = "fa-credit-card";
        entity.Ordem = 5;
        entity.PermiteParcelamento = true;
        entity.MaximoParcelas = 12;
        entity.ValorMinimoParcelamento = 50;
        entity.TaxaPercentual = 4.5m; // 4.5% taxa
        entity.TaxaJurosMensal = 2.5m; // 2.5% ao mês
        entity.RequerAutenticacao = true;
        entity.DisponivelOnline = true;
        entity.PrazoCompensacao = 30;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarBoleto(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "A_PRAZO";
        entity.Cor = "#6F42C1";
        entity.Icone = "fa-barcode";
        entity.Ordem = 6;
        entity.ValorMinimo = 20;
        entity.DisponivelOnline = true;
        entity.PrazoCompensacao = 3;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarTransferencia(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "INSTANTANEO";
        entity.Cor = "#20C997";
        entity.Icone = "fa-exchange-alt";
        entity.Ordem = 7;
        entity.DisponivelOnline = false;
        entity.PrazoCompensacao = 1;
        return entity;
    }

    public static FormaPagamentoEntity ConfigurarConvenio(this FormaPagamentoEntity entity)
    {
        entity.TipoPagamento = "A_PRAZO";
        entity.Cor = "#E83E8C";
        entity.Icone = "fa-handshake";
        entity.Ordem = 8;
        entity.RequerAutenticacao = true;
        entity.DisponivelOnline = false;
        entity.PrazoCompensacao = 30;
        return entity;
    }

    public static StatusPagamentoEntity ConfigurarStatus(
        this StatusPagamentoEntity entity,
        bool isStatusFinal,
        bool permiteCancelamento,
        bool permiteEstorno,
        bool gerarComprovante,
        bool enviarNotificacao,
        bool liberarProdutos,
        bool atualizarEstoque,
        int? tempoLimite,
        string? statusTimeout,
        string? proximosStatus,
        int ordem)
    {
        entity.IsStatusFinal = isStatusFinal;
        entity.PermiteCancelamento = permiteCancelamento;
        entity.PermiteEstorno = permiteEstorno;
        entity.GerarComprovante = gerarComprovante;
        entity.EnviarNotificacao = enviarNotificacao;
        entity.LiberarProdutos = liberarProdutos;
        entity.AtualizarEstoque = atualizarEstoque;
        entity.TempoLimiteMinutos = tempoLimite;
        entity.StatusAposTimeout = statusTimeout;
        entity.ProximosStatusValidos = proximosStatus;
        entity.Ordem = ordem;
        return entity;
    }

    public static StatusSincronizacaoEntity ConfigurarSync(
        this StatusSincronizacaoEntity entity,
        bool isStatusFinal,
        bool permiteRetry,
        int? maxTentativas,
        int? intervaloMinutos,
        bool gerarAlerta,
        int? prioridadeAlerta,
        bool notificarAdmins,
        bool bloquearOffline,
        bool aparecerDashboard,
        int? tempoLimite,
        string? statusTimeout,
        string? tiposDados,
        int ordem)
    {
        entity.IsStatusFinal = isStatusFinal;
        entity.PermiteRetry = permiteRetry;
        entity.MaximoTentativas = maxTentativas;
        entity.IntervaloTentativasMinutos = intervaloMinutos;
        entity.GerarAlerta = gerarAlerta;
        entity.PrioridadeAlerta = prioridadeAlerta;
        entity.NotificarAdministradores = notificarAdmins;
        entity.BloquearOperacoesOffline = bloquearOffline;
        entity.AparecerDashboard = aparecerDashboard;
        entity.TempoLimiteMinutos = tempoLimite;
        entity.StatusAposTimeout = statusTimeout;
        entity.TiposDadosAplicaveis = tiposDados;
        entity.Ordem = ordem;
        return entity;
    }
}