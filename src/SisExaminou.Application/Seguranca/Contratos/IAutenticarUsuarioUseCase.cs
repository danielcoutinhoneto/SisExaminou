using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.Contratos;

public interface IAutenticarUsuarioUseCase
{
    Task<AutenticacaoResultado> ExecutarAsync(
        AutenticarUsuarioComando comando,
        CancellationToken cancellationToken);
}
