using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.CasosDeUso;

public sealed class AutenticarUsuarioUseCase : IAutenticarUsuarioUseCase
{
    public const string MensagemCredenciaisInvalidas =
        "Login ou senha inválidos.";

    private readonly IAutenticacaoRepository _repository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly PoliticaAutenticacao _politica;
    private readonly TimeProvider _timeProvider;

    public AutenticarUsuarioUseCase(
        IAutenticacaoRepository repository,
        ISenhaHasher senhaHasher,
        PoliticaAutenticacao politica,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _senhaHasher = senhaHasher;
        _politica = politica;
        _timeProvider = timeProvider;
    }

    public async Task<AutenticacaoResultado> ExecutarAsync(
        AutenticarUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var login = comando.Login?.Trim();
        var senha = comando.Senha ?? string.Empty;
        if (string.IsNullOrWhiteSpace(login)
            || login.Length > 100
            || senha.Length is < 1 or > 128)
        {
            _senhaHasher.Verificar(null, senha);
            return CredenciaisInvalidas();
        }

        var usuario = await _repository.ObterPorLoginAsync(
            login,
            cancellationToken);

        var senhaValida = _senhaHasher.Verificar(usuario?.SenhaHash, senha);
        var agoraUtc = _timeProvider.GetUtcNow().UtcDateTime;

        if (usuario is null)
        {
            return CredenciaisInvalidas();
        }

        var bloqueado = usuario.BloqueadoAteUtc is not null
            && usuario.BloqueadoAteUtc > agoraUtc;

        if (!usuario.Ativo || !usuario.PerfilAtivo || bloqueado)
        {
            return CredenciaisInvalidas();
        }

        if (!senhaValida)
        {
            await _repository.RegistrarFalhaAsync(
                usuario.UsuarioId,
                agoraUtc,
                _politica.TentativasMaximas,
                _politica.DuracaoBloqueio,
                cancellationToken);

            return CredenciaisInvalidas();
        }

        await _repository.LimparFalhasAsync(
            usuario.UsuarioId,
            agoraUtc,
            cancellationToken);

        var ultimoAcessoAtualizado = true;
        try
        {
            ultimoAcessoAtualizado = await _repository.AtualizarUltimoAcessoAsync(
                usuario.UsuarioId,
                agoraUtc,
                cancellationToken);
        }
        catch (SegurancaIndisponivelException)
        {
            ultimoAcessoAtualizado = false;
        }

        var sessao = new SessaoUsuarioDto(
            usuario.UsuarioId,
            usuario.NomeCompleto,
            usuario.CodigoPerfil,
            usuario.VersaoCredencial,
            usuario.Permissoes.Distinct(StringComparer.Ordinal).ToArray());

        return new AutenticacaoResultado(
            StatusAutenticacao.Sucesso,
            sessao,
            null,
            ultimoAcessoAtualizado);
    }

    private static AutenticacaoResultado CredenciaisInvalidas() =>
        new(
            StatusAutenticacao.CredenciaisInvalidas,
            null,
            MensagemCredenciaisInvalidas,
            false);
}
