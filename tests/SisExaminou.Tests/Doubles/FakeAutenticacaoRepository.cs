using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Tests.Doubles;

internal sealed class FakeAutenticacaoRepository : IAutenticacaoRepository
{
    public UsuarioAutenticacaoDto? Usuario { get; set; }
    public SessaoUsuarioDto? SessaoValida { get; set; }
    public Exception? ExcecaoAoObter { get; set; }
    public Exception? ExcecaoAoValidarSessao { get; set; }
    public Exception? ExcecaoAoAtualizarUltimoAcesso { get; set; }
    public bool ResultadoBootstrap { get; set; } = true;
    public int FalhasRegistradas { get; private set; }
    public int LimpezasRegistradas { get; private set; }
    public int AtualizacoesUltimoAcesso { get; private set; }
    public int BootstrapExecutados { get; private set; }
    public int? UltimasTentativasMaximas { get; private set; }
    public TimeSpan? UltimaDuracaoBloqueio { get; private set; }
    public CriarAdministradorComando? UltimoAdministrador { get; private set; }
    public string? UltimoHashAdministrador { get; private set; }

    public Task<UsuarioAutenticacaoDto?> ObterPorLoginAsync(
        string login,
        CancellationToken cancellationToken)
    {
        if (ExcecaoAoObter is not null)
        {
            throw ExcecaoAoObter;
        }

        return Task.FromResult(Usuario);
    }

    public Task RegistrarFalhaAsync(
        int usuarioId,
        DateTime agoraUtc,
        int tentativasMaximas,
        TimeSpan duracaoBloqueio,
        CancellationToken cancellationToken)
    {
        FalhasRegistradas++;
        UltimasTentativasMaximas = tentativasMaximas;
        UltimaDuracaoBloqueio = duracaoBloqueio;
        return Task.CompletedTask;
    }

    public Task LimparFalhasAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        LimpezasRegistradas++;
        return Task.CompletedTask;
    }

    public Task<bool> AtualizarUltimoAcessoAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        if (ExcecaoAoAtualizarUltimoAcesso is not null)
        {
            throw ExcecaoAoAtualizarUltimoAcesso;
        }

        AtualizacoesUltimoAcesso++;
        return Task.FromResult(true);
    }

    public Task<SessaoUsuarioDto?> ObterSessaoValidaAsync(
        int usuarioId,
        int versaoCredencial,
        CancellationToken cancellationToken)
    {
        if (ExcecaoAoValidarSessao is not null)
        {
            throw ExcecaoAoValidarSessao;
        }

        var sessao = SessaoValida is not null
            && SessaoValida.UsuarioId == usuarioId
            && SessaoValida.VersaoCredencial == versaoCredencial
                ? SessaoValida
                : null;
        return Task.FromResult(sessao);
    }

    public Task<bool> CriarPrimeiroAdministradorAsync(
        string nomeCompleto,
        string login,
        string? email,
        string senhaHash,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        BootstrapExecutados++;
        UltimoAdministrador = new CriarAdministradorComando(
            nomeCompleto,
            login,
            email,
            null);
        UltimoHashAdministrador = senhaHash;
        return Task.FromResult(ResultadoBootstrap);
    }
}
