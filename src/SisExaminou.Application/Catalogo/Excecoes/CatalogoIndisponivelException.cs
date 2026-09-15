namespace SisExaminou.Application.Catalogo.Excecoes;

public sealed class CatalogoIndisponivelException : Exception
{
    public CatalogoIndisponivelException(string mensagem, Exception innerException)
        : base(mensagem, innerException)
    {
    }
}
