using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Farmacia.Application.Services;

namespace Farmacia.Infrastructure.Configuration;

/// <summary>
/// Configuração de jobs recorrentes para arquivamento automático de dados farmacêuticos
/// Implementa execução automatizada de tarefas de manutenção e compliance
/// </summary>
/// <remarks>
/// Esta classe configura todos os jobs automáticos para manter o sistema
/// em compliance com regulamentações brasileiras e otimizar performance
/// </remarks>
public static class RecurringJobsConfiguration
{
    /// <summary>
    /// Configura todos os jobs recorrentes de arquivamento e manutenção
    /// Deve ser chamado durante a inicialização da aplicação
    /// </summary>
    /// <param name="services">Container de serviços da aplicação</param>
    public static void ConfigurarJobsArquivamento(this IServiceCollection services)
    {
        // Job principal de arquivamento automático - executa mensalmente
        // Primeiro domingo do mês às 02:00 (horário de menor movimento)
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "arquivamento-dados-farmaceuticos-antigos",
            service => service.ExecutarArquivamentoCompletoAsync(),
            "0 2 * * 0#1", // Cron: 02:00 no primeiro domingo do mês
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"), // Brasília
                MisfireHandling = MisfireHandlingMode.Ignorable,
                Queue = "archival"
            });

        // Job de verificação de integridade dos dados arquivados
        // Todo dia 15 às 03:00 para monitoramento contínuo
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "verificacao-integridade-dados-arquivados",
            service => service.VerificarIntegridadeDadosArquivados(),
            "0 3 15 * *", // Cron: 03:00 todo dia 15
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"),
                MisfireHandling = MisfireHandlingMode.Strict,
                Queue = "integrity-check"
            });

        // Job anual de limpeza de dados muito antigos
        // 1º de janeiro às 01:00 para começar o ano com limpeza
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "limpeza-dados-arquivados-expirados",
            service => service.LimparDadosExpiradosAsync(),
            "0 1 1 1 *", // Cron: 01:00 em 1º de janeiro
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"),
                MisfireHandling = MisfireHandlingMode.Strict,
                Queue = "cleanup"
            });

        // Job semanal de relatório de status para monitoramento
        // Toda segunda-feira às 08:00 para início da semana
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "relatorio-status-arquivamento",
            service => service.GerarRelatorioUltimoArquivamento(),
            "0 8 * * 1", // Cron: 08:00 toda segunda-feira
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"),
                MisfireHandling = MisfireHandlingMode.Ignorable,
                Queue = "reports"
            });
    }

    /// <summary>
    /// Configura jobs específicos para ambiente de desenvolvimento
    /// Executa com maior frequência para testes
    /// </summary>
    /// <param name="services">Container de serviços da aplicação</param>
    public static void ConfigurarJobsDesenvolvimento(this IServiceCollection services)
    {
        // No desenvolvimento, executa arquivamento diariamente às 23:00 para testes
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "arquivamento-desenvolvimento",
            service => service.ExecutarArquivamentoCompletoAsync(),
            "0 23 * * *", // Cron: 23:00 todos os dias
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"),
                MisfireHandling = MisfireHandlingMode.Ignorable,
                Queue = "archival-dev"
            });

        // Verificação de integridade diária no desenvolvimento
        RecurringJob.AddOrUpdate<IDataArchivalService>(
            "verificacao-integridade-desenvolvimento", 
            service => service.VerificarIntegridadeDadosArquivados(),
            "0 1 * * *", // Cron: 01:00 todos os dias
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"),
                MisfireHandling = MisfireHandlingMode.Ignorable,
                Queue = "integrity-dev"
            });
    }

    /// <summary>
    /// Remove todos os jobs de arquivamento (útil para testes ou manutenção)
    /// </summary>
    public static void RemoverJobsArquivamento()
    {
        RecurringJob.RemoveIfExists("arquivamento-dados-farmaceuticos-antigos");
        RecurringJob.RemoveIfExists("verificacao-integridade-dados-arquivados");
        RecurringJob.RemoveIfExists("limpeza-dados-arquivados-expirados");
        RecurringJob.RemoveIfExists("relatorio-status-arquivamento");
        RecurringJob.RemoveIfExists("arquivamento-desenvolvimento");
        RecurringJob.RemoveIfExists("verificacao-integridade-desenvolvimento");
    }

    /// <summary>
    /// Executa job de arquivamento manualmente para teste ou necessidade específica
    /// </summary>
    /// <param name="jobName">Nome do job a ser executado</param>
    /// <param name="queue">Fila de execução (opcional)</param>
    public static void ExecutarJobManual(string jobName, string? queue = null)
    {
        switch (jobName.ToLowerInvariant())
        {
            case "arquivamento":
                BackgroundJob.Enqueue<IDataArchivalService>(
                    service => service.ExecutarArquivamentoCompletoAsync());
                break;
                
            case "integridade":
                BackgroundJob.Enqueue<IDataArchivalService>(
                    service => service.VerificarIntegridadeDadosArquivados());
                break;
                
            case "limpeza":
                BackgroundJob.Enqueue<IDataArchivalService>(
                    service => service.LimparDadosExpiradosAsync());
                break;
                
            case "relatorio":
                BackgroundJob.Enqueue<IDataArchivalService>(
                    service => service.GerarRelatorioUltimoArquivamento());
                break;
                
            default:
                throw new ArgumentException($"Job '{jobName}' não reconhecido. " +
                    "Jobs disponíveis: arquivamento, integridade, limpeza, relatorio");
        }
    }

    /// <summary>
    /// Obtém informações sobre o status de todos os jobs de arquivamento
    /// </summary>
    /// <returns>Lista com informações dos jobs configurados</returns>
    public static List<JobStatusInfo> ObterStatusJobs()
    {
        var jobs = new List<JobStatusInfo>
        {
            new()
            {
                Id = "arquivamento-dados-farmaceuticos-antigos",
                Nome = "Arquivamento Automático",
                Descricao = "Arquiva dados farmacêuticos com mais de 5 anos",
                Cronograma = "0 2 * * 0#1 (Primeiro domingo do mês às 02:00)",
                Fila = "archival",
                Ativo = true
            },
            new()
            {
                Id = "verificacao-integridade-dados-arquivados", 
                Nome = "Verificação de Integridade",
                Descricao = "Verifica integridade dos dados arquivados",
                Cronograma = "0 3 15 * * (Todo dia 15 às 03:00)",
                Fila = "integrity-check",
                Ativo = true
            },
            new()
            {
                Id = "limpeza-dados-arquivados-expirados",
                Nome = "Limpeza de Dados Expirados", 
                Descricao = "Remove dados arquivados há mais de 20 anos",
                Cronograma = "0 1 1 1 * (1º de janeiro às 01:00)",
                Fila = "cleanup",
                Ativo = true
            },
            new()
            {
                Id = "relatorio-status-arquivamento",
                Nome = "Relatório de Status",
                Descricao = "Gera relatório do último arquivamento",
                Cronograma = "0 8 * * 1 (Segunda-feira às 08:00)",
                Fila = "reports",
                Ativo = true
            }
        };

        return jobs;
    }
}

