using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class Categoria
{
    public Categoria(
        int categoriaId,
        string nome,
        string? descricao,
        bool ativo,
        DateTime criadoEmUtc,
        DateTime? atualizadoEmUtc,
        byte[]? versao)
    {
        CategoriaId = ValidacaoDominio.Identificador(categoriaId, nameof(categoriaId));
        Nome = ValidacaoDominio.TextoObrigatorio(nome, 100, nameof(nome));
        Descricao = ValidacaoDominio.TextoOpcional(descricao, 300, nameof(descricao));
        Ativo = ativo;
        CriadoEmUtc = criadoEmUtc;
        AtualizadoEmUtc = atualizadoEmUtc;
        Versao = ValidacaoDominio.Versao(versao);
    }

    public int CategoriaId { get; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public byte[] Versao { get; private set; }

    public void Ativar() => Ativo = true;
    public void Inativar() => Ativo = false;
}
