namespace CoreApp.Domain.Configuration;

/// <summary>
/// Configurações de políticas de retenção de dados farmacêuticos brasileiros
/// Define períodos de arquivamento baseados em regulamentações ANVISA e necessidades de auditoria
/// </summary>
/// <remarks>
/// Esta configuração implementa as melhores práticas de retenção de dados
/// para estabelecimentos farmacêuticos no Brasil, garantindo compliance total
/// </remarks>
public class DataRetentionConfiguration
{
    /// <summary>
    /// Tempo em anos para manter soft deletes antes do arquivamento
    /// Padrão: 5 anos conforme regulamentação farmacêutica brasileira
    /// </summary>
    public int AnosRetencaoSoftDelete { get; set; } = 5;
    
    /// <summary>
    /// Tempo em anos para manter dados arquivados nas tabelas _log
    /// Padrão: 20 anos para compliance farmacêutico total
    /// </summary>
    public int AnosRetencaoArquivo { get; set; } = 20;
    
    /// <summary>
    /// Entidades que NUNCA devem ser arquivadas automaticamente
    /// Ex: Dados de medicamentos controlados requerem retenção perpétua
    /// </summary>
    public List<string> EntidadesProtegidas { get; set; } = new()
    {
        "MovimentacaoMedicamentoControlado",  // Lista A, B, C ANVISA - retenção perpétua
        "ReceitaMedica",                      // Receituário médico - compliance CFF
        "AuditoriaAnvisa",                    // Logs de auditoria ANVISA
        "LogCompliance",                      // Logs de compliance farmacêutico
        "ControleEspecial",                   // Controle especial de medicamentos
        "BaixaReceituario",                   // Baixas do receituário
        "NotificacaoAnvisa"                   // Notificações à ANVISA
    };
    
    /// <summary>
    /// Configurações específicas de retenção por tipo de entidade
    /// Permite personalizar período de arquivamento baseado na importância farmacêutica
    /// </summary>
    public Dictionary<string, int> RetencaoPorEntidade { get; set; } = new()
    {
        // Dados comerciais - 7 anos (requisito fiscal brasileiro)
        { "VendaEntity", 7 },
        { "ItemVendaEntity", 7 },
        { "MovimentacaoFinanceiraEntity", 7 },
        { "ContasReceberEntity", 7 },
        
        // Dados de estoque - 5 anos (padrão farmacêutico)
        { "MovimentacaoEstoqueEntity", 5 },
        { "TransferenciaEstoqueEntity", 5 },
        { "InventarioEntity", 5 },
        { "LoteEntity", 5 },
        
        // Dados de clientes - 10 anos (LGPD + relacionamento comercial)
        { "ClienteEntity", 10 },
        { "HistoricoClienteEntity", 10 },
        
        // Dados de fornecedores - 5 anos
        { "FornecedorEntity", 5 },
        { "PedidoCompraEntity", 5 },
        { "NotaFiscalEntity", 5 },
        
        // Produtos descontinuados - 3 anos
        { "ProdutoEntity", 3 },
        
        // Usuários inativos - 2 anos (LGPD)
        { "UsuarioEntity", 2 },
        
        // Logs de sistema - 1 ano
        { "LogSistemaEntity", 1 },
        { "LogAcessoEntity", 1 }
    };
    
    /// <summary>
    /// Configurações específicas para diferentes tipos de farmácias
    /// Permite ajustar retenção baseada no porte da farmácia
    /// </summary>
    public Dictionary<TipoTenantEnum, int> AjusteRetencaoPorTipoTenant { get; set; } = new()
    {
        { TipoTenantEnum.FarmaciaIndependente, 0 },     // Sem ajuste - período padrão
        { TipoTenantEnum.RedeFarmacias, 2 },            // +2 anos para redes
        { TipoTenantEnum.Hospitalar, 5 },               // +5 anos para hospitais
        { TipoTenantEnum.Manipulacao, 3 },              // +3 anos para manipulação
        { TipoTenantEnum.Veterinaria, 1 }               // +1 ano para veterinárias
    };
    
