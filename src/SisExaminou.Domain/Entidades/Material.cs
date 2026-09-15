using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class Material
{
    public Material(
        int materialId,
        string nome,
        string? sigla,
        string? descricao,
        bool ativo,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        MaterialId = ValidacaoDominio.Identificador(materialId, nameof(materialId));
        Nome = ValidacaoDominio.TextoObrigatorio(nome, 100, nameof(nome));
        Sigla = ValidacaoDominio.TextoOpcional(sigla, 20, nameof(sigla));
        Descricao = ValidacaoDominio.TextoOpcional(descricao, 300, nameof(descricao));
        Ativo = ativo;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int MaterialId { get; }
    public string Nome { get; private set; }
    public string? Sigla { get; private set; }
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public void Ativar() => Ativo = true;
    public void Inativar() => Ativo = false;
}
