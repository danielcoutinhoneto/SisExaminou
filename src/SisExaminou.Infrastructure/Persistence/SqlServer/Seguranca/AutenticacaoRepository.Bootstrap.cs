using System.Data;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

public sealed partial class AutenticacaoRepository
{
    public Task<bool> CriarPrimeiroAdministradorAsync(
        string nomeCompleto,
        string login,
        string? email,
        string senhaHash,
        DateTime agoraUtc,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                await using var transaction = await connection.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                try
                {
                    using var command = CriarComando(
                        connection,
                        SqlAutenticacao.CriarPrimeiroAdministrador,
                        (Microsoft.Data.SqlClient.SqlTransaction)transaction);

                    command.Parameters.Add("@NomeCompleto", SqlDbType.NVarChar, 150).Value =
                        nomeCompleto;
                    command.Parameters.Add("@Login", SqlDbType.NVarChar, 100).Value = login;
                    var emailParameter = command.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        254);
                    emailParameter.Value = email is null ? DBNull.Value : email;
                    command.Parameters.Add("@SenhaHash", SqlDbType.NVarChar, 512).Value =
                        senhaHash;
                    command.Parameters.Add("@AgoraUtc", SqlDbType.DateTime2).Value =
                        agoraUtc;

                    var criado = Convert.ToBoolean(
                        await command.ExecuteScalarAsync(cancellationToken));
                    await transaction.CommitAsync(cancellationToken);
                    return criado;
                }
                catch
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }
            },
            cancellationToken);
    }
}
