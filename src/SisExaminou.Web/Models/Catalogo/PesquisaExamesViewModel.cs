using System.ComponentModel.DataAnnotations;

namespace SisExaminou.Web.Models.Catalogo;

public class PesquisaExamesInputModel
{
    [Display(Name = "Termo")]
    [StringLength(
        200,
        MinimumLength = 2,
        ErrorMessage = "Informe de 2 a 200 caracteres ou deixe o termo vazio.")]
    public string? Termo { get; set; }

    [Display(Name = "Tipo de exame")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um tipo de exame válido.")]
    public int? TipoExameId { get; set; }

    [Display(Name = "Categoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
    public int? CategoriaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
    public int Pagina { get; set; } = 1;
}

public sealed class PesquisaExamesViewModel : PesquisaExamesInputModel
{
    public IReadOnlyList<OpcaoFiltroViewModel> Tipos { get; init; } = [];
    public IReadOnlyList<OpcaoFiltroViewModel> Categorias { get; init; } = [];
    public IReadOnlyList<ExameResumoViewModel> Exames { get; init; } = [];
    public int TotalPaginas { get; init; }
    public int TotalItens { get; init; }
    public int PrimeiroItem { get; init; }
    public int UltimoItem { get; init; }
    public bool TemPaginaAnterior => Pagina > 1;
    public bool TemProximaPagina => Pagina < TotalPaginas;
}

public sealed record OpcaoFiltroViewModel(int Id, string Nome);

public sealed record ExameResumoViewModel(
    string Codigo,
    string Nome,
    string? NomePopular,
    string TipoExame,
    string Categoria,
    string? PrazoResultado);
