using System.Net;

namespace SisExaminou.Tests.Web;

public sealed partial class SegurancaWebTests
{
    private static string ExtrairAntiforgery(string html)
    {
        const string nome = "name=\"__RequestVerificationToken\"";
        const string prefixoValor = "value=\"";
        var inicioNome = html.IndexOf(nome, StringComparison.Ordinal);
        Assert.True(inicioNome >= 0, "Token antiforgery não encontrado no HTML.");

        var inicioValor = html.IndexOf(prefixoValor, inicioNome, StringComparison.Ordinal);
        Assert.True(inicioValor >= 0, "Valor antiforgery não encontrado no HTML.");
        inicioValor += prefixoValor.Length;
        var fimValor = html.IndexOf(Convert.ToChar(34), inicioValor);
        Assert.True(fimValor > inicioValor, "Valor antiforgery inválido.");

        return WebUtility.HtmlDecode(html[inicioValor..fimValor]);
    }
}
