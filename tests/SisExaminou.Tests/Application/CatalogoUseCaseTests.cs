using SisExaminou.Application.Catalogo.CasosDeUso;
using SisExaminou.Application.Catalogo.Modelos;
using SisExaminou.Tests.Doubles;

namespace SisExaminou.Tests.Application;

public sealed class CatalogoUseCaseTests
{
    [Fact]
    public async Task Pesquisa_NormalizaTermoEUsaPaginaFixa()
    {
        var repository = new FakeCatalogoConsultaRepository();
        var useCase = new PesquisarExamesUseCase(repository);

        var resposta = await useCase.ExecutarAsync(
            new PesquisaExamesFiltro("  Exame Alfa  ", 1, 1, 2),
            CancellationToken.None);

        Assert.True(resposta.EhValida);
        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal("Exame Alfa", repository.UltimaConsulta.Termo);
        Assert.Equal(10, repository.UltimaConsulta.TamanhoPagina);
        Assert.Equal(2, repository.UltimaConsulta.Pagina);
    }

    [Fact]
    public async Task Pesquisa_NaoConsultaDados_QuandoTermoTemUmCaractere()
    {
        var repository = new FakeCatalogoConsultaRepository();
        var useCase = new PesquisarExamesUseCase(repository);

        var resposta = await useCase.ExecutarAsync(
            new PesquisaExamesFiltro("A", null, null),
            CancellationToken.None);

        Assert.False(resposta.EhValida);
        Assert.Contains(nameof(PesquisaExamesFiltro.Termo), resposta.Erros.Keys);
        Assert.Equal(0, repository.QuantidadePesquisas);
    }

    [Fact]
    public async Task Detalhe_NormalizaCodigoAntesDoRepositorio()
    {
        var repository = new FakeCatalogoConsultaRepository();
        var useCase = new ObterDetalheExameUseCase(repository);

        await useCase.ExecutarAsync(" demo-alfa ", CancellationToken.None);

        Assert.Equal("DEMO-ALFA", repository.UltimoCodigo);
    }

    [Fact]
    public async Task Detalhe_NaoConsultaRepositorio_QuandoCodigoEhInvalido()
    {
        var repository = new FakeCatalogoConsultaRepository();
        var useCase = new ObterDetalheExameUseCase(repository);

        var detalhe = await useCase.ExecutarAsync(
            "codigo invalido",
            CancellationToken.None);

        Assert.Null(detalhe);
        Assert.Null(repository.UltimoCodigo);
    }
}
