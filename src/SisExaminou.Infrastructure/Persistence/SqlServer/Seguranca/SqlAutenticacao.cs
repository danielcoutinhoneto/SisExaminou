namespace SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

internal static class SqlAutenticacao
{
    public const string ObterPorLogin = """
        SELECT
            Usuario.UsuarioId,
            Usuario.PerfilId,
            Usuario.NomeCompleto,
            Usuario.Login,
            Usuario.SenhaHash,
            Usuario.Ativo,
            Usuario.TentativasFalhas,
            Usuario.BloqueadoAteUtc,
            Usuario.VersaoCredencial,
            Perfil.Codigo AS CodigoPerfil,
            Perfil.Ativo AS PerfilAtivo,
            Permissao.Codigo AS CodigoPermissao
        FROM seguranca.Usuario AS Usuario
        INNER JOIN seguranca.Perfil AS Perfil
            ON Perfil.PerfilId = Usuario.PerfilId
        LEFT JOIN seguranca.PerfilPermissao AS PerfilPermissao
            ON PerfilPermissao.PerfilId = Perfil.PerfilId
        LEFT JOIN seguranca.Permissao AS Permissao
            ON Permissao.PermissaoId = PerfilPermissao.PermissaoId
           AND Permissao.Ativo = 1
        WHERE Usuario.Login = @Login
        ORDER BY Permissao.Codigo;
        """;

    public const string RegistrarFalha = """
        UPDATE Usuario WITH (UPDLOCK, ROWLOCK)
        SET
            TentativasFalhas =
                CASE
                    WHEN BloqueadoAteUtc IS NOT NULL
                     AND BloqueadoAteUtc <= @AgoraUtc THEN 1
                    ELSE TentativasFalhas + 1
                END,
            BloqueadoAteUtc =
                CASE
                    WHEN
                        CASE
                            WHEN BloqueadoAteUtc IS NOT NULL
                             AND BloqueadoAteUtc <= @AgoraUtc THEN 1
                            ELSE TentativasFalhas + 1
                        END >= @TentativasMaximas
                    THEN DATEADD(SECOND, @DuracaoBloqueioSegundos, @AgoraUtc)
                    WHEN BloqueadoAteUtc <= @AgoraUtc THEN NULL
                    ELSE BloqueadoAteUtc
                END,
            AtualizadoEmUtc = @AgoraUtc
        FROM seguranca.Usuario AS Usuario
        WHERE Usuario.UsuarioId = @UsuarioId;

        SELECT @@ROWCOUNT AS RegistrosAfetados;
        """;

    public const string LimparFalhas = """
        UPDATE seguranca.Usuario
        SET
            TentativasFalhas = 0,
            BloqueadoAteUtc = NULL,
            AtualizadoEmUtc = @AgoraUtc
        WHERE UsuarioId = @UsuarioId;

        SELECT @@ROWCOUNT AS RegistrosAfetados;
        """;

    public const string AtualizarUltimoAcesso = """
        UPDATE seguranca.Usuario
        SET UltimoAcessoEmUtc = @AgoraUtc
        WHERE UsuarioId = @UsuarioId;

        SELECT @@ROWCOUNT AS RegistrosAfetados;
        """;

    public const string ObterSessaoValida = """
        SELECT
            Usuario.UsuarioId,
            Usuario.NomeCompleto,
            Usuario.VersaoCredencial,
            Perfil.Codigo AS CodigoPerfil,
            Permissao.Codigo AS CodigoPermissao
        FROM seguranca.Usuario AS Usuario
        INNER JOIN seguranca.Perfil AS Perfil
            ON Perfil.PerfilId = Usuario.PerfilId
           AND Perfil.Ativo = 1
        LEFT JOIN seguranca.PerfilPermissao AS PerfilPermissao
            ON PerfilPermissao.PerfilId = Perfil.PerfilId
        LEFT JOIN seguranca.Permissao AS Permissao
            ON Permissao.PermissaoId = PerfilPermissao.PermissaoId
           AND Permissao.Ativo = 1
        WHERE Usuario.UsuarioId = @UsuarioId
          AND Usuario.Ativo = 1
          AND Usuario.VersaoCredencial = @VersaoCredencial
        ORDER BY Permissao.Codigo;
        """;

    public const string CriarPrimeiroAdministrador = """
        DECLARE @PerfilAdministradorId int;

        SELECT @PerfilAdministradorId = PerfilId
        FROM seguranca.Perfil WITH (UPDLOCK, HOLDLOCK)
        WHERE Codigo = N'ADMINISTRADOR'
          AND Ativo = 1;

        IF @PerfilAdministradorId IS NULL
        BEGIN
            THROW 51200, N'O perfil ADMINISTRADOR ativo não foi encontrado.', 1;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM seguranca.Usuario WITH (UPDLOCK, HOLDLOCK)
            WHERE PerfilId = @PerfilAdministradorId
              AND Ativo = 1
        )
        BEGIN
            SELECT CONVERT(bit, 0) AS Criado;
            RETURN;
        END;

        INSERT INTO seguranca.Usuario
        (
            PerfilId,
            NomeCompleto,
            Login,
            Email,
            SenhaHash,
            Ativo,
            TentativasFalhas,
            VersaoCredencial,
            CriadoEmUtc
        )
        VALUES
        (
            @PerfilAdministradorId,
            @NomeCompleto,
            @Login,
            @Email,
            @SenhaHash,
            1,
            0,
            1,
            @AgoraUtc
        );

        SELECT CONVERT(bit, 1) AS Criado;
        """;
}
