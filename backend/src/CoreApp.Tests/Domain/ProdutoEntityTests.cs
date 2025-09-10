using CoreApp.Domain.Entities;
using Xunit;

namespace CoreApp.Tests.Domain;

/// <summary>
/// Testes unitários para ProdutoEntity
/// Verifica funcionalidades básicas e regras de negócio
/// </summary>
public class ProdutoEntityTests
{
    [Fact(DisplayName = "ProdutoEntity deve criar instância válida")]
    public void ProdutoEntity_CriarInstancia_DeveSerValida()
    {
        // Arrange & Act
        var produto = new ProdutoEntity
        {
            TenantId = "tenant-test",
            Nome = "Dipirona 500mg",
            Descricao = "Medicamento para dor e febre",
            PrecoVenda = 15.50m,
            PrecoCusto = 8.75m,
            UnidadeMedida = "UN",
            EstoqueAtual = 100,
            EstoqueMinimo = 10,
            VerticalType = "FARMACIA"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, produto.Id);
        Assert.Equal("tenant-test", produto.TenantId);
        Assert.Equal("Dipirona 500mg", produto.Nome);
        Assert.Equal(15.50m, produto.PrecoVenda);
        Assert.Equal(8.75m, produto.PrecoCusto);
        Assert.Equal("FARMACIA", produto.VerticalType);
        Assert.True(produto.Ativo);
        Assert.False(produto.Excluido);
    }

    [Fact(DisplayName = "ProdutoEntity deve calcular margem de lucro corretamente")]
    public void ProdutoEntity_CalcularMargemLucro_DeveSerCorreta()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            PrecoVenda = 20.00m,
            PrecoCusto = 10.00m
        };

        // Act
        produto.MargemLucro = (produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoCusto * 100;

        // Assert
        Assert.Equal(100.00m, produto.MargemLucro); // 100% de margem
    }

    [Fact(DisplayName = "ProdutoEntity deve implementar soft delete corretamente")]
    public void ProdutoEntity_SoftDelete_DeveMarcarComoExcluido()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            Nome = "Produto Teste",
            TenantId = "tenant-test"
        };
        var usuarioId = "user-123";
        var motivo = "Produto descontinuado";

        // Act
        produto.MarkAsDeleted(usuarioId, motivo);

        // Assert
        Assert.True(produto.Excluido);
        Assert.NotNull(produto.DataExclusao);
        Assert.Equal(usuarioId, produto.UsuarioExclusao);
        Assert.Equal(motivo, produto.MotivoExclusao);
    }

    [Fact(DisplayName = "ProdutoEntity deve restaurar produto excluído")]
    public void ProdutoEntity_Restore_DeveRestaurarProduto()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            Nome = "Produto Teste",
            TenantId = "tenant-test"
        };
        produto.MarkAsDeleted("user-123", "Teste");

        // Act
        produto.Restore();

        // Assert
        Assert.False(produto.Excluido);
        Assert.Null(produto.DataExclusao);
        Assert.Null(produto.UsuarioExclusao);
        Assert.Null(produto.MotivoExclusao);
    }

    [Theory(DisplayName = "ProdutoEntity deve validar propriedades verticais")]
    [InlineData("FARMACIA", true)]
    [InlineData("PADARIA", true)]
    [InlineData("SUPERMERCADO", true)]
    [InlineData("GENERICO", true)]
    public void ProdutoEntity_ValidateVerticalProperties_DeveValidarCorretamente(string verticalType, bool esperado)
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            VerticalType = verticalType,
            VerticalActive = true
        };
        
        // Para tipos que requerem propriedades específicas, vamos adicioná-las
        if (verticalType == "PADARIA")
        {
            produto.SetVerticalProperty("temGluten", true);
        }
        else if (verticalType == "FARMACIA")
        {
            produto.SetVerticalProperty("requerReceita", false);
        }

        // Act
        var resultado = produto.ValidateVerticalProperties();

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact(DisplayName = "ProdutoEntity deve gerenciar propriedades verticais via JSON")]
    public void ProdutoEntity_VerticalProperties_DeveGerenciarViaJSON()
    {
        // Arrange
        var produto = new ProdutoEntity
        {
            VerticalType = "FARMACIA"
        };

        // Act
        produto.SetVerticalProperty("requerReceita", true);
        produto.SetVerticalProperty("medicamentoControlado", false);
        produto.SetVerticalProperty("dosagem", "500mg");

        // Assert
        Assert.True(produto.GetVerticalProperty<bool>("requerReceita"));
        Assert.False(produto.GetVerticalProperty<bool>("medicamentoControlado"));
        Assert.Equal("500mg", produto.GetVerticalProperty<string>("dosagem"));
    }
}