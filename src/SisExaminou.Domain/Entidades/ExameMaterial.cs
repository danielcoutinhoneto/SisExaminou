using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class ExameMaterial
{
    public ExameMaterial(
        int exameId,
        int materialId,
        bool principal,
        string? observacao,
        DateTime criadoEmUtc)
    {
        ExameId = ValidacaoDominio.Identificador(exameId, nameof(exameId));
        MaterialId = ValidacaoDominio.Identificador(materialId, nameof(materialId));
        Principal = principal;
        Observacao = ValidacaoDominio.TextoOpcional(observacao, 300, nameof(observacao));
        CriadoEmUtc = criadoEmUtc;
    }

    public int ExameId { get; }
    public int MaterialId { get; }
    public bool Principal { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CriadoEmUtc { get; }
}
