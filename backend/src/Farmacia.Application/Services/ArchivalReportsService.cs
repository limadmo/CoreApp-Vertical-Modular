using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Farmacia.Domain.Configuration;
using Farmacia.Infrastructure.Data;

namespace Farmacia.Application.Services;

/// <summary>
/// Interface para serviço de relatórios de arquivamento farmacêutico
/// </summary>
public interface IArchivalReportsService
{
    Task<RelatorioArquivamentoMensalDto> GerarRelatorioMensalAsync(int ano, int mes);
    Task<RelatorioArquivamentoAnualDto> GerarRelatorioAnualAsync(int ano);
    Task<DashboardArquivamentoDto> ObterDashboardArquivamento();
    Task<List<RelatorioIntegridadeDto>> VerificarIntegridadePorTenant();
    Task<RelatorioEspacoDisco> CalcularEconomiaEspacoDisco();
    Task<List<AlertaRetencaoDto>> VerificarAlertasRetencao();
}

/// <summary>
/// Serviço para gerar relatórios detalhados de arquivamento farmacêutico
/// Fornece métricas, dashboards e alertas para gestão de dados históricos
/// </summary>
/// <remarks>
/// Este serviço é essencial para compliance ANVISA, monitoramento de espaço
/// e auditoria de processos de retenção de dados farmacêuticos
/// </remarks>
public class ArchivalReportsService : IArchivalReportsService
{
    private readonly FarmaciaDbContext _context;
    private readonly ILogger<ArchivalReportsService> _logger;
    private readonly DataRetentionConfiguration _retentionConfig;

    public ArchivalReportsService(
        FarmaciaDbContext context,
        ILogger<ArchivalReportsService> logger,
        IOptions<DataRetentionConfiguration> retentionConfig)
    {
        _context = context;
        _logger = logger;
        _retentionConfig = retentionConfig.Value;
    }

    /// <summary>
    /// Gera relatório mensal detalhado de atividades de arquivamento
    /// Inclui métricas por tenant e tipo de dados farmacêuticos
    /// </summary>
    /// <param name="ano">Ano do relatório</param>
    /// <param name="mes">Mês do relatório (1-12)</param>
    /// <returns>Relatório mensal completo</returns>
    public async Task<RelatorioArquivamentoMensalDto> GerarRelatorioMensalAsync(int ano, int mes)
    {
        _logger.LogInformation("Gerando relatório mensal de arquivamento para {Mes}/{Ano}", mes, ano);
        
        var dataInicio = new DateTime(ano, mes, 1);
        var dataFim = dataInicio.AddMonths(1).AddDays(-1);

        // Métricas gerais do mês
        var vendasArquivadas = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= dataInicio && v.DataArquivamento <= dataFim)
            .CountAsync();

        var movimentacoesArquivadas = await _context.MovimentacoesEstoqueArquivadas
            .Where(m => m.DataArquivamento >= dataInicio && m.DataArquivamento <= dataFim)
            .CountAsync();

        var clientesArquivados = await _context.ClientesArquivados
            .Where(c => c.DataArquivamento >= dataInicio && c.DataArquivamento <= dataFim)
            .CountAsync();

        var fornecedoresArquivados = await _context.FornecedoresArquivados
            .Where(f => f.DataArquivamento >= dataInicio && f.DataArquivamento <= dataFim)
            .CountAsync();

