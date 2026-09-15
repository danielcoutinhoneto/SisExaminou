using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class Usuario
{
    public Usuario(
        int usuarioId,
        int perfilId,
        string nomeCompleto,
        string login,
        string? email,
        string senhaHash,
        bool ativo,
        int tentativasFalhas,
        DateTime? bloqueadoAteUtc,
        int versaoCredencial,
        DateTime? ultimoAcessoEmUtc,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        if (tentativasFalhas < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tentativasFalhas));
        }

        if (versaoCredencial < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(versaoCredencial));
        }

        UsuarioId = ValidacaoDominio.Identificador(usuarioId, nameof(usuarioId));
        PerfilId = ValidacaoDominio.Identificador(perfilId, nameof(perfilId));
        NomeCompleto = ValidacaoDominio.TextoObrigatorio(nomeCompleto, 150, nameof(nomeCompleto));
        Login = ValidacaoDominio.TextoObrigatorio(login, 100, nameof(login));
        Email = ValidacaoDominio.TextoOpcional(email, 254, nameof(email));
        SenhaHash = ValidacaoDominio.TextoObrigatorio(senhaHash, 512, nameof(senhaHash));
        Ativo = ativo;
        TentativasFalhas = tentativasFalhas;
        BloqueadoAteUtc = bloqueadoAteUtc;
        VersaoCredencial = versaoCredencial;
        UltimoAcessoEmUtc = ultimoAcessoEmUtc;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int UsuarioId { get; }
    public int PerfilId { get; }
    public string NomeCompleto { get; private set; }
    public string Login { get; }
    public string? Email { get; private set; }
    public string SenhaHash { get; private set; }
    public bool Ativo { get; private set; }
    public int TentativasFalhas { get; private set; }
    public DateTime? BloqueadoAteUtc { get; private set; }
    public int VersaoCredencial { get; private set; }
    public DateTime? UltimoAcessoEmUtc { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public bool EstaBloqueado(DateTime agoraUtc) =>
        BloqueadoAteUtc is not null && BloqueadoAteUtc > agoraUtc;

    public bool PodeIniciarSessao(Perfil perfil, DateTime agoraUtc)
    {
        ArgumentNullException.ThrowIfNull(perfil);

        return Ativo
            && perfil.PerfilId == PerfilId
            && perfil.Ativo
            && !EstaBloqueado(agoraUtc);
    }
}
