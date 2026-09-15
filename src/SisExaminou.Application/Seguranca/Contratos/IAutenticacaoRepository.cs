using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.Contratos;

public interface IAutenticacaoRepository
{
    Task<UsuarioAutenticacaoDto?> ObterPorLoginAsync(
        string login,
        CancellationToken cancellationToken);

    Task RegistrarFalhaAsync(
        int usuarioId,
        DateTime agoraUtc,
        int tentativasMaximas,
        TimeSpan duracaoBloqueio,
        CancellationToken cancellationToken);

    Task LimparFalhasAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken);

    Task<bool> AtualizarUltimoAcessoAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken);

    Task<SessaoUsuarioDto?> ObterSessaoValidaAsync(
        int usuarioId,
        int versaoCredencial,
        CancellationToken cancellationToken);

    Task<bool> CriarPrimeiroAdministradorAsync(
        string nomeCompleto,
        string login,
        string? email,
        string senhaHash,
        DateTime agoraUtc,
        CancellationToken cancellationToken);
}
