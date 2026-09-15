using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class Orientacao
{
    public Orientacao(
        int orientacaoId,
        int exameId,
        string titulo,
        TipoOrientacao tipo,
        string conteudo,
        int ordemExibicao,
        bool ativo,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        if (ordemExibicao <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ordemExibicao));
        }

        OrientacaoId = ValidacaoDominio.Identificador(orientacaoId, nameof(orientacaoId));
        ExameId = ValidacaoDominio.Identificador(exameId, nameof(exameId));
        Titulo = ValidacaoDominio.TextoObrigatorio(titulo, 150, nameof(titulo));
        Tipo = tipo;
        Conteudo = ValidacaoDominio.TextoObrigatorio(conteudo, int.MaxValue, nameof(conteudo));
        OrdemExibicao = ordemExibicao;
        Ativo = ativo;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int OrientacaoId { get; }
    public int ExameId { get; }
    public string Titulo { get; private set; }
    public TipoOrientacao Tipo { get; private set; }
    public string Conteudo { get; private set; }
    public int OrdemExibicao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public void Ativar() => Ativo = true;
    public void Inativar() => Ativo = false;
}
