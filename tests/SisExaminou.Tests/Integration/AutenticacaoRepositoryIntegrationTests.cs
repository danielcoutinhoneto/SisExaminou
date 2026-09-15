using System.Data;
using Microsoft.Data.SqlClient;
using SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

namespace SisExaminou.Tests.Integration;

public sealed class AutenticacaoRepositoryIntegrationTests
{
    private const string VariavelConnectionString =
        "SISEXAMINOU_TEST_CONNECTION_STRING";

    [SqlServerIntegrationFact]
    public async Task ConsultasDeSeguranca_NaoEncontramLoginInexistenteNemAceitamInjecao()
    {
        var repository = CriarRepository();

        var inexistente = await repository.ObterPorLoginAsync(
            $"inexistente-{Guid.NewGuid():N}",
            CancellationToken.None);
        var tentativaInjecao = await repository.ObterPorLoginAsync(
            "' OR 1=1 --",
            CancellationToken.None);
        var sessaoInexistente = await repository.ObterSessaoValidaAsync(
            int.MaxValue,
            1,
            CancellationToken.None);

        Assert.Null(inexistente);
        Assert.Null(tentativaInjecao);
        Assert.Null(sessaoInexistente);
    }

    [SqlServerIntegrationFact]
    public async Task FalhasAplicamBloqueioELimpezaRestauraEstadoDoUsuario()
    {
        var connectionString = ObterConnectionString();
        var repository = new AutenticacaoRepository(connectionString);
        var login = $"teste-seguranca-{Guid.NewGuid():N}";
        int? usuarioId = null;
        var agoraUtc = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);

        try
        {
            usuarioId = await CriarUsuarioTemporarioAsync(
                connectionString,
                login,
                agoraUtc);

            for (var tentativa = 1; tentativa <= 5; tentativa++)
            {
                await repository.RegistrarFalhaAsync(
                    usuarioId.Value,
                    agoraUtc.AddSeconds(tentativa),
                    5,
                    TimeSpan.FromMinutes(15),
                    CancellationToken.None);
            }

            var bloqueado = await repository.ObterPorLoginAsync(
                login,
                CancellationToken.None);
            Assert.NotNull(bloqueado);
            Assert.Equal(5, bloqueado.TentativasFalhas);
            Assert.True(bloqueado.BloqueadoAteUtc > agoraUtc.AddMinutes(14));

            await repository.LimparFalhasAsync(
                usuarioId.Value,
                agoraUtc.AddMinutes(1),
                CancellationToken.None);
            var limpo = await repository.ObterPorLoginAsync(
                login,
                CancellationToken.None);

            Assert.NotNull(limpo);
            Assert.Equal(0, limpo.TentativasFalhas);
            Assert.Null(limpo.BloqueadoAteUtc);
        }
        finally
        {
            if (usuarioId is not null)
            {
                await ExcluirUsuarioTemporarioAsync(
                    connectionString,
                    usuarioId.Value);
            }
        }
    }

    private static AutenticacaoRepository CriarRepository() =>
        new(ObterConnectionString());

    private static string ObterConnectionString() =>
        Environment.GetEnvironmentVariable(VariavelConnectionString)!;

    private static async Task<int> CriarUsuarioTemporarioAsync(
        string connectionString,
        string login,
        DateTime agoraUtc)
    {
        const string sql = """
            INSERT INTO seguranca.Usuario
            (
                PerfilId,
                NomeCompleto,
                Login,
                SenhaHash,
                Ativo,
                TentativasFalhas,
                VersaoCredencial,
                CriadoEmUtc
            )
            OUTPUT INSERTED.UsuarioId
            SELECT
                PerfilId,
                @NomeCompleto,
                @Login,
                @SenhaHash,
                1,
                0,
                1,
                @AgoraUtc
            FROM seguranca.Perfil
            WHERE Codigo = N'ADMINISTRADOR'
              AND Ativo = 1;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@NomeCompleto", SqlDbType.NVarChar, 150).Value =
            "Usuário temporário de integração";
        command.Parameters.Add("@Login", SqlDbType.NVarChar, 100).Value = login;
        command.Parameters.Add("@SenhaHash", SqlDbType.NVarChar, 512).Value =
            "HASH_TEMPORARIO_NAO_UTILIZAVEL";
        command.Parameters.Add("@AgoraUtc", SqlDbType.DateTime2).Value = agoraUtc;

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task ExcluirUsuarioTemporarioAsync(
        string connectionString,
        int usuarioId)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "DELETE FROM seguranca.Usuario WHERE UsuarioId = @UsuarioId;",
            connection);
        command.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        await command.ExecuteNonQueryAsync();
    }
}
