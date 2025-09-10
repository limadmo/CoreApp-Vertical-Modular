using System.Collections.Concurrent;

namespace CoreApp.Verticals.Common;

/// <summary>
/// Registry singleton para armazenar verticais registradas
/// Mantém estado global das verticais disponíveis no sistema
/// </summary>
public interface IVerticalRegistry
{
    void RegisterVertical(IVerticalModule vertical);
    void UnregisterVertical(string verticalName);
    IVerticalModule? GetVertical(string verticalName);
    IEnumerable<IVerticalModule> GetAllVerticals();
}

/// <summary>
/// Implementação thread-safe do registry de verticais
/// </summary>
public class VerticalRegistry : IVerticalRegistry
{
    private readonly ConcurrentDictionary<string, IVerticalModule> _verticals = new();

    public void RegisterVertical(IVerticalModule vertical)
    {
        if (vertical == null)
            throw new ArgumentNullException(nameof(vertical));

        if (string.IsNullOrWhiteSpace(vertical.VerticalName))
            throw new ArgumentException("Nome da vertical não pode ser vazio", nameof(vertical));

        _verticals.TryAdd(vertical.VerticalName, vertical);
    }

    public void UnregisterVertical(string verticalName)
    {
        if (string.IsNullOrWhiteSpace(verticalName))
            throw new ArgumentException("Nome da vertical não pode ser vazio", nameof(verticalName));

        _verticals.TryRemove(verticalName, out _);
    }

    public IVerticalModule? GetVertical(string verticalName)
    {
        _verticals.TryGetValue(verticalName, out var vertical);
        return vertical;
    }

    public IEnumerable<IVerticalModule> GetAllVerticals()
    {
        return _verticals.Values.ToList();
    }
}