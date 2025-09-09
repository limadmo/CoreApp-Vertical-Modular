using Farmacia.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade de configuração para tipos de movimentação de estoque
/// Permite configurações dinâmicas por tenant farmacêutico sem necessidade de deploy
/// </summary>
/// <remarks>
/// Esta entidade substitui enums rígidos, permitindo que cada farmácia configure
/// seus próprios tipos de movimentação de acordo com suas necessidades específicas.
/// Exemplos: Compra, Venda, Devolução, Perda, Transferência, Ajuste, etc.
/// </remarks>
public class TipoMovimentacaoEntity : BaseEntity, ITenantEntity, ISoftDeletableEntity
{
    /// <summary>
    /// Identificador único do tipo de movimentação
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) - isolamento automático
    /// Permite que cada farmácia tenha seus próprios tipos
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Código único do tipo de movimentação para o tenant
    /// Usado para identificação programática
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo do tipo de movimentação
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do tipo de movimentação
    /// </summary>
    [StringLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Indica se o tipo representa entrada (+) ou saída (-) de estoque
    /// 1 = Entrada, -1 = Saída, 0 = Neutro (ajustes)
    /// </summary>
    [Required]
    [Range(-1, 1, ErrorMessage = "Direção deve ser -1 (saída), 0 (neutro) ou 1 (entrada)")]
    public int Direcao { get; set; }

    /// <summary>
    /// Indica se o tipo está ativo para uso
    /// </summary>
    [Required]
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Ordem de exibição na interface
    /// </summary>
    public int Ordem { get; set; } = 0;

    /// <summary>
    /// Cor hexadecimal para exibição na interface
    /// </summary>
    [StringLength(7)]
    public string? Cor { get; set; }

    /// <summary>
    /// Ícone para exibição na interface
    /// </summary>
    [StringLength(50)]
    public string? Icone { get; set; }

    /// <summary>
    /// Indica se requer aprovação para movimentações deste tipo
    /// </summary>
    public bool RequerAprovacao { get; set; } = false;

    /// <summary>
    /// Indica se requer nota fiscal
    /// </summary>
    public bool RequerNotaFiscal { get; set; } = false;

    /// <summary>
    /// Indica se requer fornecedor (para entradas)
    /// </summary>
    public bool RequerFornecedor { get; set; } = false;

    /// <summary>
    /// Indica se requer lote específico
    /// </summary>
    public bool RequerLote { get; set; } = false;

    /// <summary>
    /// Indica se permite movimentações de medicamentos controlados
    /// </summary>
    public bool PermiteMedicamentosControlados { get; set; } = true;

    /// <summary>
    /// Indica se deve gerar logs de auditoria especiais
    /// </summary>
    public bool GeraLogAuditoria { get; set; } = false;

    /// <summary>
    /// Campos obrigatórios específicos para este tipo (JSON)
    /// Permite personalização por farmácia
    /// </summary>
    [StringLength(1000)]
    public string? CamposObrigatorios { get; set; }

    /// <summary>
    /// Regras de validação específicas (JSON)
    /// </summary>
    [StringLength(2000)]
    public string? RegrasValidacao { get; set; }

    /// <summary>
    /// Configurações adicionais específicas do tenant (JSON)
    /// </summary>
    [StringLength(2000)]
    public string? ConfiguracoesExtras { get; set; }

    // Coleções de navegação

    /// <summary>
    /// Movimentações que usam este tipo
    /// </summary>
    public ICollection<MovimentacaoEstoqueEntity> Movimentacoes { get; set; } = new List<MovimentacaoEstoqueEntity>();

    // Propriedades de soft delete
    /// <summary>
    /// Indica se o registro está marcado como deletado
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão lógica
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Motivo da exclusão
    /// </summary>
    [StringLength(500)]
    public string? DeleteReason { get; set; }

    // Propriedades de auditoria
    /// <summary>
    /// Data de criação do registro
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou o registro
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Versão do registro para controle de concorrência
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Métodos de negócio

    /// <summary>
    /// Verifica se é um tipo de entrada de estoque
    /// </summary>
    /// <returns>True se for entrada</returns>
    public bool IsEntrada()
    {
        return Direcao > 0;
    }

    /// <summary>
    /// Verifica se é um tipo de saída de estoque
    /// </summary>
    /// <returns>True se for saída</returns>
    public bool IsSaida()
    {
        return Direcao < 0;
    }

    /// <summary>
    /// Verifica se é um tipo neutro (ajustes)
    /// </summary>
    /// <returns>True se for neutro</returns>
    public bool IsNeutro()
    {
        return Direcao == 0;
    }

    /// <summary>
    /// Obtém a descrição da direção do movimento
    /// </summary>
    /// <returns>Descrição da direção</returns>
    public string ObterDescricaoDirecao()
    {
        return Direcao switch
        {
            1 => "Entrada",
            -1 => "Saída",
            0 => "Ajuste",
            _ => "Indefinido"
        };
    }

    /// <summary>
    /// Valida se uma movimentação pode usar este tipo
    /// </summary>
    /// <param name="movimentacao">Movimentação a ser validada</param>
    /// <returns>True se válida, false caso contrário</returns>
    public bool ValidarMovimentacao(MovimentacaoEstoqueEntity movimentacao)
    {
        if (!Ativo)
            return false;

        // Verifica direção
        if ((IsEntrada() && movimentacao.Quantidade <= 0) ||
            (IsSaida() && movimentacao.Quantidade >= 0))
            return false;

        // Verifica se requer fornecedor
        if (RequerFornecedor && movimentacao.FornecedorId == null)
            return false;

        // Verifica se requer nota fiscal
        if (RequerNotaFiscal && string.IsNullOrEmpty(movimentacao.NumeroNotaFiscal))
            return false;

        // Verifica se requer lote
        if (RequerLote && movimentacao.LoteId == null)
            return false;

        // Verifica medicamentos controlados
        if (!PermiteMedicamentosControlados && movimentacao.EnvolveMedicamentoControlado())
            return false;

        return true;
    }

    /// <summary>
    /// Obtém a cor padrão baseada na direção se não houver cor específica
    /// </summary>
    /// <returns>Cor em hexadecimal</returns>
    public string ObterCor()
    {
        if (!string.IsNullOrEmpty(Cor))
            return Cor;

        return Direcao switch
        {
            1 => "#4CAF50",    // Verde para entradas
            -1 => "#F44336",   // Vermelho para saídas
            0 => "#FF9800",    // Laranja para ajustes
            _ => "#9E9E9E"     // Cinza para indefinidos
        };
    }

    /// <summary>
    /// Obtém o ícone padrão baseado na direção se não houver ícone específico
    /// </summary>
    /// <returns>Nome do ícone</returns>
    public string ObterIcone()
    {
        if (!string.IsNullOrEmpty(Icone))
            return Icone;

        return Direcao switch
        {
            1 => "add_circle",      // Ícone de entrada
            -1 => "remove_circle",  // Ícone de saída
            0 => "sync_alt",        // Ícone de ajuste
            _ => "help_outline"     // Ícone para indefinidos
        };
    }

    /// <summary>
    /// Desativa o tipo de movimentação
    /// </summary>
    /// <param name="usuarioId">Usuário responsável pela desativação</param>
    /// <param name="motivo">Motivo da desativação</param>
    public void Desativar(Guid usuarioId, string? motivo = null)
    {
        Ativo = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioId;
        
        if (!string.IsNullOrEmpty(motivo))
        {
            Descricao = $"{Descricao} [Desativado: {motivo}]".Trim();
        }
    }

    /// <summary>
    /// Reativa o tipo de movimentação
    /// </summary>
    /// <param name="usuarioId">Usuário responsável pela reativação</param>
    public void Reativar(Guid usuarioId)
    {
        Ativo = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = usuarioId;
    }

    /// <summary>
    /// Cria uma cópia do tipo para outro tenant
    /// </summary>
    /// <param name="novoTenantId">ID do novo tenant</param>
    /// <param name="usuarioId">Usuário responsável pela cópia</param>
    /// <returns>Nova instância para o tenant</returns>
    public TipoMovimentacaoEntity ClonarParaTenant(string novoTenantId, Guid usuarioId)
    {
        return new TipoMovimentacaoEntity
        {
            Id = Guid.NewGuid(),
            TenantId = novoTenantId,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            Direcao = Direcao,
            Ativo = Ativo,
            Ordem = Ordem,
            Cor = Cor,
            Icone = Icone,
            RequerAprovacao = RequerAprovacao,
            RequerNotaFiscal = RequerNotaFiscal,
            RequerFornecedor = RequerFornecedor,
            RequerLote = RequerLote,
            PermiteMedicamentosControlados = PermiteMedicamentosControlados,
            GeraLogAuditoria = GeraLogAuditoria,
            CamposObrigatorios = CamposObrigatorios,
            RegrasValidacao = RegrasValidacao,
            ConfiguracoesExtras = ConfiguracoesExtras,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioId
        };
    }

    /// <summary>
    /// Obtém resumo das configurações do tipo
    /// </summary>
    /// <returns>String com resumo das configurações</returns>
    public string ObterResumoConfiguracoes()
    {
        var configs = new List<string>
        {
            ObterDescricaoDirecao()
        };

        if (RequerAprovacao) configs.Add("Requer Aprovação");
        if (RequerNotaFiscal) configs.Add("Requer NF");
        if (RequerFornecedor) configs.Add("Requer Fornecedor");
        if (RequerLote) configs.Add("Requer Lote");
        if (!PermiteMedicamentosControlados) configs.Add("Sem Controlados");
        if (GeraLogAuditoria) configs.Add("Auditoria");

        return string.Join(" | ", configs);
    }

    /// <summary>
    /// Converte a entidade para string para logs e debugging
    /// </summary>
    /// <returns>Representação em string do tipo</returns>
    public override string ToString()
    {
        return $"{Codigo}: {Nome} ({ObterDescricaoDirecao()}) - {(Ativo ? "Ativo" : "Inativo")}";
    }
}

