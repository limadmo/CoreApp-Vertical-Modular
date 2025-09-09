using Farmacia.Domain.Entities.Base;
using Farmacia.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa os planos comerciais brasileiros do sistema SAAS
/// Permite configuração dinâmica de preços sem necessidade de deploy
/// </summary>
/// <remarks>
/// Esta entidade suporta os planos: Starter (R$149,90), Professional (R$249,90), Enterprise (R$399,90)
/// Os preços podem ser alterados via interface administrativa sem interrupção do serviço
/// Cada tenant (farmácia) está associado a um plano específico que define seus módulos ativos
/// </remarks>
public class PlanoComercialEntity : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador único do tenant (farmácia) proprietário do plano
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único do plano (STARTER, PROFESSIONAL, ENTERPRISE)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome comercial do plano exibido para farmácias brasileiras
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada dos recursos inclusos no plano
    /// </summary>
    [StringLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Preço mensal em reais brasileiros (ex: 149.90)
    /// </summary>
    [Required]
    [Range(0, 99999.99)]
    public decimal PrecoMensalBRL { get; set; }

    /// <summary>
    /// Preço anual em reais brasileiros com desconto (ex: 1499.00)
    /// </summary>
    [Required]
    [Range(0, 99999.99)]
    public decimal PrecoAnualBRL { get; set; }

    /// <summary>
    /// Percentual de desconto para pagamento anual
    /// </summary>
    [Range(0, 100)]
    public decimal DescontoAnual { get; set; }

    /// <summary>
    /// Lista de códigos dos módulos inclusos no plano (JSON array)
    /// </summary>
    [Required]
    public string ModulosInclusos { get; set; } = "[]";

    /// <summary>
    /// Número máximo de usuários permitidos (-1 = ilimitado)
    /// </summary>
    [Required]
    public int LimiteUsuarios { get; set; }

    /// <summary>
    /// Número máximo de produtos cadastrados (-1 = ilimitado)
    /// </summary>
    [Required]
    public int LimiteProdutos { get; set; }

    /// <summary>
    /// Número máximo de vendas por mês (-1 = ilimitado)
    /// </summary>
    [Required]
    public int LimiteVendasMes { get; set; }

    /// <summary>
    /// Indica se o plano está ativo para contratação
    /// </summary>
    [Required]
    public bool IsAtivo { get; set; } = true;

    /// <summary>
    /// Indica se o plano é destacado na interface (plano recomendado)
    /// </summary>
    [Required]
    public bool IsDestaque { get; set; } = false;

    /// <summary>
    /// Ordem de exibição na lista de planos (menor número = primeira posição)
    /// </summary>
    [Required]
    public int OrdemExibicao { get; set; }

    /// <summary>
    /// Cor hexadecimal para representação visual do plano (#2E7D32)
    /// </summary>
    [StringLength(7)]
    public string CorHex { get; set; } = "#2E7D32";

    /// <summary>
    /// Ícone CSS para representação visual do plano
    /// </summary>
    [StringLength(50)]
    public string Icone { get; set; } = "fas fa-pills";

    /// <summary>
    /// Configurações específicas do plano em formato JSON
    /// </summary>
    public string ConfiguracoesJSON { get; set; } = "{}";

    /// <summary>
    /// Observações internas sobre o plano
    /// </summary>
    [StringLength(500)]
    public string Observacoes { get; set; } = string.Empty;

    #region Propriedades Calculadas

    /// <summary>
    /// Calcula o valor da economia anual em reais
    /// </summary>
    /// <returns>Valor economizado no pagamento anual</returns>
    public decimal CalcularEconomiaAnual()
    {
        var valorMensalAnualizado = PrecoMensalBRL * 12;
        return Math.Max(0, valorMensalAnualizado - PrecoAnualBRL);
    }

    /// <summary>
    /// Obtém a lista de códigos dos módulos inclusos no plano
    /// </summary>
    /// <returns>Lista de códigos de módulos</returns>
    public List<string> ObterModulosInclusos()
    {
        try
        {
            var modulos = System.Text.Json.JsonSerializer.Deserialize<string[]>(ModulosInclusos);
            return modulos?.ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Verifica se o plano inclui um módulo específico
    /// </summary>
    /// <param name="codigoModulo">Código do módulo a verificar</param>
    /// <returns>True se o módulo está incluso</returns>
    public bool IncluiModulo(string codigoModulo)
    {
        if (string.IsNullOrWhiteSpace(codigoModulo)) return false;

        var modulos = ObterModulosInclusos();
        return modulos.Contains("ALL") || 
               modulos.Contains(codigoModulo.ToUpperInvariant());
    }

    /// <summary>
    /// Verifica se é um plano ilimitado (Enterprise)
    /// </summary>
    /// <returns>True se é plano ilimitado</returns>
    public bool IsIlimitado()
    {
        return Codigo.Equals("ENTERPRISE", StringComparison.OrdinalIgnoreCase) ||
               LimiteUsuarios == -1 && LimiteProdutos == -1 && LimiteVendasMes == -1;
    }

    /// <summary>
    /// Obtém a descrição da economia anual formatada
    /// </summary>
    /// <returns>Descrição da economia (ex: "Economize R$ 300,00 por ano")</returns>
    public string ObterDescricaoEconomia()
    {
        var economia = CalcularEconomiaAnual();
        if (economia <= 0) return string.Empty;

        return $"Economize R$ {economia:F2} por ano";
    }

    /// <summary>
    /// Verifica se o plano atende aos limites especificados
    /// </summary>
    /// <param name="usuarios">Número de usuários necessários</param>
    /// <param name="produtos">Número de produtos necessários</param>
    /// <param name="vendasMes">Número de vendas por mês</param>
    /// <returns>True se atende aos limites</returns>
    public bool AtendeLimites(int usuarios, int produtos, int vendasMes)
    {
        if (IsIlimitado()) return true;

        var atendeuUsuarios = LimiteUsuarios == -1 || usuarios <= LimiteUsuarios;
        var atendeuProdutos = LimiteProdutos == -1 || produtos <= LimiteProdutos;
        var atendeuVendas = LimiteVendasMes == -1 || vendasMes <= LimiteVendasMes;

        return atendeuUsuarios && atendeuProdutos && atendeuVendas;
    }

    /// <summary>
    /// Aplica desconto promocional temporário no preço
    /// </summary>
    /// <param name="percentualDesconto">Percentual de desconto (0-100)</param>
    /// <param name="mensal">Se true aplica no mensal, se false no anual</param>
    /// <returns>Preço com desconto aplicado</returns>
    public decimal AplicarDesconto(decimal percentualDesconto, bool mensal = true)
    {
        if (percentualDesconto < 0 || percentualDesconto > 100) return mensal ? PrecoMensalBRL : PrecoAnualBRL;

        var preco = mensal ? PrecoMensalBRL : PrecoAnualBRL;
        var desconto = preco * (percentualDesconto / 100);
        return Math.Max(0, preco - desconto);
    }

    #endregion

    #region Métodos Estáticos - Configurações Padrão Brasileiras

    /// <summary>
    /// Obtém a configuração padrão dos planos comerciais brasileiros
    /// </summary>
    /// <returns>Lista de planos configurados para o mercado brasileiro</returns>
    public static List<PlanoComercialEntity> ObterPlansPadraobrasileiros()
    {
        return new List<PlanoComercialEntity>
        {
            // Plano Starter - Farmácias pequenas
            new PlanoComercialEntity
            {
                Id = Guid.NewGuid(),
                TenantId = "sistema", // Configuração global
                Codigo = "STARTER",
                Nome = "Starter Farmácia",
                Descricao = "Perfeito para farmácias pequenas que estão começando. Inclui gestão básica de produtos, vendas e estoque.",
                PrecoMensalBRL = 149.90m,
                PrecoAnualBRL = 1499.00m,
                DescontoAnual = 16.7m,
                ModulosInclusos = """["PRODUCTS", "SALES", "STOCK", "USERS"]""",
                LimiteUsuarios = 3,
                LimiteProdutos = 1000,
                LimiteVendasMes = 500,
                IsAtivo = true,
                IsDestaque = false,
                OrdemExibicao = 1,
                CorHex = "#4CAF50",
                Icone = "fas fa-seedling",
                ConfiguracoesJSON = """{"suporte": "email", "backup": "semanal"}""",
                Observacoes = "Plano ideal para farmácias independentes pequenas"
            },

            // Plano Professional - Farmácias médias
            new PlanoComercialEntity
            {
                Id = Guid.NewGuid(),
                TenantId = "sistema",
                Codigo = "PROFESSIONAL", 
                Nome = "Professional Farmácia",
                Descricao = "Ideal para farmácias estabelecidas. Inclui CRM, promoções e relatórios operacionais completos.",
                PrecoMensalBRL = 249.90m,
                PrecoAnualBRL = 2399.00m,
                DescontoAnual = 20.0m,
                ModulosInclusos = """["PRODUCTS", "SALES", "STOCK", "USERS", "CUSTOMERS", "PROMOTIONS", "BASIC_REPORTS", "SUPPLIERS"]""",
                LimiteUsuarios = 10,
                LimiteProdutos = 5000,
                LimiteVendasMes = 2000,
                IsAtivo = true,
                IsDestaque = true, // Plano recomendado
                OrdemExibicao = 2,
                CorHex = "#2196F3",
                Icone = "fas fa-star",
                ConfiguracoesJSON = """{"suporte": "whatsapp", "backup": "diario", "integracao_nfe": true}""",
                Observacoes = "Plano mais popular - recomendado para maioria das farmácias"
            },

            // Plano Enterprise - Redes e farmácias grandes
            new PlanoComercialEntity
            {
                Id = Guid.NewGuid(),
                TenantId = "sistema",
                Codigo = "ENTERPRISE",
                Nome = "Enterprise Farmácia",
                Descricao = "Para redes de farmácias e grandes estabelecimentos. Recursos ilimitados e compliance total ANVISA.",
                PrecoMensalBRL = 399.90m,
                PrecoAnualBRL = 3999.00m,
                DescontoAnual = 16.7m,
                ModulosInclusos = """["ALL"]""",
                LimiteUsuarios = -1, // Ilimitado
                LimiteProdutos = -1, // Ilimitado
                LimiteVendasMes = -1, // Ilimitado
                IsAtivo = true,
                IsDestaque = false,
                OrdemExibicao = 3,
                CorHex = "#9C27B0",
                Icone = "fas fa-crown",
                ConfiguracoesJSON = """{"suporte": "telefone_24h", "backup": "tempo_real", "integracao_nfe": true, "compliance_anvisa": true, "api_personalizada": true}""",
                Observacoes = "Plano premium com todos os recursos e suporte prioritário"
            }
        };
    }

    /// <summary>
    /// Obtém plano por código
    /// </summary>
    /// <param name="codigo">Código do plano</param>
    /// <returns>Plano encontrado ou null</returns>
    public static PlanoComercialEntity? ObterPlanoPorCodigo(string codigo)
    {
        return ObterPlansPadraobrasileiros()
            .FirstOrDefault(p => p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtém lista de planos ordenados por preço
    /// </summary>
    /// <param name="apenasAtivos">Se true, retorna apenas planos ativos</param>
    /// <returns>Lista ordenada de planos</returns>
    public static List<PlanoComercialEntity> ObterPlanosOrdenados(bool apenasAtivos = true)
    {
        var planos = ObterPlansPadraobrasileiros();
        
        if (apenasAtivos)
        {
            planos = planos.Where(p => p.IsAtivo).ToList();
        }

        return planos.OrderBy(p => p.OrdemExibicao)
                     .ThenBy(p => p.PrecoMensalBRL)
                     .ToList();
    }

    #endregion

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação textual da entidade</returns>
    public override string ToString()
    {
        return $"Plano {Nome} ({Codigo}) - R$ {PrecoMensalBRL:F2}/mês - {ObterModulosInclusos().Count} módulos";
    }
}