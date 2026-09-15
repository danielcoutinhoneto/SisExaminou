using System.Data;
using Microsoft.Data.SqlClient;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Excecoes;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

public sealed partial class AutenticacaoRepository : IAutenticacaoRepository
{
    private const int TimeoutPadraoSegundos = 30;
    private readonly string _connectionString;
    private readonly int _commandTimeoutSegundos;

    public AutenticacaoRepository(
        string connectionString,
        int commandTimeoutSegundos = TimeoutPadraoSegundos)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "A connection string de segurança não foi configurada.",
                nameof(connectionString));
        }

        if (commandTimeoutSegundos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(commandTimeoutSegundos));
        }

        _connectionString = new SqlConnectionStringBuilder(connectionString).ConnectionString;
        _commandTimeoutSegundos = commandTimeoutSegundos;
    }

    private async Task<T> ExecutarAsync<T>(
        Func<Task<T>> operacao,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operacao();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqlException exception)
        {
            throw new SegurancaIndisponivelException(
                "Não foi possível acessar os dados de segurança.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new SegurancaIndisponivelException(
                "A conexão com os dados de segurança está indisponível.",
                exception);
        }
    }

    private async Task<SqlConnection> AbrirConexaoAsync(
        CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private SqlCommand CriarComando(
        SqlConnection connection,
        string sql,
        SqlTransaction? transaction = null) =>
        new(sql, connection, transaction)
        {
            CommandType = CommandType.Text,
            CommandTimeout = _commandTimeoutSegundos
        };
}
