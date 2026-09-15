namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

internal static class SqlFiltrosCatalogo
{
    public const string Comando = """
        SELECT
            TipoExame.TipoExameId AS Id,
            TipoExame.Nome
        FROM catalogo.TipoExame AS TipoExame
        WHERE TipoExame.Ativo = 1
          AND EXISTS
          (
              SELECT 1
              FROM catalogo.Exame AS Exame
              INNER JOIN catalogo.Categoria AS Categoria
                  ON Categoria.CategoriaId = Exame.CategoriaId
              WHERE Exame.TipoExameId = TipoExame.TipoExameId
                AND Exame.Ativo = 1
                AND Categoria.Ativo = 1
                AND EXISTS
                (
                    SELECT 1
                    FROM catalogo.Orientacao AS Orientacao
                    WHERE Orientacao.ExameId = Exame.ExameId
                      AND Orientacao.Ativo = 1
                )
          )
        ORDER BY TipoExame.Nome;

        SELECT
            Categoria.CategoriaId AS Id,
            Categoria.Nome
        FROM catalogo.Categoria AS Categoria
        WHERE Categoria.Ativo = 1
          AND EXISTS
          (
              SELECT 1
              FROM catalogo.Exame AS Exame
              INNER JOIN catalogo.TipoExame AS TipoExame
                  ON TipoExame.TipoExameId = Exame.TipoExameId
              WHERE Exame.CategoriaId = Categoria.CategoriaId
                AND Exame.Ativo = 1
                AND TipoExame.Ativo = 1
                AND EXISTS
                (
                    SELECT 1
                    FROM catalogo.Orientacao AS Orientacao
                    WHERE Orientacao.ExameId = Exame.ExameId
                      AND Orientacao.Ativo = 1
                )
          )
        ORDER BY Categoria.Nome;
        """;
}