        // Métricas por tenant (farmácia)
        var arquivamentoPorTenant = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= dataInicio && v.DataArquivamento <= dataFim)
            .GroupBy(v => v.TenantId)
            .Select(g => new ArquivamentoPorTenantDto
            {
                TenantId = g.Key,
                VendasArquivadas = g.Count(),
                ValorTotalArquivado = g.Sum(v => v.ValorTotal),
                MedicamentosControladosArquivados = g.Count(v => v.ContinhaMedicamentosControlados)
            })
            .OrderByDescending(x => x.VendasArquivadas)
            .ToListAsync();

        // Análise de integridade do mês
        var problemas = await IdentificarProblemasIntegridade(dataInicio, dataFim);

        var relatorio = new RelatorioArquivamentoMensalDto
        {
            Ano = ano,
            Mes = mes,
            PeriodoInicio = dataInicio,
            PeriodoFim = dataFim,
            DataGeracao = DateTime.UtcNow,
            
            // Métricas totais
            VendasArquivadas = vendasArquivadas,
            MovimentacoesArquivadas = movimentacoesArquivadas,
            ClientesArquivados = clientesArquivados,
            FornecedoresArquivados = fornecedoresArquivados,
            TotalRegistrosArquivados = vendasArquivadas + movimentacoesArquivadas + clientesArquivados + fornecedoresArquivados,
            
            // Detalhes por tenant
            ArquivamentoPorTenant = arquivamentoPorTenant,
            
            // Status de integridade
            IntegridadeVerificada = !problemas.Any(),
            ProblemasIntegridade = problemas,
            
            // Estatísticas adicionais
            EstatisticasAdicionais = await CalcularEstatisticasAdicionais(dataInicio, dataFim)
        };

        _logger.LogInformation("Relatório mensal gerado: {Total} registros arquivados em {Mes}/{Ano}", 
            relatorio.TotalRegistrosArquivados, mes, ano);

        return relatorio;
    }

    /// <summary>
    /// Gera relatório anual consolidado de arquivamento farmacêutico
    /// </summary>
    /// <param name="ano">Ano do relatório</param>
    /// <returns>Relatório anual completo</returns>
    public async Task<RelatorioArquivamentoAnualDto> GerarRelatorioAnualAsync(int ano)
    {
        _logger.LogInformation("Gerando relatório anual de arquivamento para {Ano}", ano);

        var dataInicio = new DateTime(ano, 1, 1);
        var dataFim = new DateTime(ano, 12, 31);

        // Gera relatórios mensais para todo o ano
        var relatoriosMensais = new List<RelatorioArquivamentoMensalDto>();
        for (int mes = 1; mes <= 12; mes++)
        {
            var relatorioMensal = await GerarRelatorioMensalAsync(ano, mes);
            relatoriosMensais.Add(relatorioMensal);
        }

        // Calcula totais anuais
        var totalVendas = relatoriosMensais.Sum(r => r.VendasArquivadas);
        var totalMovimentacoes = relatoriosMensais.Sum(r => r.MovimentacoesArquivadas);
        var totalClientes = relatoriosMensais.Sum(r => r.ClientesArquivados);
        var totalFornecedores = relatoriosMensais.Sum(r => r.FornecedoresArquivados);

        // Análise de tendências
        var tendenciaArquivamento = CalcularTendenciaArquivamento(relatoriosMensais);

        var relatorioAnual = new RelatorioArquivamentoAnualDto
        {
            Ano = ano,
            DataGeracao = DateTime.UtcNow,
            
            TotalVendasArquivadas = totalVendas,
            TotalMovimentacoesArquivadas = totalMovimentacoes,
            TotalClientesArquivados = totalClientes,
            TotalFornecedoresArquivados = totalFornecedores,
            TotalGeralArquivados = totalVendas + totalMovimentacoes + totalClientes + totalFornecedores,
            
            RelatoriosMensais = relatoriosMensais,
            TendenciaArquivamento = tendenciaArquivamento,
            
            // Análises específicas
            MesComMaiorArquivamento = relatoriosMensais
                .OrderByDescending(r => r.TotalRegistrosArquivados)
                .First().Mes,
                
            MediaArquivamentoMensal = relatoriosMensais.Average(r => r.TotalRegistrosArquivados),
            
            ProjecoesProximoAno = await CalcularProjecoesArquivamento(ano + 1)
        };

        return relatorioAnual;
    }

    /// <summary>
    /// Obtém dashboard em tempo real com métricas de arquivamento
    /// </summary>
    /// <returns>Dashboard com métricas atualizadas</returns>
    public async Task<DashboardArquivamentoDto> ObterDashboardArquivamento()
    {
        var agora = DateTime.UtcNow;
        var ultimosTrintaDias = agora.AddDays(-30);
        var ultimosSeteDias = agora.AddDays(-7);

        // Totais arquivados por tipo de dados
        var totalVendasArquivadas = await _context.VendasArquivadas.CountAsync();
        var totalMovimentacoesArquivadas = await _context.MovimentacoesEstoqueArquivadas.CountAsync();
        var totalClientesArquivados = await _context.ClientesArquivados.CountAsync();
        var totalFornecedoresArquivados = await _context.FornecedoresArquivados.CountAsync();

        // Atividade recente
        var arquivamentosUltimosTrintaDias = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= ultimosTrintaDias)
            .CountAsync() +
            await _context.MovimentacoesEstoqueArquivadas
            .Where(m => m.DataArquivamento >= ultimosTrintaDias)
            .CountAsync();

        var arquivamentosUltimosSeteDias = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= ultimosSeteDias)
            .CountAsync() +
            await _context.MovimentacoesEstoqueArquivadas
            .Where(m => m.DataArquivamento >= ultimosSeteDias)
            .CountAsync();

        // Status de integridade geral
        var ultimaVerificacaoIntegridade = await _context.RelatoriosArquivamento
            .OrderByDescending(r => r.DataExecucao)
            .Select(r => new { r.DataExecucao, r.IntegridadeVerificada })
            .FirstOrDefaultAsync();

        // Próximos dados elegíveis para arquivamento
        var proximosParaArquivamento = await CalcularProximosParaArquivamento();

        var dashboard = new DashboardArquivamentoDto
        {
            DataUltimaAtualizacao = agora,
            
            // Totais arquivados
            TotalVendasArquivadas = totalVendasArquivadas,
            TotalMovimentacoesArquivadas = totalMovimentacoesArquivadas,
            TotalClientesArquivados = totalClientesArquivados,
            TotalFornecedoresArquivados = totalFornecedoresArquivados,
            TotalGeralArquivados = totalVendasArquivadas + totalMovimentacoesArquivadas + 
                                   totalClientesArquivados + totalFornecedoresArquivados,
            
            // Atividade recente
            ArquivamentosUltimosTrintaDias = arquivamentosUltimosTrintaDias,
            ArquivamentosUltimosSeteDias = arquivamentosUltimosSeteDias,
            
            // Status de integridade
            UltimaVerificacaoIntegridade = ultimaVerificacaoIntegridade?.DataExecucao,
            IntegridadeOk = ultimaVerificacaoIntegridade?.IntegridadeVerificada ?? false,
            
            // Próximos arquivamentos
            ProximosParaArquivamento = proximosParaArquivamento,
            
            // Alertas
            AlertasAtivos = await VerificarAlertasRetencao()
        };

        return dashboard;
    }

    /// <summary>
    /// Verifica integridade dos dados arquivados por tenant
    /// </summary>
    /// <returns>Relatório de integridade por farmácia</returns>
    public async Task<List<RelatorioIntegridadeDto>> VerificarIntegridadePorTenant()
    {
        _logger.LogInformation("Verificando integridade de dados arquivados por tenant");
        
        var tenants = await _context.VendasArquivadas
            .Select(v => v.TenantId)
            .Distinct()
            .ToListAsync();

        var relatorizacaoIntegridade = new List<RelatorioIntegridadeDto>();

        foreach (var tenantId in tenants)
        {
            var amostrasVendas = await _context.VendasArquivadas
                .Where(v => v.TenantId == tenantId)
                .OrderByDescending(v => v.DataArquivamento)
                .Take(100) // Amostra de 100 registros por tenant
                .ToListAsync();

            var integrosVendas = amostrasVendas.Count(v => v.VerificarIntegridade());
            var corrompidasVendas = amostrasVendas.Count - integrosVendas;

            var amostrasMovimentacoes = await _context.MovimentacoesEstoqueArquivadas
                .Where(m => m.TenantId == tenantId)
                .OrderByDescending(m => m.DataArquivamento)
                .Take(100)
                .ToListAsync();

            var integrasMovimentacoes = amostrasMovimentacoes.Count(m => m.VerificarIntegridade());
            var corrompidasMovimentacoes = amostrasMovimentacoes.Count - integrasMovimentacoes;

            var totalAmostra = amostrasVendas.Count + amostrasMovimentacoes.Count;
            var totalIntegras = integrosVendas + integrasMovimentacoes;
            var percentualIntegridade = totalAmostra > 0 ? (double)totalIntegras / totalAmostra * 100 : 100;

            relatorizacaoIntegridade.Add(new RelatorioIntegridadeDto
            {
                TenantId = tenantId,
                TotalRegistrosVerificados = totalAmostra,
                RegistrosIntegros = totalIntegras,
                RegistrosCorromp idos = corrompidasVendas + corrompidasMovimentacoes,
                PercentualIntegridade = percentualIntegridade,
                StatusIntegridade = percentualIntegridade >= _retentionConfig.VerificacaoIntegridade.PercentualMinimoIntegridade 
                    ? "OK" : "ATENÇÃO",
                DataVerificacao = DateTime.UtcNow
            });
        }

        return relatorizacaoIntegridade.OrderBy(r => r.PercentualIntegridade).ToList();
    }

    /// <summary>
    /// Calcula economia de espaço em disco obtida através do arquivamento
    /// </summary>
    /// <returns>Relatório de economia de espaço</returns>
    public async Task<RelatorioEspacoDisco> CalcularEconomiaEspacoDisco()
    {
        // Estimativa de tamanho médio por tipo de registro (em KB)
        const decimal tamanhoMedioVenda = 2.5m; // JSON da venda completa
        const decimal tamanhoMedioMovimentacao = 1.2m; // Movimentação simples
        const decimal tamanhoMedioCliente = 3.0m; // Dados pessoais completos
        const decimal tamanhoMedioFornecedor = 2.8m; // Dados empresariais

        var totalVendas = await _context.VendasArquivadas.CountAsync();
        var totalMovimentacoes = await _context.MovimentacoesEstoqueArquivadas.CountAsync();
        var totalClientes = await _context.ClientesArquivados.CountAsync();
        var totalFornecedores = await _context.FornecedoresArquivados.CountAsync();

        var espacoVendas = totalVendas * tamanhoMedioVenda;
        var espacoMovimentacoes = totalMovimentacoes * tamanhoMedioMovimentacao;
        var espacoClientes = totalClientes * tamanhoMedioCliente;
        var espacoFornecedores = totalFornecedores * tamanhoMedioFornecedor;

        var espacoTotalArquivado = espacoVendas + espacoMovimentacoes + espacoClientes + espacoFornecedores;

        // Estimativa de economia (considerando que tabelas principais não têm esses registros)
        var economiaPercentual = 30; // Estimativa conservadora de 30% de economia

        return new RelatorioEspacoDisco
        {
            TotalRegistrosArquivados = totalVendas + totalMovimentacoes + totalClientes + totalFornecedores,
            EspacoTotalArquivado = espacoTotalArquivado,
            EconomiaEstimada = espacoTotalArquivado * economiaPercentual / 100,
            PercentualEconomia = economiaPercentual,
            DataCalculo = DateTime.UtcNow,
            DetalhesEspaco = new Dictionary<string, decimal>
            {
                { "Vendas", espacoVendas },
                { "Movimentacoes", espacoMovimentacoes },
                { "Clientes", espacoClientes },
                { "Fornecedores", espacoFornecedores }
            }
        };
    }

    /// <summary>
    /// Verifica alertas relacionados à política de retenção de dados
    /// </summary>
    /// <returns>Lista de alertas ativos</returns>
    public async Task<List<AlertaRetencaoDto>> VerificarAlertasRetencao()
    {
        var alertas = new List<AlertaRetencaoDto>();
        var agora = DateTime.UtcNow;

        // Verifica dados próximos do vencimento (30 dias antes)
        var dataLimiteAlerta = agora.AddDays(30);
        
        foreach (var (entidade, anos) in _retentionConfig.RetencaoPorEntidade)
        {
            if (_retentionConfig.EntidadesProtegidas.Contains(entidade))
                continue;

            var dataLimiteArquivamento = agora.AddYears(-anos);
            var proximosVencimento = await ContarProximosVencimento(entidade, dataLimiteArquivamento, dataLimiteAlerta);
            
            if (proximosVencimento > 0)
            {
                alertas.Add(new AlertaRetencaoDto
                {
                    TipoEntidade = entidade,
                    TipoAlerta = "VENCIMENTO_PROXIMO",
                    Mensagem = $"{proximosVencimento} registros de {entidade} serão arquivados nos próximos 30 dias",
                    Quantidade = proximosVencimento,
                    DataVencimento = dataLimiteAlerta,
                    Gravidade = "INFORMACAO"
                });
            }
        }

        // Verifica dados arquivados muito antigos (candidatos à exclusão)
        var dataLimiteExclusao = agora.AddYears(-_retentionConfig.AnosRetencaoArquivo);
        
        var vendasParaExclusao = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento <= dataLimiteExclusao)
            .CountAsync();
            
        if (vendasParaExclusao > 0)
        {
            alertas.Add(new AlertaRetencaoDto
            {
                TipoEntidade = "VendasArquivadas",
                TipoAlerta = "EXCLUSAO_PENDENTE",
                Mensagem = $"{vendasParaExclusao} vendas arquivadas podem ser excluídas permanentemente",
                Quantidade = vendasParaExclusao,
                DataVencimento = dataLimiteExclusao,
                Gravidade = "ATENCAO"
            });
        }

        // Verifica problemas de integridade
        var ultimaVerificacao = await _context.RelatoriosArquivamento
            .OrderByDescending(r => r.DataExecucao)
            .FirstOrDefaultAsync();

        if (ultimaVerificacao != null && !ultimaVerificacao.IntegridadeVerificada)
        {
            alertas.Add(new AlertaRetencaoDto
            {
                TipoEntidade = "INTEGRIDADE",
                TipoAlerta = "INTEGRIDADE_COMPROMETIDA", 
                Mensagem = "Problemas de integridade detectados nos dados arquivados",
                Gravidade = "ERRO",
                DataVencimento = ultimaVerificacao.DataExecucao
            });
        }

        return alertas.OrderBy(a => a.Gravidade).ToList();
    }

    #region Métodos Auxiliares

    /// <summary>
    /// Identifica problemas de integridade em um período específico
    /// </summary>
    private async Task<List<string>> IdentificarProblemasIntegridade(DateTime inicio, DateTime fim)
    {
        var problemas = new List<string>();

        // Verifica vendas com problemas
        var vendasComProblema = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= inicio && v.DataArquivamento <= fim)
            .Take(100) // Amostra
            .ToListAsync();

        var vendasCorrempidas = vendasComProblema.Count(v => !v.VerificarIntegridade());
        if (vendasCorrempidas > 0)
        {
            problemas.Add($"{vendasCorrempidas} vendas arquivadas com problemas de integridade");
        }

        return problemas;
    }

    /// <summary>
    /// Calcula estatísticas adicionais para o relatório mensal
    /// </summary>
    private async Task<Dictionary<string, object>> CalcularEstatisticasAdicionais(DateTime inicio, DateTime fim)
    {
        var stats = new Dictionary<string, object>();

        // Valor total das vendas arquivadas
        var valorTotal = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= inicio && v.DataArquivamento <= fim)
            .SumAsync(v => v.ValorTotal);

        stats["ValorTotalVendasArquivadas"] = valorTotal;

        // Medicamentos controlados arquivados
        var controladosArquivados = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento >= inicio && v.DataArquivamento <= fim && v.ContinhaMedicamentosControlados)
            .CountAsync();

        stats["MedicamentosControladosArquivados"] = controladosArquivados;

        return stats;
    }

    /// <summary>
    /// Calcula tendência de arquivamento baseada em dados históricos
    /// </summary>
    private TendenciaArquivamentoDto CalcularTendenciaArquivamento(List<RelatorioArquivamentoMensalDto> relatoriosMensais)
    {
        if (relatoriosMensais.Count < 2)
            return new TendenciaArquivamentoDto { Tendencia = "ESTAVEL", Variacao = 0 };

        var primeiraSemestre = relatoriosMensais.Take(6).Sum(r => r.TotalRegistrosArquivados);
        var segundaSemestre = relatoriosMensais.Skip(6).Sum(r => r.TotalRegistrosArquivados);

        var variacao = segundaSemestre - primeiraSemestre;
        var percentualVariacao = primeiraSemestre > 0 ? (double)variacao / primeiraSemestre * 100 : 0;

        return new TendenciaArquivamentoDto
        {
            Tendencia = Math.Abs(percentualVariacao) < 10 ? "ESTAVEL" : 
                       percentualVariacao > 0 ? "CRESCENTE" : "DECRESCENTE",
            Variacao = percentualVariacao,
            PrimeiraSemestre = primeiraSemestre,
            SegundaSemestre = segundaSemestre
        };
    }

    /// <summary>
    /// Calcula projeções de arquivamento para o próximo ano
    /// </summary>
    private async Task<ProjecaoArquivamentoDto> CalcularProjecoesArquivamento(int ano)
    {
        // Implementação simplificada - poderia usar ML.NET para previsões mais precisas
        var mediaUltimoAno = await _context.RelatoriosArquivamento
            .Where(r => r.DataExecucao.Year == ano - 1)
            .AverageAsync(r => (double?)r.TotalRegistrosArquivados) ?? 0;

        return new ProjecaoArquivamentoDto
        {
            AnoProjecao = ano,
            ProjecaoTotalArquivamentos = (int)(mediaUltimoAno * 12), // Média mensal * 12 meses
            BaseadoEm = "Média do ano anterior"
        };
    }

    /// <summary>
    /// Calcula próximos dados elegíveis para arquivamento
    /// </summary>
    private async Task<ProximosArquivamentoDto> CalcularProximosParaArquivamento()
    {
        var proximoMes = DateTime.UtcNow.AddMonths(1);
        var dataLimiteVendas = proximoMes.AddYears(-_retentionConfig.RetencaoPorEntidade.GetValueOrDefault("VendaEntity", 7));
        
        var vendasElegiveis = await _context.Vendas
            .Where(v => v.IsDeleted && v.DeletedAt <= dataLimiteVendas && !v.Arquivado)
            .CountAsync();

        return new ProximosArquivamentoDto
        {
            ProximaExecucao = proximoMes,
            VendasElegiveis = vendasElegiveis,
            TotalEstimado = vendasElegiveis // Simplificado - deveria incluir outras entidades
        };
    }

    /// <summary>
    /// Conta registros próximos do vencimento para uma entidade específica
    /// </summary>
    private async Task<int> ContarProximosVencimento(string entidade, DateTime dataLimite, DateTime dataAlerta)
    {
        return entidade switch
        {
            "VendaEntity" => await _context.Vendas
                .Where(v => v.IsDeleted && v.DeletedAt <= dataLimite && v.DeletedAt > dataAlerta && !v.Arquivado)
                .CountAsync(),
            "ClienteEntity" => await _context.Clientes
                .Where(c => c.IsDeleted && c.DeletedAt <= dataLimite && c.DeletedAt > dataAlerta && !c.Arquivado)
                .CountAsync(),
            _ => 0
        };
    }

    #endregion
}

