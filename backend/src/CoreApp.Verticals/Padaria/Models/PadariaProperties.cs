namespace CoreApp.Verticals.Padaria.Models;

/// <summary>
/// Propriedades específicas da vertical Padaria para produtos
/// Contém informações comerciais específicas do segmento de panificação
/// </summary>
public class PadariaProdutoProperties
{
    /// <summary>
    /// Tipo de produto da padaria
    /// </summary>
    public TipoProdutoPadaria Tipo { get; set; } = TipoProdutoPadaria.PaoDoce;
    
    /// <summary>
    /// Tempo de validade em horas após fabricação
    /// </summary>
    public int ValidadeHoras { get; set; } = 24;
    
    /// <summary>
    /// Temperatura ideal de conservação em Celsius
    /// </summary>
    public int TemperaturaIdeal { get; set; } = 25;
    
    /// <summary>
    /// Indica se o produto precisa ser assado no dia
    /// </summary>
    public bool AssadoNoDia { get; set; } = true;
    
    /// <summary>
    /// Horário de fabricação (para produtos frescos)
    /// </summary>
    public TimeOnly? HorarioFabricacao { get; set; }
    
    /// <summary>
    /// Ingredientes principais (para controle de alergênicos)
    /// </summary>
    public List<string> IngredientesPrincipais { get; set; } = new();
    
    /// <summary>
    /// Alergênicos presentes no produto
    /// </summary>
    public List<TipoAlergenico> Alergenicos { get; set; } = new();
    
    /// <summary>
    /// Peso médio do produto em gramas
    /// </summary>
    public decimal PesoMedioGramas { get; set; }
    
    /// <summary>
    /// Rendimento por fornada (quantidade produzida por vez)
    /// </summary>
    public int RendimentoFornada { get; set; } = 1;
    
    /// <summary>
    /// Indica se pode ser congelado para conservação
    /// </summary>
    public bool PodeCongelar { get; set; } = false;
    
    /// <summary>
    /// Observações específicas do produto para os atendentes
    /// </summary>
    public string? ObservacoesAtendimento { get; set; }
}

/// <summary>
/// Tipos específicos de produtos da padaria
/// </summary>
public enum TipoProdutoPadaria
{
    PaoDoce,
    PaoSalgado,
    Bolo,
    Torta,
    Salgadinho,
    Biscoito,
    Confeitaria,
    Lanche,
    Bebida,
    Doce,
    Outros
}

/// <summary>
/// Tipos de alergênicos conforme regulamentação brasileira ANVISA
/// </summary>
public enum TipoAlergenico
{
    Gluten,
    Leite,
    Ovos,
    Soja,
    Amendoim,
    Nozes,
    Gergelim,
    Mostarda,
    Sulfitos,
    Peixe,
    Crustaceos,
    Moluscos
}

/// <summary>
/// Propriedades específicas da vertical Padaria para clientes
/// Informações comerciais do segmento de panificação
/// </summary>
public class PadariaClienteProperties
{
    /// <summary>
    /// Tipo de cliente da padaria
    /// </summary>
    public TipoClientePadaria Tipo { get; set; } = TipoClientePadaria.Varejo;
    
    /// <summary>
    /// Horários preferenciais de compra
    /// </summary>
    public List<TimeOnly> HorarioPreferencias { get; set; } = new();
    
    /// <summary>
    /// Produtos favoritos (para sugestões)
    /// </summary>
    public List<string> ProdutosFavoritos { get; set; } = new();
    
    /// <summary>
    /// Alergias ou restrições alimentares
    /// </summary>
    public List<TipoAlergenico> RestricaoAlimentar { get; set; } = new();
    
    /// <summary>
    /// Desconto especial para cliente frequente (%)
    /// </summary>
    public decimal DescontoFidelidade { get; set; } = 0;
    
    /// <summary>
    /// Data da última compra
    /// </summary>
    public DateOnly? UltimaCompra { get; set; }
    
    /// <summary>
    /// Número de compras no mês atual
    /// </summary>
    public int ComprasMesAtual { get; set; } = 0;
    
    /// <summary>
    /// Valor médio gasto por compra
    /// </summary>
    public decimal TicketMedio { get; set; } = 0;
    
    /// <summary>
    /// Aceita receber ofertas por WhatsApp/SMS
    /// </summary>
    public bool AceitaPromocoes { get; set; } = false;
    
    /// <summary>
    /// Observações especiais do cliente
    /// </summary>
    public string? ObservacoesEspeciais { get; set; }
}

/// <summary>
/// Tipos de cliente específicos da padaria
/// </summary>
public enum TipoClientePadaria
{
    Varejo,        // Cliente comum que compra para consumo próprio
    Atacado,       // Compra grandes quantidades
    Revenda,       // Outros estabelecimentos que revendem
    Empresarial,   // Empresas que compram regularmente
    EventosFestas, // Clientes para eventos e festas
    Delivery,      // Clientes que sempre pedem entrega
    Balconista     // Cliente que consome no local
}