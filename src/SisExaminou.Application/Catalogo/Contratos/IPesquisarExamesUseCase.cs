using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Application.Catalogo.Contratos;

public interface IPesquisarExamesUseCase
{
    Task<PesquisaExamesResposta> ExecutarAsync(
        PesquisaExamesFiltro filtro,
        CancellationToken cancellationToken);
}
