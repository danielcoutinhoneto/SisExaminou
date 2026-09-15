using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Application.Catalogo.Contratos;

public interface IObterDetalheExameUseCase
{
    Task<ExameDetalheDto?> ExecutarAsync(
        string? codigo,
        CancellationToken cancellationToken);
}
