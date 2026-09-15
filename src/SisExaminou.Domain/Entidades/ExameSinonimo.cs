using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class ExameSinonimo
{
    public ExameSinonimo(
        int exameSinonimoId,
        int exameId,
        string nome,
        bool ativo,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        ExameSinonimoId = ValidacaoDominio.Identificador(exameSinonimoId, nameof(exameSinonimoId));
        ExameId = ValidacaoDominio.Identificador(exameId, nameof(exameId));
        Nome = ValidacaoDominio.TextoObrigatorio(nome, 200, nameof(nome));
        Ativo = ativo;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int ExameSinonimoId { get; }
    public int ExameId { get; }
    public string Nome { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public void Ativar() => Ativo = true;
    public void Inativar() => Ativo = false;
}
