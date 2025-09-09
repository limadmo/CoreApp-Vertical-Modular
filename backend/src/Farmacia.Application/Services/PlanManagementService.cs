using Microsoft.Extensions.Logging;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Interfaces;

namespace Farmacia.Application.Services;

/// <summary>
/// Serviço para gerenciar planos comerciais brasileiros do sistema farmacêutico
/// Controla contratações, upgrades, downgrades e sincronização de módulos
/// </summary>
/// <remarks>
/// Este serviço é responsável por toda a lógica de negócio relacionada aos
/// planos comerciais do sistema SAAS farmacêutico brasileiro
/// </remarks>
public class PlanManagementService : IPlanManagementService
{
    private readonly IPlanoComercialRepository _planoRepository;
    private readonly ITenantPlanoRepository _tenantPlanoRepository;
    private readonly ITenantModuloRepository _tenantModuloRepository;
    private readonly IModuleValidationService _moduleValidationService;
    private readonly ILogger<PlanManagementService> _logger;

    public PlanManagementService(
        IPlanoComercialRepository planoRepository,
        ITenantPlanoRepository tenantPlanoRepository,
        ITenantModuloRepository tenantModuloRepository,
        IModuleValidationService moduleValidationService,
        ILogger<PlanManagementService> logger)
    {
        _planoRepository = planoRepository;
        _tenantPlanoRepository = tenantPlanoRepository;
        _tenantModuloRepository = tenantModuloRepository;
        _moduleValidationService = moduleValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Contrata um plano para uma farmácia
    /// </summary>
    /// <param name="request">Dados da contratação</param>
    /// <returns>Resultado da contratação</returns>
    public async Task<PlanContractResult> ContractPlanAsync(ContractPlanRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando contratação do plano {PlanCode} para tenant {TenantId}", 
                request.PlanCode, request.TenantId);

            // Valida se plano existe e está ativo
            var plano = await _planoRepository.GetByCodeAsync(request.PlanCode);
            if (plano == null)
            {
                return PlanContractResult.Failure("Plano não encontrado");
            }

            if (!plano.PodeSerContratado())
            {
                return PlanContractResult.Failure("Plano não está disponível para contratação");
            }

            // Verifica se já existe contratação ativa
            var contratacaoAtiva = await _tenantPlanoRepository.GetActiveByTenantAsync(request.TenantId);
            if (contratacaoAtiva != null)
            {
                return PlanContractResult.Failure("Farmácia já possui um plano ativo. Cancele o plano atual primeiro.");
            }

            // Define valor e tipo de pagamento
            var valorContratado = request.PaymentType == "ANUAL" ? plano.PrecoAnualBRL : plano.PrecoMensalBRL;

            // Cria nova contratação
            var contratacao = TenantPlanoEntity.CriarNova(
                request.TenantId,
                plano.Id,
                request.PaymentType,
                valorContratado,
                request.IsTrialPeriod);

            await _tenantPlanoRepository.AddAsync(contratacao);

            // Sincroniza módulos do plano
            var modulosPlano = plano.ObterCodigosModulos();
            await _tenantModuloRepository.SyncWithPlanModulesAsync(
                request.TenantId, 
                modulosPlano, 
                request.ContractedBy);

            // Limpa cache de módulos
            await _moduleValidationService.ClearCacheAsync(request.TenantId);

            _logger.LogInformation("Plano {PlanCode} contratado com sucesso para tenant {TenantId} - Valor: R$ {Value}", 
                request.PlanCode, request.TenantId, valorContratado);

            return PlanContractResult.Success(contratacao, plano);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contratar plano {PlanCode} para tenant {TenantId}", 
                request.PlanCode, request.TenantId);
            return PlanContractResult.Failure("Erro interno na contratação do plano");
        }
    }

    /// <summary>
    /// Faz upgrade de plano para uma farmácia
    /// </summary>
    /// <param name="request">Dados do upgrade</param>
    /// <returns>Resultado do upgrade</returns>
    public async Task<PlanUpgradeResult> UpgradePlanAsync(UpgradePlanRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando upgrade para plano {NewPlanCode} - tenant {TenantId}", 
                request.NewPlanCode, request.TenantId);

            // Obtém contratação atual
            var contratacaoAtual = await _tenantPlanoRepository.GetWithPlanByTenantAsync(request.TenantId);
            if (contratacaoAtual == null)
            {
                return PlanUpgradeResult.Failure("Nenhum plano ativo encontrado para esta farmácia");
            }

            // Obtém novo plano
            var novoPlano = await _planoRepository.GetWithModulesAsync(request.NewPlanId);
            if (novoPlano == null || !novoPlano.PodeSerContratado())
            {
                return PlanUpgradeResult.Failure("Plano de destino não está disponível");
            }

            // Valida se é realmente um upgrade (preço maior)
            if (novoPlano.PrecoMensalBRL <= contratacaoAtual.Plano!.PrecoMensalBRL)
            {
                return PlanUpgradeResult.Failure("O novo plano deve ter valor superior ao atual para ser considerado upgrade");
            }

            // Calcula valores proporcionais
            var diasRestantes = contratacaoAtual.DiasParaVencer();
            var valorProporcional = CalculateProportionalValue(
                contratacaoAtual.Plano.PrecoMensalBRL,
                novoPlano.PrecoMensalBRL,
                diasRestantes,
                contratacaoAtual.TipoPagamento);

            // Cancela contratação atual
            contratacaoAtual.Cancelar("Upgrade para plano superior", request.UpgradedBy);
            await _tenantPlanoRepository.UpdateAsync(contratacaoAtual);

            // Cria nova contratação
            var novaContratacao = TenantPlanoEntity.CriarNova(
                request.TenantId,
                novoPlano.Id,
                contratacaoAtual.TipoPagamento,
                contratacaoAtual.TipoPagamento == "ANUAL" ? novoPlano.PrecoAnualBRL : novoPlano.PrecoMensalBRL);

            await _tenantPlanoRepository.AddAsync(novaContratacao);

            // Sincroniza módulos do novo plano
            var modulosNovoPlano = novoPlano.ObterCodigosModulos();
            await _tenantModuloRepository.SyncWithPlanModulesAsync(
                request.TenantId,
                modulosNovoPlano,
                request.UpgradedBy);

            // Limpa cache
            await _moduleValidationService.ClearCacheAsync(request.TenantId);

            _logger.LogInformation("Upgrade realizado com sucesso para tenant {TenantId} - De {OldPlan} para {NewPlan}", 
                request.TenantId, contratacaoAtual.Plano.Nome, novoPlano.Nome);

            return PlanUpgradeResult.Success(novaContratacao, novoPlano, valorProporcional);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upgrade do plano para tenant {TenantId}", request.TenantId);
            return PlanUpgradeResult.Failure("Erro interno no upgrade do plano");
        }
    }

    /// <summary>
    /// Cancela plano de uma farmácia
    /// </summary>
    /// <param name="request">Dados do cancelamento</param>
    /// <returns>Resultado do cancelamento</returns>
    public async Task<PlanCancellationResult> CancelPlanAsync(CancelPlanRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando cancelamento de plano para tenant {TenantId}", request.TenantId);

            var contratacaoAtiva = await _tenantPlanoRepository.GetActiveByTenantAsync(request.TenantId);
            if (contratacaoAtiva == null)
            {
                return PlanCancellationResult.Failure("Nenhum plano ativo encontrado para cancelar");
            }

            // Cancela contratação
            contratacaoAtiva.Cancelar(request.Reason, request.CancelledBy);
            await _tenantPlanoRepository.UpdateAsync(contratacaoAtiva);

            // Remove todos os módulos customizados (mantém apenas essenciais)
            await _tenantModuloRepository.RemoveAllByTenantAsync(request.TenantId);

            // Limpa cache
            await _moduleValidationService.ClearCacheAsync(request.TenantId);

            _logger.LogInformation("Plano cancelado com sucesso para tenant {TenantId} - Motivo: {Reason}", 
                request.TenantId, request.Reason);

            return PlanCancellationResult.Success(contratacaoAtiva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no cancelamento do plano para tenant {TenantId}", request.TenantId);
            return PlanCancellationResult.Failure("Erro interno no cancelamento do plano");
        }
    }

    /// <summary>
    /// Obtém planos disponíveis para contratação
    /// </summary>
    /// <param name="tenantId">ID do tenant (opcional, para personalização)</param>
    /// <returns>Lista de planos disponíveis</returns>
    public async Task<List<AvailablePlan>> GetAvailablePlansAsync(string? tenantId = null)
    {
        var planos = await _planoRepository.GetActiveForContractAsync();
        var planosDisponiveis = new List<AvailablePlan>();

        foreach (var plano in planos)
        {
            var planoDisponivel = new AvailablePlan
            {
                Id = plano.Id,
                Code = plano.Codigo,
                Name = plano.Nome,
                Description = plano.Descricao ?? string.Empty,
                MonthlyPriceBRL = plano.PrecoMensalBRL,
                AnnualPriceBRL = plano.PrecoAnualBRL,
                AnnualDiscountPercentage = plano.PercentualDescontoAnual,
                MaxUsers = plano.MaximoUsuarios,
                MaxProducts = plano.MaximoProdutos,
                MaxCustomers = plano.MaximoClientes,
                StorageGB = plano.EspacoArmazenamentoGB,
                AllowsMultipleBranches = plano.PermiteMultiplasFiliais,
                MaxBranches = plano.MaximoFiliais,
                BasicSupport = plano.SuporteTecnicoBasico,
                PremiumSupport = plano.SuporteTecnicoPremium,
                IncludesTraining = plano.IncluiTreinamento,
                IsPromotional = plano.PlanoPromocional,
                PromotionalValidUntil = plano.ValidadePromocional,
                DisplayOrder = plano.OrdemExibicao,
                IncludedModules = plano.ObterCodigosModulos()
            };

            planosDisponiveis.Add(planoDisponivel);
        }

        return planosDisponiveis.OrderBy(p => p.DisplayOrder).ThenBy(p => p.MonthlyPriceBRL).ToList();
    }

    /// <summary>
    /// Obtém status atual do plano de uma farmácia
    /// </summary>
    /// <param name="tenantId">ID da farmácia</param>
    /// <returns>Status do plano atual</returns>
    public async Task<CurrentPlanStatus?> GetCurrentPlanStatusAsync(string tenantId)
    {
        var contratacao = await _tenantPlanoRepository.GetWithPlanByTenantAsync(tenantId);
        if (contratacao?.Plano == null)
            return null;

        var modulosAtivos = await _moduleValidationService.GetActiveModulesAsync(tenantId);
        var limitValidation = await _moduleValidationService.ValidatePlanLimitsAsync(tenantId);

        return new CurrentPlanStatus
        {
            ContractId = contratacao.Id,
            PlanId = contratacao.PlanoId,
            PlanCode = contratacao.Plano.Codigo,
            PlanName = contratacao.Plano.Nome,
            Status = contratacao.Status,
            PaymentType = contratacao.TipoPagamento,
            ContractedValue = contratacao.ValorContratado,
            StartDate = contratacao.DataInicio,
            EndDate = contratacao.DataFim,
            NextBillingDate = contratacao.DataProximaCobranca,
            DaysUntilExpiry = contratacao.DiasParaVencer(),
            IsTrialPeriod = contratacao.PeriodoTeste,
            TrialDaysRemaining = contratacao.DiasTesteRestantes,
            AutoRenewal = contratacao.RenovacaoAutomatica,
            ActiveModules = modulosAtivos,
            IsWithinLimits = limitValidation.IsWithinLimits,
            LimitViolations = limitValidation.Violations,
            CurrentUsage = limitValidation.CurrentUsage,
            PlanLimits = limitValidation.PlanLimits
        };
    }

    /// <summary>
    /// Renova automaticamente planos que estão vencendo
    /// </summary>
    /// <param name="daysBeforeExpiry">Dias antes do vencimento para renovar</param>
    /// <returns>Resultado das renovações processadas</returns>
    public async Task<List<PlanRenewalResult>> ProcessAutoRenewalsAsync(int daysBeforeExpiry = 3)
    {
        var results = new List<PlanRenewalResult>();
        var contratacoesVencendo = await _tenantPlanoRepository.GetExpiringInDaysAsync(daysBeforeExpiry);

        foreach (var contratacao in contratacoesVencendo.Where(c => c.RenovacaoAutomatica))
        {
            try
            {
                if (contratacao.TipoPagamento == "MENSAL")
                    contratacao.Renovar(1);
                else
                    contratacao.Renovar(12);

                await _tenantPlanoRepository.UpdateAsync(contratacao);

                results.Add(new PlanRenewalResult
                {
                    TenantId = contratacao.TenantId,
                    Success = true,
                    Message = $"Plano renovado automaticamente até {contratacao.DataFim:dd/MM/yyyy}"
                });

                _logger.LogInformation("Plano renovado automaticamente para tenant {TenantId} até {EndDate}", 
                    contratacao.TenantId, contratacao.DataFim);
            }
            catch (Exception ex)
            {
                results.Add(new PlanRenewalResult
                {
                    TenantId = contratacao.TenantId,
                    Success = false,
                    Message = $"Erro na renovação automática: {ex.Message}"
                });

                _logger.LogError(ex, "Erro na renovação automática para tenant {TenantId}", contratacao.TenantId);
            }
        }

        return results;
    }

    #region Private Methods

    /// <summary>
    /// Calcula valor proporcional para upgrades/downgrades
    /// </summary>
    private decimal CalculateProportionalValue(decimal currentValue, decimal newValue, int daysRemaining, string paymentType)
    {
        if (daysRemaining <= 0) return 0;

        var daysInPeriod = paymentType == "ANUAL" ? 365 : 30;
        var dailyCurrentValue = currentValue / daysInPeriod;
        var dailyNewValue = newValue / daysInPeriod;

        var currentValueRemaining = dailyCurrentValue * daysRemaining;
        var newValueRemaining = dailyNewValue * daysRemaining;

        return newValueRemaining - currentValueRemaining;
    }

    #endregion
}