#region DTOs de Relatórios

/// <summary>
/// DTO para relatório mensal de arquivamento farmacêutico
/// </summary>
public class RelatorioArquivamentoMensalDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public DateTime DataGeracao { get; set; }
    
    public int VendasArquivadas { get; set; }
    public int MovimentacoesArquivadas { get; set; }
    public int ClientesArquivados { get; set; }
    public int FornecedoresArquivados { get; set; }
    public int TotalRegistrosArquivados { get; set; }
    
    public List<ArquivamentoPorTenantDto> ArquivamentoPorTenant { get; set; } = new();
    public bool IntegridadeVerificada { get; set; }
    public List<string> ProblemasIntegridade { get; set; } = new();
    public Dictionary<string, object> EstatisticasAdicionais { get; set; } = new();
}

/// <summary>
/// DTO para arquivamento por tenant (farmácia)
/// </summary>
public class ArquivamentoPorTenantDto
{
    public string TenantId { get; set; } = string.Empty;
    public int VendasArquivadas { get; set; }
    public decimal ValorTotalArquivado { get; set; }
    public int MedicamentosControladosArquivados { get; set; }
}

/// <summary>
/// DTO para relatório anual de arquivamento
/// </summary>
public class RelatorioArquivamentoAnualDto
{
    public int Ano { get; set; }
    public DateTime DataGeracao { get; set; }
    
