using System.Data;
using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

public sealed partial class CatalogoConsultaRepository
{
    public Task<ResultadoPaginado<ExameResumoDto>> PesquisarAsync(
        PesquisaExamesConsulta consulta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlPesquisaExames.Comando);

                var termo = command.Parameters.Add("@Termo", SqlDbType.NVarChar, 200);
                termo.Value = consulta.Termo is null
                    ? DBNull.Value
                    : consulta.Termo;

                var termoPrefixo = command.Parameters.Add(
                    "@TermoPrefixo",
                    SqlDbType.NVarChar,
                    401);
                termoPrefixo.Value = consulta.Termo is null
                    ? DBNull.Value
                    : EscaparLike(consulta.Termo) + "%";

                var tipo = command.Parameters.Add("@TipoExameId", SqlDbType.Int);
                tipo.Value = consulta.TipoExameId is null
                    ? DBNull.Value
                    : consulta.TipoExameId.Value;

                var categoria = command.Parameters.Add("@CategoriaId", SqlDbType.Int);
                categoria.Value = consulta.CategoriaId is null
                    ? DBNull.Value
                    : consulta.CategoriaId.Value;

                command.Parameters.Add("@Offset", SqlDbType.Int).Value =
                    checked((consulta.Pagina - 1) * consulta.TamanhoPagina);
                command.Parameters.Add("@TamanhoPagina", SqlDbType.Int).Value =
                    consulta.TamanhoPagina;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                await reader.ReadAsync(cancellationToken);
                var totalItens = reader.GetInt32(reader.GetOrdinal("TotalItens"));

                await reader.NextResultAsync(cancellationToken);
                var itens = new List<ExameResumoDto>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    itens.Add(new ExameResumoDto(
                        reader.GetString(reader.GetOrdinal("Codigo")),
                        reader.GetString(reader.GetOrdinal("Nome")),
                        ObterTextoOpcional(reader, "NomePopular"),
                        reader.GetString(reader.GetOrdinal("TipoExame")),
                        reader.GetString(reader.GetOrdinal("Categoria")),
                        ObterTextoOpcional(reader, "PrazoResultado")));
                }

                return new ResultadoPaginado<ExameResumoDto>(
                    itens,
                    consulta.Pagina,
                    consulta.TamanhoPagina,
                    totalItens);
            },
            cancellationToken);
    }
}
