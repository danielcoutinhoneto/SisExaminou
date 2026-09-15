using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

public sealed partial class CatalogoConsultaRepository
{
    public Task<CatalogoFiltrosDto> ListarFiltrosAsync(
        CancellationToken cancellationToken)
    {
        return ExecutarAsync(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlFiltrosCatalogo.Comando);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                var tipos = new List<OpcaoFiltroDto>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    tipos.Add(new OpcaoFiltroDto(
                        reader.GetInt32(reader.GetOrdinal("Id")),
                        reader.GetString(reader.GetOrdinal("Nome"))));
                }

                var categorias = new List<OpcaoFiltroDto>();
                await reader.NextResultAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    categorias.Add(new OpcaoFiltroDto(
                        reader.GetInt32(reader.GetOrdinal("Id")),
                        reader.GetString(reader.GetOrdinal("Nome"))));
                }

                return new CatalogoFiltrosDto(tipos, categorias);
            },
            cancellationToken);
    }
}
