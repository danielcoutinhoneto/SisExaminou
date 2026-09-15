using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Application.Catalogo.Contratos;

public interface ICatalogoConsultaRepository
{
    Task<CatalogoFiltrosDto> ListarFiltrosAsync(CancellationToken cancellationToken);

    Task<ResultadoPaginado<ExameResumoDto>> PesquisarAsync(
        PesquisaExamesConsulta consulta,
        CancellationToken cancellationToken);

    Task<ExameDetalheDto?> ObterDetalheAsync(
        string codigo,
        CancellationToken cancellationToken);
}
