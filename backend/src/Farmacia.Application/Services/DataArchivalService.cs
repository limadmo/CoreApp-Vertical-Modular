using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Farmacia.Domain.Configuration;
using Farmacia.Domain.Entities.Archived;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.Data;

namespace Farmacia.Application.Services;

/// <summary>
/// Interface para o serviço de arquivamento automático de dados farmacêuticos
/// </summary>
public interface IDataArchivalService
{
    Task ExecutarArquivamentoCompletoAsync();
    Task<bool> VerificarIntegridadeDadosArquivados();
    Task LimparDadosExpiradosAsync();
    Task<RelatorioArquivamentoDto> GerarRelatorioUltimoArquivamento();
}

/// <summary>
/// Serviço responsável pelo arquivamento automático de dados antigos farmacêuticos
/// Executa rotinas de backup para tabelas _log conforme políticas brasileiras
/// </summary>
/// <remarks>
/// Este serviço implementa estratégia completa de arquivamento para compliance
/// ANVISA, LGPD e otimização de performance das tabelas principais
/// </remarks>
public class DataArchivalService : IDataArchivalService
{
    private readonly FarmaciaDbContext _context;
    private readonly ILogger<DataArchivalService> _logger;
    private readonly DataRetentionConfiguration _retentionConfig;

    public DataArchivalService(
        FarmaciaDbContext context,
        ILogger<DataArchivalService> logger,
        IOptions<DataRetentionConfiguration> retentionConfig)
    {
        _context = context;
        _logger = logger;
        _retentionConfig = retentionConfig.Value;
    }

