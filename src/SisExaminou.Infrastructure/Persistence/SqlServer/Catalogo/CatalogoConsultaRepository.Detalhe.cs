using System.Data;
using SisExaminou.Application.Catalogo.Modelos;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

public sealed partial class CatalogoConsultaRepository
{
    public Task<ExameDetalheDto?> ObterDetalheAsync(
        string codigo,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync<ExameDetalheDto?>(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlDetalheExame.Comando);
                command.Parameters.Add("@Codigo", SqlDbType.NVarChar, 50).Value = codigo;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                var codigoEncontrado = reader.GetString(reader.GetOrdinal("Codigo"));
                var nome = reader.GetString(reader.GetOrdinal("Nome"));
                var nomePopular = ObterTextoOpcional(reader, "NomePopular");
                var descricaoPopular = ObterTextoOpcional(reader, "DescricaoPopular");
                var prazoResultado = ObterTextoOpcional(reader, "PrazoResultado");
                var tipoExame = reader.GetString(reader.GetOrdinal("TipoExame"));
                var categoria = reader.GetString(reader.GetOrdinal("Categoria"));

                await reader.NextResultAsync(cancellationToken);
                var sinonimos = new List<string>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    sinonimos.Add(reader.GetString(reader.GetOrdinal("Nome")));
                }

                await reader.NextResultAsync(cancellationToken);
                var orientacoes = new List<OrientacaoDto>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    orientacoes.Add(new OrientacaoDto(
                        reader.GetString(reader.GetOrdinal("Titulo")),
                        reader.GetString(reader.GetOrdinal("Tipo")),
                        reader.GetString(reader.GetOrdinal("Conteudo")),
                        reader.GetInt32(reader.GetOrdinal("OrdemExibicao"))));
                }

                await reader.NextResultAsync(cancellationToken);
                var materiais = new List<MaterialDto>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    materiais.Add(new MaterialDto(
                        reader.GetString(reader.GetOrdinal("Nome")),
                        ObterTextoOpcional(reader, "Sigla"),
                        ObterTextoOpcional(reader, "Observacao"),
                        reader.GetBoolean(reader.GetOrdinal("Principal"))));
                }

                return new ExameDetalheDto(
                    codigoEncontrado,
                    nome,
                    nomePopular,
                    descricaoPopular,
                    prazoResultado,
                    tipoExame,
                    categoria,
                    sinonimos,
                    orientacoes,
                    materiais);
            },
            cancellationToken);
    }
}
