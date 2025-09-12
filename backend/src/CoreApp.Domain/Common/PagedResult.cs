namespace CoreApp.Domain.Common;

/// <summary>
/// Resultado paginado genérico para APIs
/// </summary>
/// <typeparam name="T">Tipo dos itens retornados</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Número da página atual (baseado em 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Quantidade de itens por página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de itens disponíveis
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Total de páginas disponíveis
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indica se é a primeira página
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;

    /// <summary>
    /// Indica se é a última página
    /// </summary>
    public bool IsLastPage => PageNumber == TotalPages;

    /// <summary>
    /// Cria um resultado paginado vazio
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Cria um resultado paginado com os dados fornecidos
    /// </summary>
    /// <param name="items">Lista de itens</param>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="total">Total de itens</param>
    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int total)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Total = total;
        TotalPages = (int)Math.Ceiling((double)total / pageSize);
    }

    /// <summary>
    /// Cria um resultado paginado a partir de uma query
    /// </summary>
    /// <param name="source">Query source</param>
    /// <param name="pageNumber">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Resultado paginado</returns>
    public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var total = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        return new PagedResult<T>(items, pageNumber, pageSize, total);
    }
}