    /// <summary>
    /// Configurações de execução do processo de arquivamento
    /// </summary>
    public ArchivalExecutionConfiguration Execucao { get; set; } = new();
    
    /// <summary>
    /// Configurações de integridade e verificação
    /// </summary>
    public IntegrityCheckConfiguration VerificacaoIntegridade { get; set; } = new();
    
    /// <summary>
    /// Obtém o período de retenção para uma entidade específica
    /// Considera configurações por entidade e ajustes por tipo de tenant
    /// </summary>
    /// <param name="nomeEntidade">Nome da entidade</param>
    /// <param name="tipoTenant">Tipo do tenant (farmácia)</param>
    /// <returns>Período de retenção em anos</returns>
    public int ObterPeriodoRetencao(string nomeEntidade, TipoTenantEnum tipoTenant)
    {
        // Entidades protegidas nunca são arquivadas
        if (EntidadesProtegidas.Contains(nomeEntidade))
            return int.MaxValue;
        
        // Obtém período base para a entidade
        var periodoBase = RetencaoPorEntidade.GetValueOrDefault(nomeEntidade, AnosRetencaoSoftDelete);
        
        // Aplica ajuste baseado no tipo de tenant
        var ajuste = AjusteRetencaoPorTipoTenant.GetValueOrDefault(tipoTenant, 0);
        
        return periodoBase + ajuste;
    }
}

/// <summary>
/// Configurações específicas de execução do processo de arquivamento
/// </summary>
public class ArchivalExecutionConfiguration
{
    /// <summary>
    /// Número máximo de registros processados por lote
    /// Evita sobrecarregar o banco durante arquivamento
    /// </summary>
    public int TamanhoLote { get; set; } = 1000;
    
    /// <summary>
    /// Intervalo em milissegundos entre lotes
    /// Permite que outras operações sejam executadas
    /// </summary>
    public int IntervaloEntreLotes { get; set; } = 100;
    
    /// <summary>
    /// Timeout em minutos para operações de arquivamento
    /// </summary>
    public int TimeoutMinutos { get; set; } = 60;
    
    /// <summary>
    /// Se true, executa verificação de integridade após arquivamento
    /// </summary>
    public bool VerificarIntegridadeAposArquivamento { get; set; } = true;
    
    /// <summary>
    /// Se true, envia notificação por email ao administrador após arquivamento
    /// </summary>
    public bool NotificarAdministrador { get; set; } = true;
    
    /// <summary>
    /// Email do administrador para notificações
    /// </summary>
    public string EmailAdministrador { get; set; } = string.Empty;
}

/// <summary>
/// Configurações de verificação de integridade dos dados arquivados
/// </summary>
public class IntegrityCheckConfiguration
{
    /// <summary>
    /// Tamanho da amostra para verificação de integridade (número de registros)
    /// </summary>
    public int TamanhoAmostra { get; set; } = 1000;
    
    /// <summary>
    /// Percentual mínimo de integridade aceitável
    /// Se abaixo, dispara alertas
    /// </summary>
    public double PercentualMinimoIntegridade { get; set; } = 99.9;
    
    /// <summary>
    /// Se true, re-arquiva automaticamente registros com problemas de integridade
    /// </summary>
    public bool RearquivarDadosCorrempidos { get; set; } = true;
    
    /// <summary>
    /// Número máximo de tentativas de re-arquivamento
    /// </summary>
    public int MaximoTentativasRearquivamento { get; set; } = 3;
}

/// <summary>
/// Enumeração dos tipos de tenant (farmácias) brasileiros
/// </summary>
public enum TipoTenantEnum
{
    FarmaciaIndependente = 1,
    RedeFarmacias = 2,
    Hospitalar = 3,
    Manipulacao = 4,
    Veterinaria = 5
}