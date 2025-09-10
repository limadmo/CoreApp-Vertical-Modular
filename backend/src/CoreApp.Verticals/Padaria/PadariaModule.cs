using CoreApp.Verticals.Common;
using CoreApp.Verticals.Padaria.Models;
using CoreApp.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreApp.Verticals.Padaria;

/// <summary>
/// Implementação da vertical específica para o segmento de Padaria
/// Gerencia regras de negócio específicas do comércio de panificação brasileiro
/// </summary>
/// <remarks>
/// Esta vertical adiciona funcionalidades específicas como:
/// - Controle de validade por horas
/// - Gestão de fornadas
/// - Controle de alergênicos conforme ANVISA
/// - Tipos específicos de produtos e clientes de padaria
/// - Integração com horários de fabricação
/// </remarks>
public class PadariaModule : BaseVerticalModule
{
    public override string VerticalName => "Padaria";
    public override string Version => "1.0.0";
    public override string Description => "Módulo especializado para gerenciamento de padarias e confeitarias brasileiras";

    public override IEnumerable<string> RequiredModules => new[]
    {
        "PRODUTOS",  // Gestão de produtos (pães, doces, salgados)
        "VENDAS",     // Sistema de vendas no balcão
        "ESTOQUE"      // Controle de estoque com validade por horas
    };

    public override IEnumerable<string> OptionalModules => new[]
    {
        "CLIENTES",       // CRM para clientes fiéis e encomendas
        "PROMOCOES",      // Descontos e promoções (combo café da manhã)
        "RELATORIOS_BASICOS",   // Relatórios de vendas por período do dia
        "DELIVERY",        // Sistema de entrega para encomendas
        "MOBILE"           // App para encomendas e cardápio digital
    };

    public override Dictionary<string, object> DefaultConfigurations => new()
    {
        // Configurações específicas da padaria
        { "ValidadePadraoHoras", 24 },
        { "TemperaturaAmbienteIdeal", 25 },
        { "HorarioAbertura", "06:00" },
        { "HorarioFechamento", "19:00" },
        { "HorarioFabricaoMatinal", "04:00" },
        { "HorarioFabricaoVespertina", "14:00" },
        { "AlertaValidadeHoras", 2 }, // Alerta quando faltam 2h para vencer
        { "DescontoFidelidadePadrao", 5.0 }, // 5% para clientes fiéis
        { "TicketMedioMinimo", 15.00 }, // R$ 15,00 ticket médio esperado
        { "MaxItensPromocaoCombo", 3 }, // Máximo 3 itens em combos
        
        // Configurações de conformidade ANVISA
        { "ExibirAlergenicosObrigatorio", true },
        { "ValidarTemperaturaConservacao", true },
        { "ControlarLoteFabricacao", true },
        
        // Configurações comerciais
        { "PermitirVendaVencimento", false }, // Não vender produtos vencidos
        { "DescartarAutomaticoVencidos", true },
        { "NotificarClientesPromocoes", true },
        { "IntegrarWhatsAppEncomendas", false }
    };

    public PadariaModule(
        ILogger<PadariaModule> logger,
        IModuleValidationService moduleValidationService) 
        : base(logger, moduleValidationService)
    {
    }

