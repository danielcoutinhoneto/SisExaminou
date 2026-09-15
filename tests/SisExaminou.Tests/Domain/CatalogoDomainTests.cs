using SisExaminou.Domain.Entidades;
using SisExaminou.Domain.ObjetosDeValor;

namespace SisExaminou.Tests.Domain;

public sealed class CatalogoDomainTests
{
    [Fact]
    public void CodigoExame_NormalizaCodigoValido()
    {
        var codigo = CodigoExame.Criar("  demo-alfa ");

        Assert.Equal("DEMO-ALFA", codigo.Valor);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("DEMO_ALFA")]
    [InlineData("DEMO ALFA")]
    [InlineData("DEMO/ALFA")]
    public void CodigoExame_RejeitaFormatoInvalido(string valor)
    {
        Assert.Throws<ArgumentException>(() => CodigoExame.Criar(valor));
    }

    [Fact]
    public void Exame_PodeSerPublicado_QuandoTodaRegraEstaAtendida()
    {
        var exame = CriarExame(ativo: true);
        var tipo = CriarTipo(ativo: true);
        var categoria = CriarCategoria(ativo: true);
        var orientacoes = new[] { CriarOrientacao(ativo: true) };

        Assert.True(exame.PodeSerPublicado(tipo, categoria, orientacoes));
    }

    [Fact]
    public void Exame_NaoPodeSerPublicado_SemOrientacaoAtiva()
    {
        var exame = CriarExame(ativo: true);
        var orientacoes = new[] { CriarOrientacao(ativo: false) };

        Assert.False(
            exame.PodeSerPublicado(
                CriarTipo(ativo: true),
                CriarCategoria(ativo: true),
                orientacoes));
    }

    private static Exame CriarExame(bool ativo) =>
        new(
            1,
            1,
            1,
            CodigoExame.Criar("DEMO-ALFA"),
            "Exame Alfa",
            null,
            null,
            null,
            ativo,
            DateTime.UtcNow,
            null,
            []);

    private static TipoExame CriarTipo(bool ativo) =>
        new(1, "Laboratorial", null, ativo, DateTime.UtcNow, null, []);

    private static Categoria CriarCategoria(bool ativo) =>
        new(1, "Rotina", null, ativo, DateTime.UtcNow, null, []);

    private static Orientacao CriarOrientacao(bool ativo) =>
        new(
            1,
            1,
            "Preparação",
            TipoOrientacao.Preparo,
            "Orientação demonstrativa.",
            1,
            ativo,
            DateTime.UtcNow,
            null,
            []);
}
