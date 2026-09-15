using System.Net.Mail;
using System.Text.RegularExpressions;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Application.Seguranca.CasosDeUso;

public sealed class CriarPrimeiroAdministradorUseCase
    : ICriarPrimeiroAdministradorUseCase
{
    private static readonly Regex FormatoLogin = new(
        "^[a-zA-Z0-9._-]{3,100}$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    private readonly IAutenticacaoRepository _repository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly TimeProvider _timeProvider;

    public CriarPrimeiroAdministradorUseCase(
        IAutenticacaoRepository repository,
        ISenhaHasher senhaHasher,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _senhaHasher = senhaHasher;
        _timeProvider = timeProvider;
    }

    public async Task<BootstrapAdministradorResultado> ExecutarAsync(
        CriarAdministradorComando comando,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var nome = comando.NomeCompleto?.Trim();
        var login = comando.Login?.Trim().ToLowerInvariant();
        var email = string.IsNullOrWhiteSpace(comando.Email)
            ? null
            : comando.Email.Trim().ToLowerInvariant();
        var senha = comando.Senha ?? string.Empty;

        var erros = Validar(nome, login, email, senha);
        if (erros.Count > 0)
        {
            return new BootstrapAdministradorResultado(
                StatusBootstrapAdministrador.DadosInvalidos,
                erros);
        }

        var senhaHash = _senhaHasher.GerarHash(senha);
        var criado = await _repository.CriarPrimeiroAdministradorAsync(
            nome!,
            login!,
            email,
            senhaHash,
            _timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return new BootstrapAdministradorResultado(
            criado
                ? StatusBootstrapAdministrador.Criado
                : StatusBootstrapAdministrador.AdministradorAtivoJaExiste,
            new Dictionary<string, string[]>(StringComparer.Ordinal));
    }

    private static IReadOnlyDictionary<string, string[]> Validar(
        string? nome,
        string? login,
        string? email,
        string senha)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (nome is null || nome.Length is < 3 or > 150)
        {
            erros[nameof(CriarAdministradorComando.NomeCompleto)] =
                ["Informe um nome de 3 a 150 caracteres."];
        }

        if (login is null || !FormatoLogin.IsMatch(login))
        {
            erros[nameof(CriarAdministradorComando.Login)] =
                ["Use de 3 a 100 letras, números, ponto, hífen ou sublinhado."];
        }

        if (email is not null
            && (email.Length > 254 || !MailAddress.TryCreate(email, out _)))
        {
            erros[nameof(CriarAdministradorComando.Email)] =
                ["Informe um email válido com até 254 caracteres."];
        }

        if (senha.Length is < 15 or > 128 || string.IsNullOrWhiteSpace(senha))
        {
            erros[nameof(CriarAdministradorComando.Senha)] =
                ["Use uma frase-senha de 15 a 128 caracteres."];
        }

        return erros;
    }
}
