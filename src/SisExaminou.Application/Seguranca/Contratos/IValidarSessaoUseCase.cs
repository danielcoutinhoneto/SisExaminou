using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.Contratos;

public interface IValidarSessaoUseCase
{
    Task<SessaoUsuarioDto?> ExecutarAsync(
        int usuarioId,
        int versaoCredencial,
        CancellationToken cancellationToken);
}
