using SisExaminou.Application.Seguranca.Contratos;

namespace SisExaminou.Tests.Doubles;

internal sealed class FakeSenhaHasher : ISenhaHasher
{
    public const string HashValido = "HASH_VALIDO";
    public string SenhaEsperada { get; set; } = "frase-senha-valida";
    public int Verificacoes { get; private set; }
    public string? UltimoHashVerificado { get; private set; }
    public int HashesGerados { get; private set; }

    public string GerarHash(string senha)
    {
        HashesGerados++;
        return $"HASH::{senha.Length}";
    }

    public bool Verificar(string? senhaHash, string senhaInformada)
    {
        Verificacoes++;
        UltimoHashVerificado = senhaHash;
        return senhaHash == HashValido && senhaInformada == SenhaEsperada;
    }
}
