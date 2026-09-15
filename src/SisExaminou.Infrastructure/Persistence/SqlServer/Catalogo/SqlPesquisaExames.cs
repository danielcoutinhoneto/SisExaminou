namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

internal static class SqlPesquisaExames
{
    public const string Comando = """
        DECLARE @Resultados TABLE
        (
            ExameId int NOT NULL PRIMARY KEY,
            Relevancia int NOT NULL
        );

        INSERT INTO @Resultados (ExameId, Relevancia)
        SELECT
            Exame.ExameId,
            CASE
                WHEN @Termo IS NULL THEN 0
                WHEN Exame.Codigo = @Termo THEN 1
                WHEN Exame.Nome = @Termo THEN 2
                WHEN Exame.Nome LIKE @TermoPrefixo ESCAPE N'~' THEN 3
                WHEN Exame.NomePopular = @Termo
                  OR Exame.NomePopular LIKE @TermoPrefixo ESCAPE N'~' THEN 4
                WHEN EXISTS
                (
                    SELECT 1
                    FROM catalogo.ExameSinonimo AS Sinonimo
                    WHERE Sinonimo.ExameId = Exame.ExameId
                      AND Sinonimo.Ativo = 1
                      AND
                      (
                          Sinonimo.Nome = @Termo
                          OR Sinonimo.Nome LIKE @TermoPrefixo ESCAPE N'~'
                      )
                ) THEN 5
                ELSE 6
            END
        FROM catalogo.Exame AS Exame
        INNER JOIN catalogo.TipoExame AS TipoExame
            ON TipoExame.TipoExameId = Exame.TipoExameId
        INNER JOIN catalogo.Categoria AS Categoria
            ON Categoria.CategoriaId = Exame.CategoriaId
        WHERE Exame.Ativo = 1
          AND TipoExame.Ativo = 1
          AND Categoria.Ativo = 1
          AND EXISTS
          (
              SELECT 1
              FROM catalogo.Orientacao AS Orientacao
              WHERE Orientacao.ExameId = Exame.ExameId
                AND Orientacao.Ativo = 1
          )
          AND (@TipoExameId IS NULL OR Exame.TipoExameId = @TipoExameId)
          AND (@CategoriaId IS NULL OR Exame.CategoriaId = @CategoriaId)
          AND
          (
              @Termo IS NULL
              OR Exame.Codigo = @Termo
              OR Exame.Nome = @Termo
              OR Exame.Nome LIKE @TermoPrefixo ESCAPE N'~'
              OR Exame.NomePopular = @Termo
              OR Exame.NomePopular LIKE @TermoPrefixo ESCAPE N'~'
              OR EXISTS
              (
                  SELECT 1
                  FROM catalogo.ExameSinonimo AS Sinonimo
                  WHERE Sinonimo.ExameId = Exame.ExameId
                    AND Sinonimo.Ativo = 1
                    AND
                    (
                        Sinonimo.Nome = @Termo
                        OR Sinonimo.Nome LIKE @TermoPrefixo ESCAPE N'~'
                    )
              )
          );

        SELECT COUNT(*) AS TotalItens
        FROM @Resultados;

        SELECT
            Exame.Codigo,
            Exame.Nome,
            Exame.NomePopular,
            TipoExame.Nome AS TipoExame,
            Categoria.Nome AS Categoria,
            Exame.PrazoResultado
        FROM @Resultados AS Resultado
        INNER JOIN catalogo.Exame AS Exame
            ON Exame.ExameId = Resultado.ExameId
        INNER JOIN catalogo.TipoExame AS TipoExame
            ON TipoExame.TipoExameId = Exame.TipoExameId
        INNER JOIN catalogo.Categoria AS Categoria
            ON Categoria.CategoriaId = Exame.CategoriaId
        ORDER BY
            Resultado.Relevancia,
            Exame.Nome,
            Exame.Codigo
        OFFSET @Offset ROWS
        FETCH NEXT @TamanhoPagina ROWS ONLY;
        """;
}
