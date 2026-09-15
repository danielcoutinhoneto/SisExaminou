using SisExaminou.Application.Catalogo.Modelos;
using SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

namespace SisExaminou.Tests.Integration;

internal sealed class SqlServerIntegrationFactAttribute : FactAttribute
{
    public SqlServerIntegrationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(
                "SISEXAMINOU_TEST_CONNECTION_STRING")))
        {
            Skip = "Defina SISEXAMINOU_TEST_CONNECTION_STRING para executar.";
        }
    }
}

public sealed class CatalogoConsultaRepositoryIntegrationTests
{
    private const string VariavelConnectionString =
        "SISEXAMINOU_TEST_CONNECTION_STRING";

    [SqlServerIntegrationFact]
    public async Task PesquisaEFiltros_LeemCatalogoDemonstrativoDoSqlServer()
    {
        var repository = CriarRepositoryOuPular();

        var filtros = await repository.ListarFiltrosAsync(CancellationToken.None);
        var tipoLaboratorial = filtros.Tipos.Single(
            item => item.Nome == "[DEMO] Laboratorial").Id;
        var categoriaRotina = filtros.Categorias.Single(
            item => item.Nome == "[DEMO] Rotina").Id;
        var filtrosCombinados = await repository.PesquisarAsync(
            new PesquisaExamesConsulta(
                null,
                tipoLaboratorial,
                categoriaRotina,
                1,
                10),
            CancellationToken.None);
        var primeiraPagina = await repository.PesquisarAsync(
            new PesquisaExamesConsulta(null, null, null, 1, 2),
            CancellationToken.None);
        var segundaPagina = await repository.PesquisarAsync(
            new PesquisaExamesConsulta(null, null, null, 2, 2),
            CancellationToken.None);
        var codigoExato = await repository.PesquisarAsync(
            new PesquisaExamesConsulta("DEMO-ALFA", null, null, 1, 10),
            CancellationToken.None);
        var prefixoComCuringaEscapado = await repository.PesquisarAsync(
            new PesquisaExamesConsulta("[DEMO]", null, null, 1, 10),
            CancellationToken.None);

        Assert.Equal(2, filtros.Tipos.Count);
        Assert.Equal(2, filtros.Categorias.Count);
        Assert.Equal(2, filtrosCombinados.TotalItens);
        Assert.Equal(6, primeiraPagina.TotalItens);
        Assert.Equal(2, primeiraPagina.Itens.Count);
        Assert.Equal(2, segundaPagina.Itens.Count);
        Assert.Empty(
            primeiraPagina.Itens.Select(item => item.Codigo)
                .Intersect(segundaPagina.Itens.Select(item => item.Codigo)));
        Assert.Single(codigoExato.Itens);
        Assert.Equal("DEMO-ALFA", codigoExato.Itens[0].Codigo);
        Assert.Equal(6, prefixoComCuringaEscapado.TotalItens);
    }

    [SqlServerIntegrationFact]
    public async Task Detalhe_LeOrientacoesEMateriaisNaOrdemEsperada()
    {
        var repository = CriarRepositoryOuPular();

        var detalhe = await repository.ObterDetalheAsync(
            "DEMO-ALFA",
            CancellationToken.None);

        Assert.NotNull(detalhe);
        Assert.Equal("DEMO-ALFA", detalhe.Codigo);
        Assert.Equal(
            [1, 2],
            detalhe.Orientacoes.Select(item => item.OrdemExibicao));
        Assert.NotEmpty(detalhe.Materiais);
        Assert.True(detalhe.Materiais[0].Principal);
    }

    private static CatalogoConsultaRepository CriarRepositoryOuPular()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            VariavelConnectionString);

        return new CatalogoConsultaRepository(connectionString!);
    }
}
