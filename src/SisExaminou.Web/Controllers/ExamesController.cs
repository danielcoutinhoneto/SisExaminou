using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Excecoes;
using SisExaminou.Application.Catalogo.Modelos;
using SisExaminou.Web.Models.Catalogo;

namespace SisExaminou.Web.Controllers;

[AllowAnonymous]
[Route("exames")]
public sealed class ExamesController : Controller
{
    private readonly IPesquisarExamesUseCase _pesquisarExames;
    private readonly IObterDetalheExameUseCase _obterDetalhe;
    private readonly ILogger<ExamesController> _logger;

    public ExamesController(
        IPesquisarExamesUseCase pesquisarExames,
        IObterDetalheExameUseCase obterDetalhe,
        ILogger<ExamesController> logger)
    {
        _pesquisarExames = pesquisarExames;
        _obterDetalhe = obterDetalhe;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] PesquisaExamesInputModel filtro,
        CancellationToken cancellationToken)
    {
        try
        {
            var resposta = await _pesquisarExames.ExecutarAsync(
                new PesquisaExamesFiltro(
                    filtro.Termo,
                    filtro.TipoExameId,
                    filtro.CategoriaId,
                    filtro.Pagina),
                cancellationToken);

            AdicionarErros(resposta.Erros);
            return View(MapearPesquisa(filtro, resposta));
        }
        catch (CatalogoIndisponivelException exception)
        {
            _logger.LogError(
                exception,
                "Falha ao consultar a pesquisa pública de exames.");

            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(
                "Indisponivel",
                new CatalogoErroViewModel(
                    "A consulta de exames está temporariamente indisponível. Tente novamente em alguns instantes."));
        }
    }

    [HttpGet("{codigo}", Name = "DetalheExame")]
    public async Task<IActionResult> Detalhe(
        string codigo,
        CancellationToken cancellationToken)
    {
        try
        {
            var exame = await _obterDetalhe.ExecutarAsync(codigo, cancellationToken);
            if (exame is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("NaoEncontrado");
            }

            return View(MapearDetalhe(exame));
        }
        catch (CatalogoIndisponivelException exception)
        {
            _logger.LogError(
                exception,
                "Falha ao consultar o detalhe público de um exame.");

            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(
                "Indisponivel",
                new CatalogoErroViewModel(
                    "O detalhe do exame está temporariamente indisponível. Tente novamente em alguns instantes."));
        }
    }

    private void AdicionarErros(IReadOnlyDictionary<string, string[]> erros)
    {
        foreach (var erro in erros)
        {
            foreach (var mensagem in erro.Value)
            {
                ModelState.AddModelError(erro.Key, mensagem);
            }
        }
    }

    private static PesquisaExamesViewModel MapearPesquisa(
        PesquisaExamesInputModel filtro,
        PesquisaExamesResposta resposta)
    {
        return new PesquisaExamesViewModel
        {
            Termo = filtro.Termo,
            TipoExameId = filtro.TipoExameId,
            CategoriaId = filtro.CategoriaId,
            Pagina = resposta.Resultado.Pagina,
            Tipos = resposta.Filtros.Tipos
                .Select(item => new OpcaoFiltroViewModel(item.Id, item.Nome))
                .ToArray(),
            Categorias = resposta.Filtros.Categorias
                .Select(item => new OpcaoFiltroViewModel(item.Id, item.Nome))
                .ToArray(),
            Exames = resposta.Resultado.Itens
                .Select(item => new ExameResumoViewModel(
                    item.Codigo,
                    item.Nome,
                    item.NomePopular,
                    item.TipoExame,
                    item.Categoria,
                    item.PrazoResultado))
                .ToArray(),
            TotalPaginas = resposta.Resultado.TotalPaginas,
            TotalItens = resposta.Resultado.TotalItens,
            PrimeiroItem = resposta.Resultado.PrimeiroItem,
            UltimoItem = resposta.Resultado.UltimoItem
        };
    }

    private static ExameDetalheViewModel MapearDetalhe(ExameDetalheDto exame)
    {
        return new ExameDetalheViewModel(
            exame.Codigo,
            exame.Nome,
            exame.NomePopular,
            exame.DescricaoPopular,
            exame.PrazoResultado,
            exame.TipoExame,
            exame.Categoria,
            exame.Sinonimos,
            exame.Orientacoes
                .Select(item => new OrientacaoViewModel(
                    item.Titulo,
                    FormatarTipoOrientacao(item.Tipo),
                    item.Conteudo,
                    item.OrdemExibicao))
                .ToArray(),
            exame.Materiais
                .Select(item => new MaterialViewModel(
                    item.Nome,
                    item.Sigla,
                    item.Observacao,
                    item.Principal))
                .ToArray());
    }

    private static string FormatarTipoOrientacao(string tipo) =>
        tipo switch
        {
            "PREPARO" => "Preparo",
            "COLETA" => "Coleta",
            "RESTRICAO" => "Restrição",
            "INFORMACAO" => "Informação",
            _ => "Orientação"
        };
}
