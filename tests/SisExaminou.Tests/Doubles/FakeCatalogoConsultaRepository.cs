using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Tests.Doubles;

internal sealed class FakeCatalogoConsultaRepository : ICatalogoConsultaRepository
{
    public int QuantidadePesquisas { get; private set; }
    public PesquisaExamesConsulta? UltimaConsulta { get; private set; }
    public string? UltimoCodigo { get; private set; }
    public Exception? Excecao { get; set; }

    public CatalogoFiltrosDto Filtros { get; set; } = new(
        [new OpcaoFiltroDto(1, "Laboratorial")],
        [new OpcaoFiltroDto(1, "Rotina")]);

    public ResultadoPaginado<ExameResumoDto> ResultadoPesquisa { get; set; } =
        new([], 1, 10, 0);

    public ExameDetalheDto? Detalhe { get; set; }

    public Task<CatalogoFiltrosDto> ListarFiltrosAsync(
        CancellationToken cancellationToken)
    {
        LancarSeConfigurado();
        return Task.FromResult(Filtros);
    }

    public Task<ResultadoPaginado<ExameResumoDto>> PesquisarAsync(
        PesquisaExamesConsulta consulta,
        CancellationToken cancellationToken)
    {
        LancarSeConfigurado();
        QuantidadePesquisas++;
        UltimaConsulta = consulta;
        return Task.FromResult(ResultadoPesquisa);
    }

    public Task<ExameDetalheDto?> ObterDetalheAsync(
        string codigo,
        CancellationToken cancellationToken)
    {
        LancarSeConfigurado();
        UltimoCodigo = codigo;
        return Task.FromResult(Detalhe);
    }

    private void LancarSeConfigurado()
    {
        if (Excecao is not null)
        {
            throw Excecao;
        }
    }
}
