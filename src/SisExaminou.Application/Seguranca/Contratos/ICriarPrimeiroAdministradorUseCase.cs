using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.Contratos;

public interface ICriarPrimeiroAdministradorUseCase
{
    Task<BootstrapAdministradorResultado> ExecutarAsync(
        CriarAdministradorComando comando,
        CancellationToken cancellationToken);
}
