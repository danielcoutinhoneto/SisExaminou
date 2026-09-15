namespace SisExaminou.Application.Catalogo.Modelos;

public sealed record PesquisaExamesFiltro(
    string? Termo,
    int? TipoExameId,
    int? CategoriaId,
    int Pagina = 1);

public sealed record PesquisaExamesConsulta(
    string? Termo,
    int? TipoExameId,
    int? CategoriaId,
    int Pagina,
    int TamanhoPagina);

public sealed record OpcaoFiltroDto(int Id, string Nome);

public sealed record CatalogoFiltrosDto(
    IReadOnlyList<OpcaoFiltroDto> Tipos,
    IReadOnlyList<OpcaoFiltroDto> Categorias)
{
    public static CatalogoFiltrosDto Vazio { get; } = new([], []);
}

public sealed record ExameResumoDto(
    string Codigo,
    string Nome,
    string? NomePopular,
    string TipoExame,
    string Categoria,
    string? PrazoResultado);

public sealed record OrientacaoDto(
    string Titulo,
    string Tipo,
    string Conteudo,
    int OrdemExibicao);

public sealed record MaterialDto(
    string Nome,
    string? Sigla,
    string? Observacao,
    bool Principal);

public sealed record ExameDetalheDto(
    string Codigo,
    string Nome,
    string? NomePopular,
    string? DescricaoPopular,
    string? PrazoResultado,
    string TipoExame,
    string Categoria,
    IReadOnlyList<string> Sinonimos,
    IReadOnlyList<OrientacaoDto> Orientacoes,
    IReadOnlyList<MaterialDto> Materiais);

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens)
{
    public int TotalPaginas =>
        TotalItens == 0
            ? 0
            : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);

    public int PrimeiroItem =>
        Itens.Count == 0
            ? 0
            : ((Pagina - 1) * TamanhoPagina) + 1;

    public int UltimoItem =>
        Itens.Count == 0
            ? 0
            : PrimeiroItem + Itens.Count - 1;
}

public sealed record PesquisaExamesResposta(
    ResultadoPaginado<ExameResumoDto> Resultado,
    CatalogoFiltrosDto Filtros,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public bool EhValida => Erros.Count == 0;
}