    public int TotalVendasArquivadas { get; set; }
    public int TotalMovimentacoesArquivadas { get; set; }
    public int TotalClientesArquivados { get; set; }
    public int TotalFornecedoresArquivados { get; set; }
    public int TotalGeralArquivados { get; set; }
    
    public List<RelatorioArquivamentoMensalDto> RelatoriosMensais { get; set; } = new();
    public TendenciaArquivamentoDto TendenciaArquivamento { get; set; } = new();
    
    public int MesComMaiorArquivamento { get; set; }
    public double MediaArquivamentoMensal { get; set; }
    public ProjecaoArquivamentoDto ProjecoesProximoAno { get; set; } = new();
}

/// <summary>
/// DTO para dashboard de arquivamento em tempo real
/// </summary>
public class DashboardArquivamentoDto
{
    public DateTime DataUltimaAtualizacao { get; set; }
    
    public int TotalVendasArquivadas { get; set; }
    public int TotalMovimentacoesArquivadas { get; set; }
    public int TotalClientesArquivados { get; set; }
    public int TotalFornecedoresArquivados { get; set; }
    public int TotalGeralArquivados { get; set; }
    
    public int ArquivamentosUltimosTrintaDias { get; set; }
    public int ArquivamentosUltimosSeteDias { get; set; }
    