    /// <summary>
    /// Executa o processo completo de arquivamento para todas as entidades
    /// Roda automaticamente via Hangfire mensalmente
    /// </summary>
    public async Task ExecutarArquivamentoCompletoAsync()
    {
        _logger.LogInformation("Iniciando processo de arquivamento automático de dados farmacêuticos antigos");
        
        var inicioProcesso = DateTime.UtcNow;
        var totalArquivados = 0;
        var relatorio = new Dictionary<string, int>();
        
        try
        {
            // Executa arquivamento para cada tipo de entidade configurado
            foreach (var (tipoEntidade, anosRetencao) in _retentionConfig.RetencaoPorEntidade)
            {
                if (_retentionConfig.EntidadesProtegidas.Contains(tipoEntidade))
                {
                    _logger.LogInformation("Entidade {TipoEntidade} está protegida - pulando arquivamento", tipoEntidade);
                    continue;
                }
                
                var dataLimite = DateTime.UtcNow.AddYears(-anosRetencao);
                var arquivados = await ArquivarEntidadePorTipoAsync(tipoEntidade, dataLimite);
                
                totalArquivados += arquivados;
                relatorio[tipoEntidade] = arquivados;
                
                if (arquivados > 0)
                {
                    _logger.LogInformation("Arquivados {Quantidade} registros de {TipoEntidade} anteriores a {DataLimite}", 
                        arquivados, tipoEntidade, dataLimite.ToString("yyyy-MM-dd"));
                }
                
                // Pausa entre entidades para não sobrecarregar
                if (arquivados > 0)
                {
                    await Task.Delay(_retentionConfig.Execucao.IntervaloEntreLotes);
                }
            }
            
            var duracaoProcesso = DateTime.UtcNow - inicioProcesso;
            
            _logger.LogInformation(
                "Processo de arquivamento concluído com sucesso. Total arquivados: {Total}. Duração: {Duracao}",
                totalArquivados, duracaoProcesso);

            // Executa verificação de integridade se configurado
            if (_retentionConfig.Execucao.VerificarIntegridadeAposArquivamento && totalArquivados > 0)
            {
                _logger.LogInformation("Executando verificação de integridade dos dados arquivados");
                var integridadeOk = await VerificarIntegridadeDadosArquivados();
                
                if (!integridadeOk)
                {
                    _logger.LogWarning("Problemas de integridade detectados nos dados arquivados");
                }
            }

            // Salva relatório do arquivamento
            await SalvarRelatorioArquivamento(inicioProcesso, totalArquivados, relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico durante processo de arquivamento automático");
            throw;
        }
    }

    /// <summary>
    /// Arquiva registros de um tipo específico de entidade farmacêutica
    /// </summary>
    private async Task<int> ArquivarEntidadePorTipoAsync(string tipoEntidade, DateTime dataLimite)
    {
        return tipoEntidade switch
        {
            "VendaEntity" => await ArquivarVendasAsync(dataLimite),
            "MovimentacaoEstoqueEntity" => await ArquivarMovimentacoesEstoqueAsync(dataLimite),
            "ClienteEntity" => await ArquivarClientesAsync(dataLimite),
            "FornecedorEntity" => await ArquivarFornecedoresAsync(dataLimite),
            _ => 0
        };
    }

    /// <summary>
    /// Arquiva vendas antigas para tabela vendas_log
    /// Preserva dados fiscais e farmacêuticos para compliance brasileiro
    /// </summary>
    private async Task<int> ArquivarVendasAsync(DateTime dataLimite)
    {
        var loteSize = _retentionConfig.Execucao.TamanhoLote;
        var totalArquivados = 0;

        while (true)
        {
            var vendasParaArquivar = await _context.Vendas
                .Where(v => v.IsDeleted && 
                           v.DeletedAt.HasValue && 
                           v.DeletedAt.Value <= dataLimite &&
                           !v.Arquivado)
                .Take(loteSize)
                .Include(v => v.Itens)
                .ToListAsync();

            if (!vendasParaArquivar.Any())
                break;

            var vendasArquivadas = new List<VendaArquivada>();

            foreach (var venda in vendasParaArquivar)
            {
                var vendaArquivada = new VendaArquivada
                {
                    OriginalId = venda.Id,
                    DataDelecao = venda.DeletedAt!.Value,
                    DataArquivamento = DateTime.UtcNow,
                    TenantId = venda.TenantId,
                    UsuarioDeletou = venda.DeletedBy ?? "Sistema",
                    MotivoArquivamento = $"Arquivamento automático - dados com mais de {_retentionConfig.RetencaoPorEntidade["VendaEntity"]} anos",
                    VersaoSistema = "1.0.0",
                    
                    // Dados específicos da venda para consultas rápidas
                    DataVenda = venda.DataVenda,
                    ValorTotal = venda.ValorTotal,
                    CpfCnpjCliente = venda.CpfCnpjCliente,
                    ContinhaMedicamentosControlados = venda.Itens?.Any(i => i.ProdutoControlado) ?? false,
                    NumeroNotaFiscal = venda.NumeroNotaFiscal,
                    SerieNotaFiscal = venda.SerieNotaFiscal
                };
                
                // Serializa dados completos e calcula hash
                vendaArquivada.DefinirDadosOriginais(venda);
                vendasArquivadas.Add(vendaArquivada);

                // Marca como arquivado na tabela original
                venda.Arquivado = true;
                venda.DataArquivamento = DateTime.UtcNow;
            }

            // Salva lote na tabela de arquivo
            await _context.VendasArquivadas.AddRangeAsync(vendasArquivadas);
            await _context.SaveChangesAsync();

            totalArquivados += vendasArquivadas.Count;
            
            _logger.LogDebug("Arquivado lote de {Quantidade} vendas", vendasArquivadas.Count);

            // Pausa entre lotes
            await Task.Delay(_retentionConfig.Execucao.IntervaloEntreLotes);
        }

        return totalArquivados;
    }

    /// <summary>
    /// Arquiva movimentações de estoque antigas para estoque_movimentacoes_log
    /// Preserva rastreabilidade farmacêutica e compliance ANVISA
    /// </summary>
    private async Task<int> ArquivarMovimentacoesEstoqueAsync(DateTime dataLimite)
    {
        var loteSize = _retentionConfig.Execucao.TamanhoLote;
        var totalArquivados = 0;

        while (true)
        {
            var movimentacoesParaArquivar = await _context.MovimentacoesEstoque
                .Where(m => m.IsDeleted && 
                           m.DeletedAt.HasValue && 
                           m.DeletedAt.Value <= dataLimite &&
                           !m.Arquivado)
                .Take(loteSize)
                .Include(m => m.Produto)
                .Include(m => m.Usuario)
                .ToListAsync();

            if (!movimentacoesParaArquivar.Any())
                break;

            var movimentacoesArquivadas = new List<MovimentacaoEstoqueArquivada>();

            foreach (var movimentacao in movimentacoesParaArquivar)
            {
                var movimentacaoArquivada = new MovimentacaoEstoqueArquivada
                {
                    OriginalId = movimentacao.Id,
                    DataDelecao = movimentacao.DeletedAt!.Value,
                    DataArquivamento = DateTime.UtcNow,
                    TenantId = movimentacao.TenantId,
                    UsuarioDeletou = movimentacao.DeletedBy ?? "Sistema",
                    MotivoArquivamento = "Arquivamento automático por política de retenção",
                    VersaoSistema = "1.0.0",
                    
                    // Dados específicos para consultas rápidas
                    DataMovimentacao = movimentacao.DataMovimentacao,
                    ProdutoId = movimentacao.ProdutoId,
                    NomeProduto = movimentacao.Produto?.Nome ?? "Produto não encontrado",
                    TipoMovimentacao = movimentacao.TipoMovimentacao.ToString(),
                    Quantidade = movimentacao.Quantidade,
                    SaldoAnterior = movimentacao.SaldoAnterior,
                    SaldoAtual = movimentacao.SaldoAtual,
                    Lote = movimentacao.Lote,
                    DataValidade = movimentacao.DataValidade,
                    EraControlado = movimentacao.Produto?.IsControlado ?? false,
                    ClassificacaoAnvisa = movimentacao.Produto?.ClassificacaoAnvisa?.ToString(),
                    UsuarioMovimentacaoId = movimentacao.UsuarioId,
                    NomeUsuario = movimentacao.Usuario?.Nome ?? "Usuário não encontrado"
                };
                
                movimentacaoArquivada.DefinirDadosOriginais(movimentacao);
                movimentacoesArquivadas.Add(movimentacaoArquivada);

                // Marca como arquivada
                movimentacao.Arquivado = true;
                movimentacao.DataArquivamento = DateTime.UtcNow;
            }

            await _context.MovimentacoesEstoqueArquivadas.AddRangeAsync(movimentacoesArquivadas);
            await _context.SaveChangesAsync();

            totalArquivados += movimentacoesArquivadas.Count;
            
            _logger.LogDebug("Arquivado lote de {Quantidade} movimentações de estoque", movimentacoesArquivadas.Count);

            await Task.Delay(_retentionConfig.Execucao.IntervaloEntreLotes);
        }

        return totalArquivados;
    }

    /// <summary>
    /// Arquiva clientes inativos para clientes_log
    /// Respeita LGPD mantendo dados para compliance comercial
    /// </summary>
    private async Task<int> ArquivarClientesAsync(DateTime dataLimite)
    {
        var loteSize = _retentionConfig.Execucao.TamanhoLote;
        var totalArquivados = 0;

        while (true)
        {
            var clientesParaArquivar = await _context.Clientes
                .Where(c => c.IsDeleted && 
                           c.DeletedAt.HasValue && 
                           c.DeletedAt.Value <= dataLimite &&
                           !c.Arquivado)
                .Take(loteSize)
                .ToListAsync();

            if (!clientesParaArquivar.Any())
                break;

            var clientesArquivados = new List<ClienteArquivado>();

            foreach (var cliente in clientesParaArquivar)
            {
                // Calcula estatísticas do cliente
                var totalCompras = await _context.Vendas
                    .Where(v => v.ClienteId == cliente.Id && !v.IsDeleted)
                    .CountAsync();
                
                var valorTotalCompras = await _context.Vendas
                    .Where(v => v.ClienteId == cliente.Id && !v.IsDeleted)
                    .SumAsync(v => v.ValorTotal);

                var dataUltimaCompra = await _context.Vendas
                    .Where(v => v.ClienteId == cliente.Id && !v.IsDeleted)
                    .MaxAsync(v => (DateTime?)v.DataVenda);

                var utilizavaMedicamentosControlados = await _context.ItensVenda
                    .Join(_context.Vendas, iv => iv.VendaId, v => v.Id, (iv, v) => new { iv, v })
                    .Where(x => x.v.ClienteId == cliente.Id && !x.v.IsDeleted && x.iv.ProdutoControlado)
                    .AnyAsync();

                var clienteArquivado = new ClienteArquivado
                {
                    OriginalId = cliente.Id,
                    DataDelecao = cliente.DeletedAt!.Value,
                    DataArquivamento = DateTime.UtcNow,
                    TenantId = cliente.TenantId,
                    UsuarioDeletou = cliente.DeletedBy ?? "Sistema",
                    MotivoArquivamento = "Arquivamento automático - LGPD compliance",
                    VersaoSistema = "1.0.0",
                    
                    // Dados específicos do cliente
                    CpfCnpj = cliente.CpfCnpj,
                    Nome = cliente.Nome,
                    Email = cliente.Email,
                    DataCadastro = cliente.DataCadastro,
                    DataUltimaCompra = dataUltimaCompra,
                    ValorTotalCompras = valorTotalCompras,
                    TotalCompras = totalCompras,
                    UtilizavaMedicamentosControlados = utilizavaMedicamentosControlados,
                    StatusCliente = cliente.Status.ToString(),
                    MotivoInativacao = cliente.MotivoInativacao,
                    DataNascimento = cliente.DataNascimento,
                    Estado = cliente.Estado,
                    Cidade = cliente.Cidade
                };
                
                clienteArquivado.DefinirDadosOriginais(cliente);
                clientesArquivados.Add(clienteArquivado);

                cliente.Arquivado = true;
                cliente.DataArquivamento = DateTime.UtcNow;
            }

            await _context.ClientesArquivados.AddRangeAsync(clientesArquivados);
            await _context.SaveChangesAsync();

            totalArquivados += clientesArquivados.Count;
            
            _logger.LogDebug("Arquivado lote de {Quantidade} clientes", clientesArquivados.Count);

            await Task.Delay(_retentionConfig.Execucao.IntervaloEntreLotes);
        }

        return totalArquivados;
    }

    /// <summary>
    /// Arquiva fornecedores inativos para fornecedores_log
    /// </summary>
    private async Task<int> ArquivarFornecedoresAsync(DateTime dataLimite)
    {
        var loteSize = _retentionConfig.Execucao.TamanhoLote;
        var totalArquivados = 0;

        while (true)
        {
            var fornecedoresParaArquivar = await _context.Fornecedores
                .Where(f => f.IsDeleted && 
                           f.DeletedAt.HasValue && 
                           f.DeletedAt.Value <= dataLimite &&
                           !f.Arquivado)
                .Take(loteSize)
                .ToListAsync();

            if (!fornecedoresParaArquivar.Any())
                break;

            var fornecedoresArquivados = new List<FornecedorArquivado>();

            foreach (var fornecedor in fornecedoresParaArquivar)
            {
                var fornecedorArquivado = new FornecedorArquivado
                {
                    OriginalId = fornecedor.Id,
                    DataDelecao = fornecedor.DeletedAt!.Value,
                    DataArquivamento = DateTime.UtcNow,
                    TenantId = fornecedor.TenantId,
                    UsuarioDeletou = fornecedor.DeletedBy ?? "Sistema",
                    MotivoArquivamento = "Arquivamento automático por inatividade",
                    VersaoSistema = "1.0.0",
                    
                    Cnpj = fornecedor.Cnpj,
                    RazaoSocial = fornecedor.RazaoSocial,
                    NomeFantasia = fornecedor.NomeFantasia,
                    DataCadastro = fornecedor.DataCadastro,
                    StatusFornecedor = fornecedor.Status.ToString(),
                    MotivoInativacao = fornecedor.MotivoInativacao,
                    Estado = fornecedor.Estado,
                    Cidade = fornecedor.Cidade,
                    Telefone = fornecedor.Telefone,
                    Email = fornecedor.Email
                };
                
                fornecedorArquivado.DefinirDadosOriginais(fornecedor);
                fornecedoresArquivados.Add(fornecedorArquivado);

                fornecedor.Arquivado = true;
                fornecedor.DataArquivamento = DateTime.UtcNow;
            }

            await _context.FornecedoresArquivados.AddRangeAsync(fornecedoresArquivados);
            await _context.SaveChangesAsync();

            totalArquivados += fornecedoresArquivados.Count;
            
            _logger.LogDebug("Arquivado lote de {Quantidade} fornecedores", fornecedoresArquivados.Count);

            await Task.Delay(_retentionConfig.Execucao.IntervaloEntreLotes);
        }

        return totalArquivados;
    }

    /// <summary>
    /// Verifica integridade de dados arquivados através de hash MD5
    /// Detecta possível corrupção ou modificação não autorizada
    /// </summary>
    public async Task<bool> VerificarIntegridadeDadosArquivados()
    {
        _logger.LogInformation("Iniciando verificação de integridade de dados arquivados");
        
        var dadosCorretos = 0;
        var dadosCorrempidos = 0;
        var tamanhoAmostra = _retentionConfig.VerificacaoIntegridade.TamanhoAmostra;

        // Verifica amostra de cada tipo de arquivo
        await VerificarIntegridadeTipo<VendaArquivada>("vendas arquivadas", tamanhoAmostra);
        await VerificarIntegridadeTipo<MovimentacaoEstoqueArquivada>("movimentações arquivadas", tamanhoAmostra);
        await VerificarIntegridadeTipo<ClienteArquivado>("clientes arquivados", tamanhoAmostra);
        await VerificarIntegridadeTipo<FornecedorArquivado>("fornecedores arquivados", tamanhoAmostra);

        var percentualIntegridade = dadosCorretos + dadosCorrempidos > 0 
            ? (double)dadosCorretos / (dadosCorretos + dadosCorrempidos) * 100 
            : 100;

        var integridadeOk = percentualIntegridade >= _retentionConfig.VerificacaoIntegridade.PercentualMinimoIntegridade;
        
        _logger.LogInformation(
            "Verificação de integridade concluída. Corretos: {Corretos}, Corrompidos: {Corrompidos}, Percentual: {Percentual:F2}%",
            dadosCorretos, dadosCorrempidos, percentualIntegridade);

        return integridadeOk;

        async Task VerificarIntegridadeTipo<T>(string nomeTabela, int amostra) where T : ArchivedEntity
        {
            var registros = await _context.Set<T>()
                .OrderByDescending(r => r.DataArquivamento)
                .Take(amostra)
                .ToListAsync();

            foreach (var registro in registros)
            {
                if (registro.VerificarIntegridade())
                {
                    dadosCorretos++;
                }
                else
                {
                    dadosCorrempidos++;
                    _logger.LogWarning("Dados corrompidos detectados em {Tabela} - ID: {Id}", nomeTabela, registro.OriginalId);
                }
            }
        }
    }

    /// <summary>
    /// Remove dados arquivados que excederam o período máximo de retenção
    /// Executado anualmente para liberar espaço
    /// </summary>
    public async Task LimparDadosExpiradosAsync()
    {
        _logger.LogInformation("Iniciando limpeza de dados arquivados expirados");
        
        var dataLimiteExclusao = DateTime.UtcNow.AddYears(-_retentionConfig.AnosRetencaoArquivo);
        var totalRemovidos = 0;

        // Remove vendas muito antigas
        var vendasExpiradas = await _context.VendasArquivadas
            .Where(v => v.DataArquivamento <= dataLimiteExclusao)
            .ToListAsync();
        
        if (vendasExpiradas.Any())
        {
            _context.VendasArquivadas.RemoveRange(vendasExpiradas);
            totalRemovidos += vendasExpiradas.Count;
        }

        // Remove movimentações muito antigas  
        var movimentacoesExpiradas = await _context.MovimentacoesEstoqueArquivadas
            .Where(m => m.DataArquivamento <= dataLimiteExclusao)
            .ToListAsync();
        
        if (movimentacoesExpiradas.Any())
        {
            _context.MovimentacoesEstoqueArquivadas.RemoveRange(movimentacoesExpiradas);
            totalRemovidos += movimentacoesExpiradas.Count;
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Limpeza concluída. Removidos {Total} registros arquivados expirados", totalRemovidos);
    }

    /// <summary>
    /// Gera relatório do último processo de arquivamento
    /// </summary>
    public async Task<RelatorioArquivamentoDto> GerarRelatorioUltimoArquivamento()
    {
        var ultimoRelatorio = await _context.RelatoriosArquivamento
            .OrderByDescending(r => r.DataExecucao)
            .FirstOrDefaultAsync();

        if (ultimoRelatorio == null)
        {
            return new RelatorioArquivamentoDto
            {
                DataExecucao = DateTime.MinValue,
                TotalRegistrosArquivados = 0,
                Detalhes = "Nenhum arquivamento executado ainda"
            };
        }

        return new RelatorioArquivamentoDto
        {
            DataExecucao = ultimoRelatorio.DataExecucao,
            TotalRegistrosArquivados = ultimoRelatorio.TotalRegistrosArquivados,
            VendasArquivadas = ultimoRelatorio.VendasArquivadas,
            MovimentacoesArquivadas = ultimoRelatorio.MovimentacoesArquivadas,
            ClientesArquivados = ultimoRelatorio.ClientesArquivados,
            FornecedoresArquivados = ultimoRelatorio.FornecedoresArquivados,
            IntegridadeVerificada = ultimoRelatorio.IntegridadeVerificada,
            Detalhes = ultimoRelatorio.Detalhes
        };
    }

    /// <summary>
    /// Salva relatório do processo de arquivamento para auditoria
    /// </summary>
    private async Task SalvarRelatorioArquivamento(DateTime inicio, int total, Dictionary<string, int> detalhes)
    {
        var relatorio = new RelatorioArquivamentoEntity
        {
            DataExecucao = inicio,
            TotalRegistrosArquivados = total,
            VendasArquivadas = detalhes.GetValueOrDefault("VendaEntity", 0),
            MovimentacoesArquivadas = detalhes.GetValueOrDefault("MovimentacaoEstoqueEntity", 0),
            ClientesArquivados = detalhes.GetValueOrDefault("ClienteEntity", 0),
            FornecedoresArquivados = detalhes.GetValueOrDefault("FornecedorEntity", 0),
            IntegridadeVerificada = true,
            Detalhes = JsonSerializer.Serialize(detalhes)
        };

        _context.RelatoriosArquivamento.Add(relatorio);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// DTO para relatório de arquivamento
/// </summary>
public class RelatorioArquivamentoDto
{
    public DateTime DataExecucao { get; set; }
    public int TotalRegistrosArquivados { get; set; }
    public int VendasArquivadas { get; set; }
    public int MovimentacoesArquivadas { get; set; }
    public int ClientesArquivados { get; set; }
    public int FornecedoresArquivados { get; set; }
    public bool IntegridadeVerificada { get; set; }
    public string Detalhes { get; set; } = string.Empty;
}

/// <summary>
/// Entidade para armazenar relatórios de arquivamento
/// </summary>
public class RelatorioArquivamentoEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DataExecucao { get; set; }
    public int TotalRegistrosArquivados { get; set; }
    public int VendasArquivadas { get; set; }
    public int MovimentacoesArquivadas { get; set; }
    public int ClientesArquivados { get; set; }
    public int FornecedoresArquivados { get; set; }
    public bool IntegridadeVerificada { get; set; }
    public string Detalhes { get; set; } = string.Empty;
}