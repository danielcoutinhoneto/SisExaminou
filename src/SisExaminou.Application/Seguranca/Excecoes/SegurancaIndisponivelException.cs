namespace SisExaminou.Application.Seguranca.Excecoes;

public sealed class SegurancaIndisponivelException : Exception
{
    public SegurancaIndisponivelException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