    public DateTime? UltimaVerificacaoIntegridade { get; set; }
    public bool IntegridadeOk { get; set; }
    
    public ProximosArquivamentoDto ProximosParaArquivamento { get; set; } = new();
    public List<AlertaRetencaoDto> AlertasAtivos { get; set; } = new();
}

/// <summary>
/// DTO para relatório de integridade por tenant
/// </summary>
public class RelatorioIntegridadeDto
{
    public string TenantId { get; set; } = string.Empty;
    public int TotalRegistrosVerificados { get; set; }
    public int RegistrosIntegros { get; set; }
    public int RegistrosCorrempidos { get; set; }
    public double PercentualIntegridade { get; set; }
    public string StatusIntegridade { get; set; } = string.Empty;
    public DateTime DataVerificacao { get; set; }
}

/// <summary>
/// DTO para relatório de espaço em disco
/// </summary>
public class RelatorioEspacoDisco
{
    public int TotalRegistrosArquivados { get; set; }
    public decimal EspacoTotalArquivado { get; set; } // Em KB
    public decimal EconomiaEstimada { get; set; } // Em KB
    public int PercentualEconomia { get; set; }
    public DateTime DataCalculo { get; set; }
    public Dictionary<string, decimal> DetalhesEspaco { get; set; } = new();
}

