namespace SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

internal static class SqlDetalheExame
{
    public const string Comando = """
        SELECT
            Exame.Codigo,
            Exame.Nome,
            Exame.NomePopular,
            Exame.DescricaoPopular,
            Exame.PrazoResultado,
            TipoExame.Nome AS TipoExame,
            Categoria.Nome AS Categoria
        FROM catalogo.Exame AS Exame
        INNER JOIN catalogo.TipoExame AS TipoExame
            ON TipoExame.TipoExameId = Exame.TipoExameId
        INNER JOIN catalogo.Categoria AS Categoria
            ON Categoria.CategoriaId = Exame.CategoriaId
        WHERE Exame.Codigo = @Codigo
          AND Exame.Ativo = 1
          AND TipoExame.Ativo = 1
          AND Categoria.Ativo = 1
          AND EXISTS
          (
              SELECT 1
              FROM catalogo.Orientacao AS Orientacao
              WHERE Orientacao.ExameId = Exame.ExameId
                AND Orientacao.Ativo = 1
          );

        SELECT Sinonimo.Nome
        FROM catalogo.ExameSinonimo AS Sinonimo
        INNER JOIN catalogo.Exame AS Exame
            ON Exame.ExameId = Sinonimo.ExameId
        WHERE Exame.Codigo = @Codigo
          AND Sinonimo.Ativo = 1
        ORDER BY Sinonimo.Nome;

        SELECT
            Orientacao.Titulo,
            Orientacao.TipoOrientacao AS Tipo,
            Orientacao.Conteudo,
            Orientacao.OrdemExibicao
        FROM catalogo.Orientacao AS Orientacao
        INNER JOIN catalogo.Exame AS Exame
            ON Exame.ExameId = Orientacao.ExameId
        WHERE Exame.Codigo = @Codigo
          AND Orientacao.Ativo = 1
        ORDER BY Orientacao.OrdemExibicao;

        SELECT
            Material.Nome,
            Material.Sigla,
            ExameMaterial.Observacao,
            ExameMaterial.Principal
        FROM catalogo.ExameMaterial AS ExameMaterial
        INNER JOIN catalogo.Exame AS Exame
            ON Exame.ExameId = ExameMaterial.ExameId
        INNER JOIN catalogo.Material AS Material
            ON Material.MaterialId = ExameMaterial.MaterialId
        WHERE Exame.Codigo = @Codigo
          AND Material.Ativo = 1
        ORDER BY ExameMaterial.Principal DESC, Material.Nome;
        """;
}
