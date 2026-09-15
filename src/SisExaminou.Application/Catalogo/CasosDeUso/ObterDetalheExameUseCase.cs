using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Modelos;
using SisExaminou.Domain.ObjetosDeValor;

namespace SisExaminou.Application.Catalogo.CasosDeUso;

public sealed class ObterDetalheExameUseCase : IObterDetalheExameUseCase
{
    private readonly ICatalogoConsultaRepository _repository;

    public ObterDetalheExameUseCase(ICatalogoConsultaRepository repository)
    {
        _repository = repository;
    }

    public Task<ExameDetalheDto?> ExecutarAsync(
        string? codigo,
        CancellationToken cancellationToken)
    {
        if (!CodigoExame.TentarCriar(codigo, out var codigoValido))
        {
            return Task.FromResult<ExameDetalheDto?>(null);
        }

        return _repository.ObterDetalheAsync(
            codigoValido!.Valor,
            cancellationToken);
    }
}