/// <summary>
/// Informações sobre o status de um job recorrente
/// </summary>
public class JobStatusInfo
{
    /// <summary>
    /// Identificador único do job
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome amigável do job
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada do que o job faz
    /// </summary>
    public string Descricao { get; set; } = string.Empty;
    
    /// <summary>
    /// Expressão cron e descrição do cronograma
    /// </summary>
    public string Cronograma { get; set; } = string.Empty;
    
    /// <summary>
    /// Fila de execução do job
    /// </summary>
    public string Fila { get; set; } = string.Empty;
    
    /// <summary>
    /// Se o job está ativo e sendo executado
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Data/hora da última execução
    /// </summary>
    public DateTime? UltimaExecucao { get; set; }
    
    /// <summary>
    /// Data/hora da próxima execução agendada
    /// </summary>
    public DateTime? ProximaExecucao { get; set; }
    
    /// <summary>
    /// Status da última execução (sucesso/erro)
    /// </summary>
    public string? StatusUltimaExecucao { get; set; }
}

/// <summary>
/// Extensões para facilitar o uso dos jobs de arquivamento
/// </summary>
public static class ArchivalJobsExtensions
{
    /// <summary>
    /// Agenda execução única de arquivamento para horário específico
    /// Útil para manutenções programadas
    /// </summary>
    /// <param name="dataHoraExecucao">Quando executar o arquivamento</param>
    /// <param name="incluirVerificacaoIntegridade">Se deve verificar integridade após</param>
    public static void AgendarArquivamentoUnico(DateTime dataHoraExecucao, bool incluirVerificacaoIntegridade = true)
    {
        // Agenda arquivamento
        BackgroundJob.Schedule<IDataArchivalService>(
            service => service.ExecutarArquivamentoCompletoAsync(),
            dataHoraExecucao);

        // Agenda verificação de integridade 30 minutos depois, se solicitado
        if (incluirVerificacaoIntegridade)
        {
            BackgroundJob.Schedule<IDataArchivalService>(
                service => service.VerificarIntegridadeDadosArquivados(),
                dataHoraExecucao.AddMinutes(30));
        }
    }

    /// <summary>
    /// Força execução imediata de arquivamento para dados críticos
    /// Deve ser usado apenas em situações de emergência ou manutenção
    /// </summary>
    public static void ForcarArquivamentoImediato()
    {
        BackgroundJob.Enqueue<IDataArchivalService>(service => service.ExecutarArquivamentoCompletoAsync());
    }
}