#region DTOs and Results

/// <summary>
/// Interface do serviço de gerenciamento de planos
/// </summary>
public interface IPlanManagementService
{
    Task<PlanContractResult> ContractPlanAsync(ContractPlanRequest request);
    Task<PlanUpgradeResult> UpgradePlanAsync(UpgradePlanRequest request);
    Task<PlanCancellationResult> CancelPlanAsync(CancelPlanRequest request);
    Task<List<AvailablePlan>> GetAvailablePlansAsync(string? tenantId = null);
    Task<CurrentPlanStatus?> GetCurrentPlanStatusAsync(string tenantId);
    Task<List<PlanRenewalResult>> ProcessAutoRenewalsAsync(int daysBeforeExpiry = 3);
}

/// <summary>
/// Request para contratação de plano
/// </summary>
public class ContractPlanRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string PaymentType { get; set; } = "MENSAL"; // MENSAL ou ANUAL
    public bool IsTrialPeriod { get; set; } = false;
    public string ContractedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request para upgrade de plano
/// </summary>
public class UpgradePlanRequest
{
    public string TenantId { get; set; } = string.Empty;
    public Guid NewPlanId { get; set; }
    public string NewPlanCode { get; set; } = string.Empty;
    public string UpgradedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request para cancelamento de plano
/// </summary>
public class CancelPlanRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string CancelledBy { get; set; } = string.Empty;
}

