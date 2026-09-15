using Microsoft.AspNetCore.Identity;
using SisExaminou.Application.Seguranca.Contratos;

namespace SisExaminou.Infrastructure.Seguranca;

public sealed class SenhaHasher : ISenhaHasher
{
    private static readonly object Contexto = new();
    private readonly PasswordHasher<object> _passwordHasher = new();
    private readonly string _hashSentinela;

    public SenhaHasher()
    {
        _hashSentinela = _passwordHasher.HashPassword(
            Contexto,
            "sentinela-nao-e-credencial");
    }

    public string GerarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);
        return _passwordHasher.HashPassword(Contexto, senha);
    }

    public bool Verificar(string? senhaHash, string senhaInformada)
    {
        var hash = string.IsNullOrWhiteSpace(senhaHash)
            ? _hashSentinela
            : senhaHash;

        try
        {
            return _passwordHasher.VerifyHashedPassword(
                    Contexto,
                    hash,
                    senhaInformada)
                != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
