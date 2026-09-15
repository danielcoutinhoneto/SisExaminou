using System.Data;
using Microsoft.Data.SqlClient;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Catalogo.Excecoes;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

public sealed partial class CatalogoConsultaRepository : ICatalogoConsultaRepository
{
    private const int TimeoutPadraoSegundos = 30;

    private readonly string _connectionString;
    private readonly int _commandTimeoutSegundos;

    public CatalogoConsultaRepository(
        string connectionString,
        int commandTimeoutSegundos = TimeoutPadraoSegundos)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "A connection string do catálogo não foi configurada.",
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
            throw new CatalogoIndisponivelException(
                "Não foi possível consultar o catálogo no SQL Server.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new CatalogoIndisponivelException(
                "A conexão com o catálogo está indisponível.",
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

    private SqlCommand CriarComando(SqlConnection connection, string sql)
    {
        return new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = _commandTimeoutSegundos
        };
    }

    private static string? ObterTextoOpcional(SqlDataReader reader, string coluna)
    {
        var ordinal = reader.GetOrdinal(coluna);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static string EscaparLike(string termo)
    {
        return termo
            .Replace("~", "~~", StringComparison.Ordinal)
            .Replace("%", "~%", StringComparison.Ordinal)
            .Replace("_", "~_", StringComparison.Ordinal)
            .Replace("[", "~[", StringComparison.Ordinal);
    }
}
