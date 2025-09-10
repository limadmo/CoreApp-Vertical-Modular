using CoreApp.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace CoreApp.Tests.Debug;

public class TestSimpleDebug
{
    private readonly ITestOutputHelper _output;

    public TestSimpleDebug(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "Debug: Testar propriedades verticais simples")]
    public void Debug_VerticalProperties()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            VerticalType = "FARMACIA",
            VerticalActive = true
        };

        // Act
        produto.SetVerticalProperty("requerReceita", true);
        
        // Debug - Verificar o JSON gerado
        var json = produto.VerticalProperties;
        var valorRecuperado = produto.GetVerticalProperty<bool>("requerReceita");

        // Output para debug
        _output.WriteLine($"JSON gerado: {json}");
        _output.WriteLine($"Valor recuperado: {valorRecuperado}");

        // Assert  
        Assert.NotNull(json);
        Assert.Contains("requerReceita", json);
        Assert.True(valorRecuperado);
    }
    
    [Fact(DisplayName = "Debug: Testar validação PADARIA")]
    public void Debug_PadariaValidation()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            VerticalType = "PADARIA",
            VerticalActive = true
        };

        // Testar sem propriedades primeiro
        var resultadoSemPropriedades = produto.ValidateVerticalProperties();
        _output.WriteLine($"Resultado sem propriedades: {resultadoSemPropriedades}");
        
        // Adicionar propriedades necessárias
        produto.SetVerticalProperty("temGluten", true);
        
        // Debug das propriedades
        var json = produto.VerticalProperties;
        var valorRecuperado = produto.GetVerticalProperty<bool?>("temGluten");
        _output.WriteLine($"JSON após SetVerticalProperty: {json}");
        _output.WriteLine($"Valor temGluten recuperado: {valorRecuperado}");
        
        var resultadoComPropriedades = produto.ValidateVerticalProperties();
        _output.WriteLine($"Resultado com propriedades: {resultadoComPropriedades}");

        // Assert - Ajustando expectativas baseado no código real
        Assert.False(resultadoSemPropriedades); // Deve falhar sem propriedades
        Assert.True(resultadoComPropriedades);  // Deve passar com propriedades
    }

    [Fact(DisplayName = "Debug: Testar IsVerticalConfigEnabled com Boolean")]
    public void Debug_IsVerticalConfigEnabled()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            VerticalType = "FARMACIA",
            VerticalActive = true,
            VerticalConfiguration = "{\"requerReceita\":true,\"controlado\":false}"
        };

        // Act
        var requerReceita = produto.IsVerticalConfigEnabled("requerReceita");
        var controlado = produto.IsVerticalConfigEnabled("controlado");
        var inexistente = produto.IsVerticalConfigEnabled("naoExiste");

        // Debug
        _output.WriteLine($"VerticalConfiguration: {produto.VerticalConfiguration}");
        _output.WriteLine($"requerReceita: {requerReceita}");
        _output.WriteLine($"controlado: {controlado}");
        _output.WriteLine($"inexistente: {inexistente}");

        // Assert
        Assert.True(requerReceita);
        Assert.False(controlado);
        Assert.False(inexistente);
    }
}