    protected override async Task<VerticalActivationResult> ActivateSpecificAsync(string tenantId, Dictionary<string, object> configuration)
    {
        var result = new VerticalActivationResult();

        try
        {
            // 1. Validar configurações específicas da padaria
            var validationErrors = ValidatePadariaConfiguration(configuration);
            if (validationErrors.Any())
            {
                result.Success = false;
                result.Message = "Configurações inválidas para a vertical Padaria";
                result.Errors.AddRange(validationErrors);
                return result;
            }

            // 2. Criar categorias padrão de produtos da padaria
            await CreateDefaultProductCategories(tenantId, configuration);

            // 3. Configurar tipos de clientes específicos
            await SetupPadariaCustomerTypes(tenantId, configuration);

            // 4. Configurar alertas e automações
            await ConfigurePadariaAutomations(tenantId, configuration);

            // 5. Configurar compliance ANVISA
            await SetupAnvisaCompliance(tenantId, configuration);

            result.Success = true;
            result.Message = "Vertical Padaria ativada com sucesso";
            result.ResultData.Add("CategoriasCriadas", await GetCreatedCategoriesCount(tenantId));
            result.ResultData.Add("ConfiguracoesAplicadas", configuration.Count);

            Logger.LogInformation(
                "Vertical Padaria ativada para tenant {TenantId} com {ConfigCount} configurações",
                tenantId, configuration.Count);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro na ativação específica da vertical Padaria para tenant {TenantId}",
                tenantId);

            result.Success = false;
            result.Message = "Erro interno na ativação da vertical Padaria";
            result.Errors.Add($"Exceção: {ex.Message}");
            return result;
        }
    }

    protected override async Task<VerticalActivationResult> DeactivateSpecificAsync(string tenantId)
    {
        var result = new VerticalActivationResult();

        try
        {
            // 1. Limpar configurações específicas da padaria
            await RemovePadariaConfigurations(tenantId);

            // 2. Desativar automações específicas
            await DisablePadariaAutomations(tenantId);

            // 3. Manter dados históricos mas marcar como inativo
            await MarkPadariaDataAsInactive(tenantId);

            result.Success = true;
            result.Message = "Vertical Padaria desativada com sucesso";
            result.ResultData.Add("DadosPreservados", true);
            result.ResultData.Add("ConfiguracoesRemovidas", true);

            Logger.LogInformation(
                "Vertical Padaria desativada para tenant {TenantId}",
                tenantId);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro na desativação específica da vertical Padaria para tenant {TenantId}",
                tenantId);

            result.Success = false;
            result.Message = "Erro interno na desativação da vertical Padaria";
            result.Errors.Add($"Exceção: {ex.Message}");
            return result;
        }
    }

    public override async Task<VerticalPropertiesValidationResult> ValidatePropertiesAsync(string entityType, Dictionary<string, object> properties)
    {
        var result = new VerticalPropertiesValidationResult { IsValid = true };

        try
        {
            switch (entityType.ToLower())
            {
                case "produto":
                    result = await ValidateProdutoProperties(properties);
                    break;
                
                case "cliente":
                    result = await ValidateClienteProperties(properties);
                    break;
                
                default:
                    result.Warnings.Add($"Tipo de entidade '{entityType}' não possui validações específicas da vertical Padaria");
                    break;
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro na validação de propriedades da vertical Padaria para entidade {EntityType}",
                entityType);

            return new VerticalPropertiesValidationResult
            {
                IsValid = false,
                Errors = { $"Erro interno na validação: {ex.Message}" }
            };
        }
    }

    #region Métodos Privados de Implementação

    private List<string> ValidatePadariaConfiguration(Dictionary<string, object> configuration)
    {
        var errors = new List<string>();

        // Validar horários
        if (configuration.TryGetValue("HorarioAbertura", out var abertura) && 
            !TimeOnly.TryParse(abertura.ToString(), out _))
        {
            errors.Add("HorarioAbertura deve estar no formato HH:mm");
        }

        if (configuration.TryGetValue("HorarioFechamento", out var fechamento) && 
            !TimeOnly.TryParse(fechamento.ToString(), out _))
        {
            errors.Add("HorarioFechamento deve estar no formato HH:mm");
        }

        // Validar valores numéricos
        if (configuration.TryGetValue("ValidadePadraoHoras", out var validadeObj) &&
            (!int.TryParse(validadeObj.ToString(), out var validade) || validade <= 0))
        {
            errors.Add("ValidadePadraoHoras deve ser um número inteiro positivo");
        }

        return errors;
    }

    private async Task CreateDefaultProductCategories(string tenantId, Dictionary<string, object> configuration)
    {
        // Simula criação de categorias padrão da padaria
        // Na implementação real, integraria com o serviço de produtos
        var categoriasPadrao = new[]
        {
            "Pães Doces", "Pães Salgados", "Bolos", "Tortas", 
            "Salgadinhos", "Biscoitos", "Confeitaria", "Lanches", 
            "Bebidas", "Doces Caseiros"
        };

        Logger.LogInformation(
            "Criadas {Count} categorias padrão da padaria para tenant {TenantId}",
            categoriasPadrao.Length, tenantId);

        await Task.CompletedTask;
    }

    private async Task SetupPadariaCustomerTypes(string tenantId, Dictionary<string, object> configuration)
    {
        // Simula configuração de tipos de cliente específicos da padaria
        Logger.LogInformation(
            "Configurados tipos de cliente específicos da padaria para tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task ConfigurePadariaAutomations(string tenantId, Dictionary<string, object> configuration)
    {
        // Simula configuração de automações específicas
        // - Alertas de validade
        // - Descarte automático de produtos vencidos
        // - Notificações de clientes fiéis
        Logger.LogInformation(
            "Configuradas automações específicas da padaria para tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task SetupAnvisaCompliance(string tenantId, Dictionary<string, object> configuration)
    {
        // Simula configuração de compliance ANVISA
        // - Controle de alergênicos
        // - Rastreabilidade de lotes
        // - Controle de temperatura
        Logger.LogInformation(
            "Configurado compliance ANVISA para padaria do tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task<int> GetCreatedCategoriesCount(string tenantId)
    {
        // Simula contagem de categorias criadas
        await Task.CompletedTask;
        return 10; // Número padrão de categorias da padaria
    }

    private async Task RemovePadariaConfigurations(string tenantId)
    {
        // Simula remoção de configurações específicas
        Logger.LogInformation(
            "Removidas configurações específicas da padaria para tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task DisablePadariaAutomations(string tenantId)
    {
        // Simula desativação de automações
        Logger.LogInformation(
            "Desativadas automações da padaria para tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task MarkPadariaDataAsInactive(string tenantId)
    {
        // Simula marcação de dados como inativos
        Logger.LogInformation(
            "Dados da padaria marcados como inativos para tenant {TenantId}",
            tenantId);

        await Task.CompletedTask;
    }

    private async Task<VerticalPropertiesValidationResult> ValidateProdutoProperties(Dictionary<string, object> properties)
    {
        var result = new VerticalPropertiesValidationResult { IsValid = true };

        try
        {
            // Tenta deserializar as propriedades como PadariaProdutoProperties
            var json = JsonConvert.SerializeObject(properties);
            var produtoProps = JsonConvert.DeserializeObject<PadariaProdutoProperties>(json);

            if (produtoProps != null)
            {
                // Validações específicas do produto de padaria
                if (produtoProps.ValidadeHoras <= 0)
                {
                    result.IsValid = false;
                    result.Errors.Add("ValidadeHoras deve ser maior que zero");
                }

                if (produtoProps.PesoMedioGramas <= 0)
                {
                    result.Warnings.Add("PesoMedioGramas não informado ou inválido");
                }

                if (produtoProps.RendimentoFornada <= 0)
                {
                    result.Warnings.Add("RendimentoFornada deve ser informado para controle de produção");
                }

                // Validar alergênicos obrigatórios para certos tipos
                if (produtoProps.Tipo == TipoProdutoPadaria.Bolo || produtoProps.Tipo == TipoProdutoPadaria.Torta)
                {
                    if (!produtoProps.Alergenicos.Any())
                    {
                        result.Warnings.Add("Produtos de confeitaria devem ter alergênicos informados");
                    }
                }

                result.ValidatedData = properties;
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Erro na deserialização das propriedades do produto: {ex.Message}");
        }

        await Task.CompletedTask;
        return result;
    }

    private async Task<VerticalPropertiesValidationResult> ValidateClienteProperties(Dictionary<string, object> properties)
    {
        var result = new VerticalPropertiesValidationResult { IsValid = true };

        try
        {
            // Tenta deserializar as propriedades como PadariaClienteProperties
            var json = JsonConvert.SerializeObject(properties);
            var clienteProps = JsonConvert.DeserializeObject<PadariaClienteProperties>(json);

            if (clienteProps != null)
            {
                // Validações específicas do cliente de padaria
                if (clienteProps.DescontoFidelidade < 0 || clienteProps.DescontoFidelidade > 50)
                {
                    result.IsValid = false;
                    result.Errors.Add("DescontoFidelidade deve estar entre 0 e 50%");
                }

                if (clienteProps.TicketMedio < 0)
                {
                    result.Warnings.Add("TicketMedio deve ser um valor positivo");
                }

                result.ValidatedData = properties;
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Erro na deserialização das propriedades do cliente: {ex.Message}");
        }

        await Task.CompletedTask;
        return result;
    }

    #endregion
}