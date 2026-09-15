namespace SisExaminou.Web.Configuracao;

public sealed class SegurancaWebOptions
{
    public BloqueioOptions Bloqueio { get; init; } = new();
    public SessaoOptions Sessao { get; init; } = new();

    public void Validar()
    {
        if (Bloqueio.TentativasMaximas is < 1 or > 20)
        {
            throw new InvalidOperationException(
                "Seguranca:Bloqueio:TentativasMaximas deve estar entre 1 e 20.");
        }

        if (Bloqueio.DuracaoMinutos is < 1 or > 1440)
        {
            throw new InvalidOperationException(
                "Seguranca:Bloqueio:DuracaoMinutos deve estar entre 1 e 1440.");
        }

        if (Sessao.ExpiracaoMinutos is < 5 or > 1440)
        {
            throw new InvalidOperationException(
                "Seguranca:Sessao:ExpiracaoMinutos deve estar entre 5 e 1440.");
        }

        if (Sessao.IntervaloRevalidacaoMinutos is < 0 or > 60)
        {
            throw new InvalidOperationException(
                "Seguranca:Sessao:IntervaloRevalidacaoMinutos deve estar entre 0 e 60.");
        }
    }
}

public sealed class BloqueioOptions
{
    public int TentativasMaximas { get; init; } = 5;
    public int DuracaoMinutos { get; init; } = 15;
}

public sealed class SessaoOptions
{
    public int ExpiracaoMinutos { get; init; } = 30;
    public bool RenovacaoDeslizante { get; init; } = true;
    public int IntervaloRevalidacaoMinutos { get; init; } = 5;
}
