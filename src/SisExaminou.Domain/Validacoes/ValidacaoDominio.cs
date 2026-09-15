namespace SisExaminou.Domain.Validacoes;

internal static class ValidacaoDominio
{
    public static int Identificador(int valor, string parametro)
    {
        if (valor < 0)
        {
            throw new ArgumentOutOfRangeException(parametro);
        }

        return valor;
    }

    public static string TextoObrigatorio(string? valor, int tamanhoMaximo, string parametro)
    {
        var normalizado = valor?.Trim();
        if (string.IsNullOrWhiteSpace(normalizado))
        {
            throw new ArgumentException("O valor não pode ser vazio.", parametro);
        }

        if (normalizado.Length > tamanhoMaximo)
        {
            throw new ArgumentException($"O valor deve possuir no máximo {tamanhoMaximo} caracteres.", parametro);
        }

        return normalizado;
    }

    public static string? TextoOpcional(string? valor, int tamanhoMaximo, string parametro)
    {
        if (valor is null)
        {
            return null;
        }

        var normalizado = valor.Trim();
        if (normalizado.Length == 0)
        {
            throw new ArgumentException("O valor informado não pode ser vazio.", parametro);
        }

        if (normalizado.Length > tamanhoMaximo)
        {
            throw new ArgumentException($"O valor deve possuir no máximo {tamanhoMaximo} caracteres.", parametro);
        }

        return normalizado;
    }

    public static byte[] Versao(byte[]? valor)
    {
        return valor is null ? [] : [.. valor];
    }
}