/// <summary>
/// Resultado da contratação de plano
/// </summary>
public class PlanContractResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TenantPlanoEntity? Contract { get; set; }
    public PlanoComercialEntity? Plan { get; set; }

    public static PlanContractResult Success(TenantPlanoEntity contract, PlanoComercialEntity plan)
        => new() { Success = true, Contract = contract, Plan = plan, Message = "Plano contratado com sucesso" };

    public static PlanContractResult Failure(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Resultado do upgrade de plano
/// </summary>
public class PlanUpgradeResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TenantPlanoEntity? NewContract { get; set; }
    public PlanoComercialEntity? NewPlan { get; set; }
    public decimal ProportionalValue { get; set; }

    public static PlanUpgradeResult Success(TenantPlanoEntity contract, PlanoComercialEntity plan, decimal proportionalValue)
        => new() { Success = true, NewContract = contract, NewPlan = plan, ProportionalValue = proportionalValue, Message = "Upgrade realizado com sucesso" };

    public static PlanUpgradeResult Failure(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Resultado do cancelamento de plano
/// </summary>
public class PlanCancellationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TenantPlanoEntity? CancelledContract { get; set; }

    public static PlanCancellationResult Success(TenantPlanoEntity contract)
        => new() { Success = true, CancelledContract = contract, Message = "Plano cancelado com sucesso" };

    public static PlanCancellationResult Failure(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// Plano disponível para contratação
/// </summary>
public class AvailablePlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPriceBRL { get; set; }
    public decimal AnnualPriceBRL { get; set; }
    public decimal AnnualDiscountPercentage { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProducts { get; set; }
    public int MaxCustomers { get; set; }
    public int StorageGB { get; set; }
    public bool AllowsMultipleBranches { get; set; }
    public int MaxBranches { get; set; }
    public bool BasicSupport { get; set; }
    public bool PremiumSupport { get; set; }
    public bool IncludesTraining { get; set; }
    public bool IsPromotional { get; set; }
    public DateTime? PromotionalValidUntil { get; set; }
    public int DisplayOrder { get; set; }
    public List<string> IncludedModules { get; set; } = new List<string>();
}

/// <summary>
/// Status do plano atual de uma farmácia
/// </summary>
public class CurrentPlanStatus
{
    public Guid ContractId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal ContractedValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsTrialPeriod { get; set; }
    public int TrialDaysRemaining { get; set; }
    public bool AutoRenewal { get; set; }
    public List<string> ActiveModules { get; set; } = new List<string>();
    public bool IsWithinLimits { get; set; }
    public List<PlanLimitViolation> LimitViolations { get; set; } = new List<PlanLimitViolation>();
    public Dictionary<string, int> CurrentUsage { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> PlanLimits { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Resultado de renovação automática
/// </summary>
public class PlanRenewalResult
{
    public string TenantId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion