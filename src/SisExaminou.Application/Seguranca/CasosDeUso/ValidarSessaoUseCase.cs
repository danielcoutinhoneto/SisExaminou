using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.CasosDeUso;

public sealed class ValidarSessaoUseCase : IValidarSessaoUseCase
{
    private readonly IAutenticacaoRepository _repository;

    public ValidarSessaoUseCase(IAutenticacaoRepository repository)
    {
        _repository = repository;
    }

    public Task<SessaoUsuarioDto?> ExecutarAsync(
        int usuarioId,
        int versaoCredencial,
        CancellationToken cancellationToken)
    {
        if (usuarioId <= 0 || versaoCredencial <= 0)
        {
            return Task.FromResult<SessaoUsuarioDto?>(null);
        }

        return _repository.ObterSessaoValidaAsync(
            usuarioId,
            versaoCredencial,
            cancellationToken);
    }
}