/// <summary>
/// DTO para alertas de retenção
/// </summary>
public class AlertaRetencaoDto
{
    public string TipoEntidade { get; set; } = string.Empty;
    public string TipoAlerta { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateTime? DataVencimento { get; set; }
    public string Gravidade { get; set; } = string.Empty; // INFO, ATENÇÃO, ERRO
}

/// <summary>
/// DTO para tendência de arquivamento
/// </summary>
public class TendenciaArquivamentoDto
{
    public string Tendencia { get; set; } = string.Empty; // CRESCENTE, DECRESCENTE, ESTAVEL
    public double Variacao { get; set; }
    public int PrimeiraSemestre { get; set; }
    public int SegundaSemestre { get; set; }
}

/// <summary>
/// DTO para projeções de arquivamento
/// </summary>
public class ProjecaoArquivamentoDto
{
    public int AnoProjecao { get; set; }
    public int ProjecaoTotalArquivamentos { get; set; }
    public string BaseadoEm { get; set; } = string.Empty;
}

/// <summary>
/// DTO para próximos arquivamentos
/// </summary>
public class ProximosArquivamentoDto
{
    public DateTime ProximaExecucao { get; set; }
    public int VendasElegiveis { get; set; }
    public int TotalEstimado { get; set; }
}

#endregion