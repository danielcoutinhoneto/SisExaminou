using System.Data;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;

public sealed partial class AutenticacaoRepository
{
    public Task<UsuarioAutenticacaoDto?> ObterPorLoginAsync(
        string login,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync<UsuarioAutenticacaoDto?>(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlAutenticacao.ObterPorLogin);
                command.Parameters.Add("@Login", SqlDbType.NVarChar, 100).Value = login;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                var usuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId"));
                var perfilId = reader.GetInt32(reader.GetOrdinal("PerfilId"));
                var nomeCompleto = reader.GetString(reader.GetOrdinal("NomeCompleto"));
                var loginEncontrado = reader.GetString(reader.GetOrdinal("Login"));
                var senhaHash = reader.GetString(reader.GetOrdinal("SenhaHash"));
                var ativo = reader.GetBoolean(reader.GetOrdinal("Ativo"));
                var tentativasFalhas = reader.GetInt32(reader.GetOrdinal("TentativasFalhas"));
                var bloqueadoOrdinal = reader.GetOrdinal("BloqueadoAteUtc");
                var bloqueadoAteUtc = reader.IsDBNull(bloqueadoOrdinal)
                    ? (DateTime?)null
                    : reader.GetDateTime(bloqueadoOrdinal);
                var versaoCredencial = reader.GetInt32(reader.GetOrdinal("VersaoCredencial"));
                var codigoPerfil = reader.GetString(reader.GetOrdinal("CodigoPerfil"));
                var perfilAtivo = reader.GetBoolean(reader.GetOrdinal("PerfilAtivo"));
                var permissaoOrdinal = reader.GetOrdinal("CodigoPermissao");
                var permissoes = new List<string>();

                do
                {
                    if (!reader.IsDBNull(permissaoOrdinal))
                    {
                        permissoes.Add(reader.GetString(permissaoOrdinal));
                    }
                }
                while (await reader.ReadAsync(cancellationToken));

                return new UsuarioAutenticacaoDto(
                    usuarioId,
                    perfilId,
                    nomeCompleto,
                    loginEncontrado,
                    senhaHash,
                    ativo,
                    tentativasFalhas,
                    bloqueadoAteUtc,
                    versaoCredencial,
                    codigoPerfil,
                    perfilAtivo,
                    permissoes);
            },
            cancellationToken);
    }

    public Task<SessaoUsuarioDto?> ObterSessaoValidaAsync(
        int usuarioId,
        int versaoCredencial,
        CancellationToken cancellationToken)
    {
        return ExecutarAsync<SessaoUsuarioDto?>(
            async () =>
            {
                await using var connection = await AbrirConexaoAsync(cancellationToken);
                using var command = CriarComando(connection, SqlAutenticacao.ObterSessaoValida);
                command.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                command.Parameters.Add("@VersaoCredencial", SqlDbType.Int).Value = versaoCredencial;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                var id = reader.GetInt32(reader.GetOrdinal("UsuarioId"));
                var nome = reader.GetString(reader.GetOrdinal("NomeCompleto"));
                var versao = reader.GetInt32(reader.GetOrdinal("VersaoCredencial"));
                var perfil = reader.GetString(reader.GetOrdinal("CodigoPerfil"));
                var permissaoOrdinal = reader.GetOrdinal("CodigoPermissao");
                var permissoes = new List<string>();

                do
                {
                    if (!reader.IsDBNull(permissaoOrdinal))
                    {
                        permissoes.Add(reader.GetString(permissaoOrdinal));
                    }
                }
                while (await reader.ReadAsync(cancellationToken));

                return new SessaoUsuarioDto(
                    id,
                    nome,
                    perfil,
                    versao,
                    permissoes);
            },
            cancellationToken);
    }
}