/// <summary>
/// Classe com tipos de movimentação padrão para seed inicial
/// </summary>
public static class TiposMovimentacaoPadrao
{
    /// <summary>
    /// Obtém lista de tipos de movimentação padrão para farmácias brasileiras
    /// </summary>
    /// <param name="tenantId">ID do tenant</param>
    /// <param name="usuarioId">Usuário responsável pela criação</param>
    /// <returns>Lista de tipos padrão</returns>
    public static List<TipoMovimentacaoEntity> ObterTiposPadrao(string tenantId, Guid usuarioId)
    {
        return new List<TipoMovimentacaoEntity>
        {
            // Entradas
            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "COMPRA",
                Nome = "Compra de Mercadoria",
                Descricao = "Entrada de produtos via compra de fornecedor",
                Direcao = 1,
                Ordem = 1,
                Cor = "#4CAF50",
                Icone = "shopping_cart",
                RequerNotaFiscal = true,
                RequerFornecedor = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },
            
            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "DEVOLUCAO_CLIENTE",
                Nome = "Devolução de Cliente",
                Descricao = "Retorno de produto vendido ao cliente",
                Direcao = 1,
                Ordem = 2,
                Cor = "#2196F3",
                Icone = "keyboard_return",
                RequerLote = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "TRANSFERENCIA_ENTRADA",
                Nome = "Transferência - Entrada",
                Descricao = "Recebimento de produtos de outra unidade",
                Direcao = 1,
                Ordem = 3,
                Cor = "#9C27B0",
                Icone = "call_received",
                RequerLote = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "AJUSTE_POSITIVO",
                Nome = "Ajuste Positivo",
                Descricao = "Correção de estoque com aumento",
                Direcao = 1,
                Ordem = 4,
                Cor = "#FF9800",
                Icone = "trending_up",
                RequerAprovacao = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            // Saídas
            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "VENDA",
                Nome = "Venda ao Cliente",
                Descricao = "Saída de produto por venda",
                Direcao = -1,
                Ordem = 5,
                Cor = "#F44336",
                Icone = "point_of_sale",
                RequerLote = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "DEVOLUCAO_FORNECEDOR",
                Nome = "Devolução ao Fornecedor",
                Descricao = "Retorno de produto ao fornecedor",
                Direcao = -1,
                Ordem = 6,
                Cor = "#E91E63",
                Icone = "undo",
                RequerFornecedor = true,
                RequerNotaFiscal = true,
                RequerLote = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "TRANSFERENCIA_SAIDA",
                Nome = "Transferência - Saída",
                Descricao = "Envio de produtos para outra unidade",
                Direcao = -1,
                Ordem = 7,
                Cor = "#673AB7",
                Icone = "call_made",
                RequerLote = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "PERDA",
                Nome = "Perda/Quebra",
                Descricao = "Saída por perda, vencimento ou avaria",
                Direcao = -1,
                Ordem = 8,
                Cor = "#795548",
                Icone = "warning",
                RequerAprovacao = true,
                RequerLote = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "AJUSTE_NEGATIVO",
                Nome = "Ajuste Negativo",
                Descricao = "Correção de estoque com redução",
                Direcao = -1,
                Ordem = 9,
                Cor = "#FF5722",
                Icone = "trending_down",
                RequerAprovacao = true,
                GeraLogAuditoria = true,
                CreatedBy = usuarioId
            },

            // Especiais para medicamentos controlados
            new() {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = "VENDA_CONTROLADO",
                Nome = "Venda Medicamento Controlado",
                Descricao = "Venda específica para medicamentos controlados",
                Direcao = -1,
                Ordem = 10,
                Cor = "#B71C1C",
                Icone = "medical_services",
                RequerLote = true,
                RequerAprovacao = true,
                GeraLogAuditoria = true,
                PermiteMedicamentosControlados = true,
                CreatedBy = usuarioId
            }
        };
    }
}