using System.Text.RegularExpressions;

namespace SisExaminou.Domain.ObjetosDeValor;

public sealed partial record CodigoExame
{
    private CodigoExame(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static CodigoExame Criar(string valor)
    {
        var normalizado = valor?.Trim().ToUpperInvariant()
            ?? throw new ArgumentNullException(nameof(valor));

        if (!FormatoCodigoRegex().IsMatch(normalizado))
        {
            throw new ArgumentException(
                "O código deve possuir de 2 a 50 caracteres e utilizar apenas letras de A a Z, números ou hífen.",
                nameof(valor));
        }

        return new CodigoExame(normalizado);
    }

    public static bool TentarCriar(string? valor, out CodigoExame? codigo)
    {
        codigo = null;

        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        var normalizado = valor.Trim().ToUpperInvariant();
        if (!FormatoCodigoRegex().IsMatch(normalizado))
        {
            return false;
        }

        codigo = new CodigoExame(normalizado);
        return true;
    }

    public override string ToString() => Valor;

    [GeneratedRegex("^[A-Z0-9-]{2,50}$", RegexOptions.CultureInvariant)]
    private static partial Regex FormatoCodigoRegex();
}
