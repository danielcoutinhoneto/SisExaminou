using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Application.Catalogo.CasosDeUso;

public sealed class PesquisarExamesUseCase : IPesquisarExamesUseCase
{
    public const int TamanhoPagina = 10;

    private readonly ICatalogoConsultaRepository _repository;

    public PesquisarExamesUseCase(ICatalogoConsultaRepository repository)
    {
        _repository = repository;
    }

    public async Task<PesquisaExamesResposta> ExecutarAsync(
        PesquisaExamesFiltro filtro,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var termo = string.IsNullOrWhiteSpace(filtro.Termo)
            ? null
            : filtro.Termo.Trim();

        var erros = Validar(filtro, termo);
        var filtros = await _repository.ListarFiltrosAsync(cancellationToken);

        if (erros.Count > 0)
        {
            return new PesquisaExamesResposta(
                new ResultadoPaginado<ExameResumoDto>(
                    [],
                    Math.Max(filtro.Pagina, 1),
                    TamanhoPagina,
                    0),
                filtros,
                erros);
        }

        var consulta = new PesquisaExamesConsulta(
            termo,
            filtro.TipoExameId,
            filtro.CategoriaId,
            filtro.Pagina,
            TamanhoPagina);

        var resultado = await _repository.PesquisarAsync(consulta, cancellationToken);
        return new PesquisaExamesResposta(resultado, filtros, erros);
    }

    private static IReadOnlyDictionary<string, string[]> Validar(
        PesquisaExamesFiltro filtro,
        string? termo)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (termo is not null && termo.Length is < 2 or > 200)
        {
            erros[nameof(filtro.Termo)] =
            [
                "Informe de 2 a 200 caracteres ou deixe o termo vazio."
            ];
        }

        if (filtro.Pagina <= 0)
        {
            erros[nameof(filtro.Pagina)] = ["A página deve ser maior que zero."];
        }

        if (filtro.TipoExameId <= 0)
        {
            erros[nameof(filtro.TipoExameId)] = ["Selecione um tipo de exame válido."];
        }

        if (filtro.CategoriaId <= 0)
        {
            erros[nameof(filtro.CategoriaId)] = ["Selecione uma categoria válida."];
        }

        return erros;
    }
}
