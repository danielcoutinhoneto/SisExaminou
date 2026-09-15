using SisExaminou.Domain.Validacoes;

namespace SisExaminou.Domain.Entidades;

public sealed class PerfilPermissao
{
    public PerfilPermissao(
        int perfilId,
        int permissaoId,
        DateTime criadoEmUtc)
    {
        PerfilId = ValidacaoDominio.Identificador(perfilId, nameof(perfilId));
        PermissaoId = ValidacaoDominio.Identificador(permissaoId, nameof(permissaoId));
        CriadoEmUtc = criadoEmUtc;
    }

    public int PerfilId { get; }
    public int PermissaoId { get; }
    public DateTime CriadoEmUtc { get; }
}
