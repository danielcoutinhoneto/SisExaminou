using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Excecoes;
using SisExaminou.Application.Catalogo.Modelos;
using SisExaminou.Tests.Doubles;

namespace SisExaminou.Tests.Web;

public sealed class ConsultaPublicaWebTests
{
    [Fact]
    public async Task Pesquisa_EhPublicaERenderizaResultados()
    {
        await using var factory = new CatalogoWebApplicationFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/exames");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains("Encontre um exame", html);
        Assert.Contains("DEMO-ALFA", html);
    }

    [Fact]
    public async Task Detalhe_EhPublicoEExibeOrientacaoEMaterial()
    {
        await using var factory = new CatalogoWebApplicationFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/exames/DEMO-ALFA");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Prepara", html);
        Assert.Contains("Material principal", html);
    }

    [Fact]
    public async Task Pesquisa_RenderizaPaginacaoEPreservaFiltros()
    {
        var repository = CatalogoWebApplicationFactory.CriarRepository();
        repository.ResultadoPesquisa = repository.ResultadoPesquisa with
        {
            TotalItens = 11
        };

        await using var factory = new CatalogoWebApplicationFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/exames?Termo=Alfa&TipoExameId=1&CategoriaId=1&Pagina=1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("1 de 2", html);
        Assert.Contains("Pagina=2", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TipoExameId=1", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CategoriaId=1", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Detalhe_Retorna404_ParaCodigoInvalido()
    {
        await using var factory = new CatalogoWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/exames/codigo_invalido");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Pesquisa_Retorna503SemDetalheTecnico_QuandoBancoFalha()
    {
        var repository = CatalogoWebApplicationFactory.CriarRepository();
        repository.Excecao = new CatalogoIndisponivelException(
            "Detalhe tecnico interno.",
            new InvalidOperationException("Servidor interno."));

        await using var factory = new CatalogoWebApplicationFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/exames");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("temporariamente indispon", html);
        Assert.DoesNotContain("Detalhe tecnico interno", html);
        Assert.DoesNotContain("Servidor interno", html);
    }

    private sealed class CatalogoWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private readonly FakeCatalogoConsultaRepository _repository;

        public CatalogoWebApplicationFactory()
            : this(CriarRepository())
        {
        }

        public CatalogoWebApplicationFactory(
            FakeCatalogoConsultaRepository repository)
        {
            _repository = repository;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICatalogoConsultaRepository>();
                services.AddSingleton<ICatalogoConsultaRepository>(_repository);
            });
        }

        public static FakeCatalogoConsultaRepository CriarRepository()
        {
            var resumo = new ExameResumoDto(
                "DEMO-ALFA",
                "[DEMO] Exame Alfa",
                "[DEMO] Alfa",
                "[DEMO] Laboratorial",
                "[DEMO] Rotina",
                "Prazo demonstrativo.");

            return new FakeCatalogoConsultaRepository
            {
                ResultadoPesquisa = new ResultadoPaginado<ExameResumoDto>(
                    [resumo],
                    1,
                    10,
                    1),
                Detalhe = new ExameDetalheDto(
                    resumo.Codigo,
                    resumo.Nome,
                    resumo.NomePopular,
                    "Descricao demonstrativa.",
                    resumo.PrazoResultado,
                    resumo.TipoExame,
                    resumo.Categoria,
                    ["Alfa alternativo"],
                    [new OrientacaoDto("Preparacao", "PREPARO", "Conteudo demonstrativo.", 1)],
                    [new MaterialDto("Amostra Alfa", "ALFA", null, true)])
            };
        }
    }
}
