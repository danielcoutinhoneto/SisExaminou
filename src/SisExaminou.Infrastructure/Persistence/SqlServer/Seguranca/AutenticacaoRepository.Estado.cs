using System.Data;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

public sealed partial class AutenticacaoRepository
{
    public Task RegistrarFalhaAsync(
        int usuarioId,
        DateTime agoraUtc,
        int tentativasMaximas,
        TimeSpan duracaoBloqueio,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlAutenticacao.RegistrarFalha);
                AdicionarUsuarioEData(command, usuarioId, agoraUtc);
                command.Parameters.Add("@TentativasMaximas", SqlDbType.Int).Value =
                    tentativasMaximas;
                command.Parameters.Add("@DuracaoBloqueioSegundos", SqlDbType.Int).Value =
                    checked((int)duracaoBloqueio.TotalSeconds);

                var afetados = Convert.ToInt32(
                    await command.ExecuteScalarAsync(cancellationToken));
                if (afetados != 1)
                {
                    throw new InvalidOperationException(
                        "O usuário da tentativa de autenticação não foi encontrado.");
                }

                return true;
            },
            cancellationToken);
    }

    public Task LimparFalhasAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlAutenticacao.LimparFalhas);
                AdicionarUsuarioEData(command, usuarioId, agoraUtc);

                var afetados = Convert.ToInt32(
                    await command.ExecuteScalarAsync(cancellationToken));
                if (afetados != 1)
                {
                    throw new InvalidOperationException(
                        "O usuário autenticado não foi encontrado.");
                }

                return true;
            },
            cancellationToken);
    }

    public Task<bool> AtualizarUltimoAcessoAsync(
        int usuarioId,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(
                    connection,
                    SqlAutenticacao.AtualizarUltimoAcesso);
                AdicionarUsuarioEData(command, usuarioId, agoraUtc);

                var afetados = Convert.ToInt32(
                    await command.ExecuteScalarAsync(cancellationToken));
                return afetados == 1;
            },
            cancellationToken);
    }

    private static void AdicionarUsuarioEData(
        Microsoft.Data.SqlClient.SqlCommand command,
        int usuarioId,
        DateTime agoraUtc)
    {
        command.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        command.Parameters.Add("@AgoraUtc", SqlDbType.DateTime2).Value = agoraUtc;
    }
}
