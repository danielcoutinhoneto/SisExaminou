namespace SisExaminou.Application.Seguranca.Modelos;

public sealed record AutenticarUsuarioComando(string? Login, string? Senha);

public sealed record UsuarioAutenticacaoDto(
    int UsuarioId,
    int PerfilId,
    string NomeCompleto,
    string Login,
    string SenhaHash,
    bool Ativo,
    int TentativasFalhas,
    DateTime? BloqueadoAteUtc,
    int VersaoCredencial,
    string CodigoPerfil,
    bool PerfilAtivo,
    IReadOnlyList<string> Permissoes);

public sealed record SessaoUsuarioDto(
    int UsuarioId,
    string NomeCompleto,
    string CodigoPerfil,
    int VersaoCredencial,
    IReadOnlyList<string> Permissoes);

public enum StatusAutenticacao
{
    Sucesso,
    CredenciaisInvalidas
}

public sealed record AutenticacaoResultado(
    StatusAutenticacao Status,
    SessaoUsuarioDto? Sessao,
    string? Mensagem,
    bool UltimoAcessoAtualizado)
{
    public bool Autenticado => Status == StatusAutenticacao.Sucesso;
}

public sealed record CriarAdministradorComando(
    string? NomeCompleto,
    string? Login,
    string? Email,
    string? Senha);

public enum StatusBootstrapAdministrador
{
    Criado,
    AdministradorAtivoJaExiste,
    DadosInvalidos
}

public sealed record BootstrapAdministradorResultado(
    StatusBootstrapAdministrador Status,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public bool Criado => Status == StatusBootstrapAdministrador.Criado;
}
