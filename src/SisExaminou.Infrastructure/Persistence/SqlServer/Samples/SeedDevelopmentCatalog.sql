/*
      CARGA EXCLUSIVA PARA DESENVOLVIMENTO

      Este arquivo:
      - não é uma migração;
      - não deve ser executado em produção;
      - não cria usuários ou credenciais;
      - utiliza somente informações sintéticas;
      - pode ser reexecutado sem duplicar registros.

      Antes de executar, habilite a carga na mesma conexão:

      EXEC sys.sp_set_session_context
          @key = N'PermitirCargaDemonstrativa',
          @value = 1;
  */

  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  IF DB_NAME() <> N'SisExaminouDB'
  BEGIN
      THROW 51100,
          N'Execute a carga demonstrativa no banco SisExaminouDB.',
          1;
  END;

  IF ISNULL
  (
      TRY_CONVERT
      (
          bit,
          SESSION_CONTEXT(N'PermitirCargaDemonstrativa')
      ),
      0
  ) <> 1
  BEGIN
      THROW 51101,
          N'A carga demonstrativa não foi autorizada nesta conexão.',
          1;
  END;

  IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
     OR NOT EXISTS
     (
         SELECT 1
         FROM dbo.MigracaoBanco
         WHERE Versao = '007'
     )
  BEGIN
      THROW 51102,
          N'Execute primeiro as migrações 001 a 007.',
          1;
  END;

  BEGIN TRY
      BEGIN TRANSACTION;

      /* =========================================================
         1. Tipos de exame
         ========================================================= */

      DECLARE @Tipos TABLE
      (
          Nome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
              PRIMARY KEY,

          Descricao nvarchar(300)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
      );

      INSERT INTO @Tipos
      (
          Nome,
          Descricao
      )
      VALUES
      (
          N'[DEMO] Laboratorial',
          N'Tipo sintético utilizado exclusivamente em desenvolvimento.'
      ),
      (
          N'[DEMO] Imagem',
          N'Tipo sintético utilizado exclusivamente em desenvolvimento.'
      );

      INSERT INTO catalogo.TipoExame
      (
          Nome,
          Descricao
      )
      SELECT
          Origem.Nome,
          Origem.Descricao
      FROM @Tipos AS Origem
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.TipoExame AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.Nome = Origem.Nome
      );

      /* =========================================================
         2. Categorias
         ========================================================= */

      DECLARE @Categorias TABLE
      (
          Nome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
              PRIMARY KEY,

          Descricao nvarchar(300)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
      );

      INSERT INTO @Categorias
      (
          Nome,
          Descricao
      )
      VALUES
      (
          N'[DEMO] Rotina',
          N'Categoria sintética utilizada exclusivamente em desenvolvimento.'
      ),
      (
          N'[DEMO] Especial',
          N'Categoria sintética utilizada exclusivamente em desenvolvimento.'
      );

      INSERT INTO catalogo.Categoria
      (
          Nome,
          Descricao
      )
      SELECT
          Origem.Nome,
          Origem.Descricao
      FROM @Categorias AS Origem
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.Categoria AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.Nome = Origem.Nome
      );

      /* =========================================================
         3. Materiais
         ========================================================= */

      DECLARE @Materiais TABLE
      (
          Nome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
              PRIMARY KEY,

          Sigla nvarchar(20)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Descricao nvarchar(300)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
      );

      INSERT INTO @Materiais
      (
          Nome,
          Sigla,
          Descricao
      )
      VALUES
      (
          N'[DEMO] Amostra Alfa',
          N'D-ALFA',
          N'Material sintético sem validade clínica.'
      ),
      (
          N'[DEMO] Amostra Beta',
          N'D-BETA',
          N'Material sintético sem validade clínica.'
      ),
      (
          N'[DEMO] Amostra Gama',
          N'D-GAMA',
          N'Material sintético sem validade clínica.'
      );

      INSERT INTO catalogo.Material
      (
          Nome,
          Sigla,
          Descricao
      )
      SELECT
          Origem.Nome,
          Origem.Sigla,
          Origem.Descricao
      FROM @Materiais AS Origem
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.Material AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.Nome = Origem.Nome
      );

      /* =========================================================
         4. Exames
         ========================================================= */

      DECLARE @Exames TABLE
      (
          Codigo nvarchar(50)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL
              PRIMARY KEY,

          TipoExameNome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          CategoriaNome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Nome nvarchar(200)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          NomePopular nvarchar(200)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          DescricaoPopular nvarchar(1000)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          PrazoResultado nvarchar(300)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Ativo bit NOT NULL
      );

      INSERT INTO @Exames
      (
          Codigo,
          TipoExameNome,
          CategoriaNome,
          Nome,
          NomePopular,
          DescricaoPopular,
          PrazoResultado,
          Ativo
      )
      VALUES
      (
          N'DEMO-ALFA',
          N'[DEMO] Laboratorial',
          N'[DEMO] Rotina',
          N'[DEMO] Exame Alfa',
          N'[DEMO] Alfa',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-BETA',
          N'[DEMO] Laboratorial',
          N'[DEMO] Rotina',
          N'[DEMO] Exame Beta',
          N'[DEMO] Beta',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-GAMA',
          N'[DEMO] Laboratorial',
          N'[DEMO] Especial',
          N'[DEMO] Exame Gama',
          N'[DEMO] Gama',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-DELTA',
          N'[DEMO] Imagem',
          N'[DEMO] Rotina',
          N'[DEMO] Exame Delta',
          N'[DEMO] Delta',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-EPSILON',
          N'[DEMO] Imagem',
          N'[DEMO] Especial',
          N'[DEMO] Exame Epsilon',
          N'[DEMO] Epsilon',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-ZETA',
          N'[DEMO] Imagem',
          N'[DEMO] Especial',
          N'[DEMO] Exame Zeta',
          N'[DEMO] Zeta',
          N'Registro sintético utilizado para demonstrar a consulta pública.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      ),
      (
          N'DEMO-INATIVO',
          N'[DEMO] Laboratorial',
          N'[DEMO] Rotina',
          N'[DEMO] Exame Inativo',
          N'[DEMO] Inativo',
          N'Registro sintético usado para validar a inativação.',
          N'Prazo demonstrativo sem validade operacional.',
          0
      ),
      (
          N'DEMO-SEM-ORIENTACAO',
          N'[DEMO] Imagem',
          N'[DEMO] Especial',
          N'[DEMO] Exame sem orientação ativa',
          N'[DEMO] Sem orientação',
          N'Registro sintético usado para validar a regra de publicação.',
          N'Prazo demonstrativo sem validade operacional.',
          1
      );

      INSERT INTO catalogo.Exame
      (
          TipoExameId,
          CategoriaId,
          Codigo,
          Nome,
          NomePopular,
          DescricaoPopular,
          PrazoResultado,
          Ativo
      )
      SELECT
          TipoExame.TipoExameId,
          Categoria.CategoriaId,
          Origem.Codigo,
          Origem.Nome,
          Origem.NomePopular,
          Origem.DescricaoPopular,
          Origem.PrazoResultado,
          Origem.Ativo
      FROM @Exames AS Origem
      INNER JOIN catalogo.TipoExame AS TipoExame
          ON TipoExame.Nome = Origem.TipoExameNome
      INNER JOIN catalogo.Categoria AS Categoria
          ON Categoria.Nome = Origem.CategoriaNome
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.Exame AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.Codigo = Origem.Codigo
      );

      /* =========================================================
         5. Sinônimos
         ========================================================= */

      DECLARE @Sinonimos TABLE
      (
          CodigoExame nvarchar(50)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Nome nvarchar(200)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Ativo bit NOT NULL,

          PRIMARY KEY (CodigoExame, Nome)
      );

      INSERT INTO @Sinonimos
      (
          CodigoExame,
          Nome,
          Ativo
      )
      VALUES
      (
          N'DEMO-ALFA',
          N'[DEMO] Alfa alternativo',
          1
      ),
      (
          N'DEMO-ALFA',
          N'[DEMO] Termo comum',
          1
      ),
      (
          N'DEMO-ALFA',
          N'[DEMO] Sinônimo inativo',
          0
      ),
      (
          N'DEMO-BETA',
          N'[DEMO] Beta alternativo',
          1
      ),
      (
          N'DEMO-BETA',
          N'[DEMO] Termo comum',
          1
      ),
      (
          N'DEMO-GAMA',
          N'[DEMO] Gama alternativo',
          1
      ),
      (
          N'DEMO-DELTA',
          N'[DEMO] Delta alternativo',
          1
      ),
      (
          N'DEMO-EPSILON',
          N'[DEMO] Epsilon alternativo',
          1
      ),
      (
          N'DEMO-ZETA',
          N'[DEMO] Zeta alternativo',
          1
      );

      INSERT INTO catalogo.ExameSinonimo
      (
          ExameId,
          Nome,
          Ativo
      )
      SELECT
          Exame.ExameId,
          Origem.Nome,
          Origem.Ativo
      FROM @Sinonimos AS Origem
      INNER JOIN catalogo.Exame AS Exame
          ON Exame.Codigo = Origem.CodigoExame
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.ExameSinonimo AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.ExameId = Exame.ExameId
            AND Destino.Nome = Origem.Nome
      );

      /* =========================================================
         6. Orientações
         ========================================================= */

       DECLARE @Orientacoes TABLE
      (
          CodigoExame nvarchar(50)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Titulo nvarchar(150)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          TipoOrientacao nvarchar(30)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Conteudo nvarchar(max)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          OrdemExibicao int NOT NULL,
          Ativo bit NOT NULL,

          PRIMARY KEY (CodigoExame, OrdemExibicao)
      );

      INSERT INTO @Orientacoes
      (
          CodigoExame,
          Titulo,
          TipoOrientacao,
          Conteudo,
          OrdemExibicao,
          Ativo
      )
      VALUES
      (
          N'DEMO-ALFA',
          N'[DEMO] Preparação',
          N'PREPARO',
          N'Conteúdo exclusivamente demonstrativo. Não utilizar como orientação médica.',
          1,
          1
      ),
      (
          N'DEMO-ALFA',
          N'[DEMO] Informação complementar',
          N'INFORMACAO',
          N'Informação sintética sem validade clínica ou operacional.',
          2,
          1
      ),
      (
          N'DEMO-BETA',
          N'[DEMO] Coleta',
          N'COLETA',
          N'Conteúdo exclusivamente demonstrativo. Não utilizar como orientação médica.',
          1,
          1
      ),
      (
          N'DEMO-GAMA',
          N'[DEMO] Restrição',
          N'RESTRICAO',
          N'Conteúdo exclusivamente demonstrativo. Não utilizar como orientação médica.',
          1,
          1
      ),
      (
          N'DEMO-DELTA',
          N'[DEMO] Informação',
          N'INFORMACAO',
          N'Informação sintética sem validade clínica ou operacional.',
          1,
          1
      ),
      (
          N'DEMO-DELTA',
          N'[DEMO] Informação adicional',
          N'COLETA',
          N'Conteúdo exclusivamente demonstrativo. Não utilizar como orientação médica.',
          2,
          1
      ),
      (
          N'DEMO-EPSILON',
          N'[DEMO] Preparação',
          N'PREPARO',
          N'Conteúdo exclusivamente demonstrativo. Não utilizar como orientação médica.',
          1,
          1
      ),
      (
          N'DEMO-ZETA',
          N'[DEMO] Informação',
          N'INFORMACAO',
          N'Informação sintética sem validade clínica ou operacional.',
          1,
          1
      ),
      (
          N'DEMO-INATIVO',
          N'[DEMO] Informação do exame inativo',
          N'INFORMACAO',
          N'Orientação sintética vinculada a um exame inativo.',
          1,
          1
      ),
      (
          N'DEMO-SEM-ORIENTACAO',
          N'[DEMO] Orientação inativa',
          N'INFORMACAO',
          N'Orientação sintética inativa para validar a regra de publicação.',
          1,
          0
      );

      INSERT INTO catalogo.Orientacao
      (
          ExameId,
          Titulo,
          TipoOrientacao,
          Conteudo,
          OrdemExibicao,
          Ativo
      )
      SELECT
          Exame.ExameId,
          Origem.Titulo,
          Origem.TipoOrientacao,
          Origem.Conteudo,
          Origem.OrdemExibicao,
          Origem.Ativo
      FROM @Orientacoes AS Origem
      INNER JOIN catalogo.Exame AS Exame
          ON Exame.Codigo = Origem.CodigoExame
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.Orientacao AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.ExameId = Exame.ExameId
            AND Destino.OrdemExibicao = Origem.OrdemExibicao
      );

      /* =========================================================
         7. Associação entre exames e materiais
         ========================================================= */

      DECLARE @ExameMateriais TABLE
      (
          CodigoExame nvarchar(50)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          MaterialNome nvarchar(100)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          Principal bit NOT NULL,

          Observacao nvarchar(300)
              COLLATE Latin1_General_100_CI_AI
              NOT NULL,

          PRIMARY KEY (CodigoExame, MaterialNome)
      );

      INSERT INTO @ExameMateriais
      (
          CodigoExame,
          MaterialNome,
          Principal,
          Observacao
      )
      VALUES
      (
          N'DEMO-ALFA',
          N'[DEMO] Amostra Alfa',
          1,
          N'Material principal demonstrativo.'
      ),
      (
          N'DEMO-ALFA',
          N'[DEMO] Amostra Beta',
          0,
          N'Material complementar demonstrativo.'
      ),
      (
          N'DEMO-BETA',
          N'[DEMO] Amostra Beta',
          1,
          N'Material principal demonstrativo.'
      ),
      (
          N'DEMO-GAMA',
          N'[DEMO] Amostra Gama',
          1,
          N'Material principal demonstrativo.'
      ),
      (
          N'DEMO-DELTA',
          N'[DEMO] Amostra Alfa',
          1,
          N'Material principal demonstrativo.'
      ),
      (
          N'DEMO-EPSILON',
          N'[DEMO] Amostra Beta',
          1,
          N'Material principal demonstrativo.'
      ),
      (
          N'DEMO-EPSILON',
          N'[DEMO] Amostra Gama',
          0,
          N'Material complementar demonstrativo.'
      ),
      (
          N'DEMO-ZETA',
          N'[DEMO] Amostra Gama',
          1,
          N'Material principal demonstrativo.'
      );

      INSERT INTO catalogo.ExameMaterial
      (
          ExameId,
          MaterialId,
          Principal,
          Observacao
      )
      SELECT
          Exame.ExameId,
          Material.MaterialId,
          Origem.Principal,
          Origem.Observacao
      FROM @ExameMateriais AS Origem
      INNER JOIN catalogo.Exame AS Exame
          ON Exame.Codigo = Origem.CodigoExame
      INNER JOIN catalogo.Material AS Material
          ON Material.Nome = Origem.MaterialNome
      WHERE NOT EXISTS
      (
          SELECT 1
          FROM catalogo.ExameMaterial AS Destino
              WITH (UPDLOCK, HOLDLOCK)
          WHERE Destino.ExameId = Exame.ExameId
            AND Destino.MaterialId = Material.MaterialId
      );

      /* =========================================================
         8. Validação da carga
         ========================================================= */

      DECLARE @QuantidadeTipos int =
      (
          SELECT COUNT(*)
          FROM @Tipos AS Esperado
          INNER JOIN catalogo.TipoExame AS Encontrado
              ON Encontrado.Nome = Esperado.Nome
      );

      DECLARE @QuantidadeCategorias int =
      (
          SELECT COUNT(*)
          FROM @Categorias AS Esperado
          INNER JOIN catalogo.Categoria AS Encontrado
              ON Encontrado.Nome = Esperado.Nome
      );

      DECLARE @QuantidadeMateriais int =
      (
          SELECT COUNT(*)
          FROM @Materiais AS Esperado
          INNER JOIN catalogo.Material AS Encontrado
              ON Encontrado.Nome = Esperado.Nome
      );

      DECLARE @QuantidadeExames int =
      (
          SELECT COUNT(*)
          FROM @Exames AS Esperado
          INNER JOIN catalogo.Exame AS Encontrado
              ON Encontrado.Codigo = Esperado.Codigo
      );

      DECLARE @QuantidadePublicaveis int =
      (
          SELECT COUNT(*)
          FROM @Exames AS Esperado
          INNER JOIN catalogo.Exame AS Exame
              ON Exame.Codigo = Esperado.Codigo
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
      );

      DECLARE @QuantidadeInativos int =
      (
          SELECT COUNT(*)
          FROM @Exames AS Esperado
          INNER JOIN catalogo.Exame AS Exame
              ON Exame.Codigo = Esperado.Codigo
          WHERE Exame.Ativo = 0
      );

      DECLARE @QuantidadeAtivosSemOrientacao int =
      (
          SELECT COUNT(*)
          FROM @Exames AS Esperado
          INNER JOIN catalogo.Exame AS Exame
              ON Exame.Codigo = Esperado.Codigo
          WHERE Exame.Ativo = 1
            AND NOT EXISTS
            (
                SELECT 1
                FROM catalogo.Orientacao AS Orientacao
                WHERE Orientacao.ExameId = Exame.ExameId
                  AND Orientacao.Ativo = 1
            )
      );

      IF @QuantidadeTipos <> 2
         OR @QuantidadeCategorias <> 2
         OR @QuantidadeMateriais <> 3
         OR @QuantidadeExames <> 8
         OR @QuantidadePublicaveis <> 6
         OR @QuantidadeInativos <> 1
         OR @QuantidadeAtivosSemOrientacao <> 1
      BEGIN
          THROW 51103,
              N'A carga demonstrativa não atingiu as contagens esperadas.',
              1;
      END;

      COMMIT TRANSACTION;

      EXEC sys.sp_set_session_context
          @key = N'PermitirCargaDemonstrativa',
          @value = NULL;

      SELECT
          @QuantidadeTipos AS Tipos,
          @QuantidadeCategorias AS Categorias,
          @QuantidadeMateriais AS Materiais,
          @QuantidadeExames AS Exames,
          @QuantidadePublicaveis AS ExamesPublicaveis,
          @QuantidadeInativos AS ExamesInativos,
          @QuantidadeAtivosSemOrientacao AS AtivosSemOrientacao;

  END TRY
  BEGIN CATCH
      IF XACT_STATE() <> 0
      BEGIN
          ROLLBACK TRANSACTION;
      END;

      EXEC sys.sp_set_session_context
          @key = N'PermitirCargaDemonstrativa',
          @value = NULL;

      THROW;
  END CATCH;