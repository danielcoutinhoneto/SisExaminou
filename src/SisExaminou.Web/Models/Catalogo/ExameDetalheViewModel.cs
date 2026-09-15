namespace SisExaminou.Web.Models.Catalogo;

public sealed record ExameDetalheViewModel(
    string Codigo,
    string Nome,
    string? NomePopular,
    string? DescricaoPopular,
    string? PrazoResultado,
    string TipoExame,
    string Categoria,
    IReadOnlyList<string> Sinonimos,
    IReadOnlyList<OrientacaoViewModel> Orientacoes,
    IReadOnlyList<MaterialViewModel> Materiais);

public sealed record OrientacaoViewModel(
    string Titulo,
    string Tipo,
    string Conteudo,
    int OrdemExibicao);

public sealed record MaterialViewModel(
    string Nome,
    string? Sigla,
    string? Observacao,
    bool Principal);

public sealed record CatalogoErroViewModel(string Mensagem);
