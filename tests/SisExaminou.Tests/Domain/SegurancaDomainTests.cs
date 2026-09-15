using SisExaminou.Domain.Entidades;

namespace SisExaminou.Tests.Domain;

public sealed class SegurancaDomainTests
{
    private static readonly DateTime Agora =
        new(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Usuario_SoIniciaSessaoComMesmoPerfilAtivoESemBloqueio()
    {
        var usuario = CriarUsuario(perfilId: 1);
        var perfilCorreto = CriarPerfil(1, ativo: true);
        var perfilDiferente = CriarPerfil(2, ativo: true);
        var perfilInativo = CriarPerfil(1, ativo: false);

        Assert.True(usuario.PodeIniciarSessao(perfilCorreto, Agora));
        Assert.False(usuario.PodeIniciarSessao(perfilDiferente, Agora));
        Assert.False(usuario.PodeIniciarSessao(perfilInativo, Agora));
    }

    [Fact]
    public void Usuario_BloqueioExpiraNoInstanteDefinido()
    {
        var usuario = CriarUsuario(bloqueadoAteUtc: Agora.AddMinutes(15));

        Assert.True(usuario.EstaBloqueado(Agora));
        Assert.False(usuario.EstaBloqueado(Agora.AddMinutes(15)));
    }

    [Fact]
    public void Usuario_ExigeUmaVersaoCredencialPositiva()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CriarUsuario(
            versaoCredencial: 0));
    }

    [Fact]
    public void PerfilEPermissao_PodemSerInativadosEReativados()
    {
        var perfil = CriarPerfil(1, ativo: true);
        var permissao = new Permissao(
            1,
            "CONTEUDO_GERENCIAR",
            "Gerenciar conteúdo",
            null,
            true,
            Agora,
            null,
            [1]);

        perfil.Inativar();
        permissao.Inativar();
        Assert.False(perfil.Ativo);
        Assert.False(permissao.Ativo);

        perfil.Ativar();
        permissao.Ativar();
        Assert.True(perfil.Ativo);
        Assert.True(permissao.Ativo);
    }

    private static Usuario CriarUsuario(
        int perfilId = 1,
        DateTime? bloqueadoAteUtc = null,
        int versaoCredencial = 1) =>
        new(
            1,
            perfilId,
            "Administrador",
            "admin",
            null,
            "hash",
            true,
            0,
            bloqueadoAteUtc,
            versaoCredencial,
            null,
            Agora,
            null,
            [1]);

    private static Perfil CriarPerfil(int id, bool ativo) =>
        new(
            id,
            $"PERFIL_{id}",
            $"Perfil {id}",
            null,
            ativo,
            Agora,
            null,
            [1]);
}
