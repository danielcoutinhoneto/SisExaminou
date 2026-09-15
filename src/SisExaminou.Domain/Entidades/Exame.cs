using SisExaminou.Domain.ObjetosDeValor;
using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class Exame
{
    public Exame(
        int exameId,
        int tipoExameId,
        int categoriaId,
        CodigoExame codigo,
        string nome,
        string? nomePopular,
        string? descricaoPopular,
        string? prazoResultado,
        bool ativo,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        ExameId = ValidacaoDominio.Identificador(exameId, nameof(exameId));
        TipoExameId = ValidacaoDominio.Identificador(tipoExameId, nameof(tipoExameId));
        CategoriaId = ValidacaoDominio.Identificador(categoriaId, nameof(categoriaId));
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nome = ValidacaoDominio.TextoObrigatorio(nome, 200, nameof(nome));
        NomePopular = ValidacaoDominio.TextoOpcional(nomePopular, 200, nameof(nomePopular));
        DescricaoPopular = ValidacaoDominio.TextoOpcional(descricaoPopular, 1000, nameof(descricaoPopular));
        PrazoResultado = ValidacaoDominio.TextoOpcional(prazoResultado, 300, nameof(prazoResultado));
        Ativo = ativo;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int ExameId { get; }
    public int TipoExameId { get; }
    public int CategoriaId { get; }
    public CodigoExame Codigo { get; }
    public string Nome { get; private set; }
    public string? NomePopular { get; private set; }
    public string? DescricaoPopular { get; private set; }
    public string? PrazoResultado { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public bool PodeSerPublicado(
        TipoExame tipoExame,
        Categoria categoria,
        IEnumerable<Orientacao> orientacoes)
    {
        ArgumentNullException.ThrowIfNull(tipoExame);
        ArgumentNullException.ThrowIfNull(categoria);
        ArgumentNullException.ThrowIfNull(orientacoes);

        return Ativo
            && tipoExame.TipoExameId == TipoExameId
            && tipoExame.Ativo
            && categoria.CategoriaId == CategoriaId
            && categoria.Ativo
            && orientacoes.Any(orientacao =>
                orientacao.ExameId == ExameId
                && orientacao.Ativo);
    }

    public void Ativar() => Ativo = true;
    public void Inativar() => Ativo = false;
}
