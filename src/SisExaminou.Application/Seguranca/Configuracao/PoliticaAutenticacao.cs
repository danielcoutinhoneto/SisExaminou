namespace SisExaminou.Application.Seguranca.Configuracao;

public sealed class PoliticaAutenticacao
{
    public const int TentativasMaximasPadrao = 5;
    public static readonly TimeSpan DuracaoBloqueioPadrao = TimeSpan.FromMinutes(15);

    public PoliticaAutenticacao(
        int tentativasMaximas = TentativasMaximasPadrao,
        TimeSpan? duracaoBloqueio = null)
    {
        if (tentativasMaximas is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(tentativasMaximas));
        }

        var duracao = duracaoBloqueio ?? DuracaoBloqueioPadrao;
        if (duracao <= TimeSpan.Zero || duracao > TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(duracaoBloqueio));
        }

        TentativasMaximas = tentativasMaximas;
        DuracaoBloqueio = duracao;
    }

    public int TentativasMaximas { get; }
    public TimeSpan DuracaoBloqueio { get; }
}
