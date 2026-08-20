SET NOCOUNT ON;
  SET XACT_ABORT ON;

  IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
     OR NOT EXISTS
     (
         SELECT 1
         FROM dbo.MigracaoBanco
         WHERE Versao = '006'
     )
  BEGIN
      THROW 51007, N'Execute primeiro as migrações 001 a 006.', 1;
  END;

  IF EXISTS
  (
      SELECT 1
      FROM dbo.MigracaoBanco
      WHERE Versao = '007'
  )
  BEGIN
      RETURN;
  END;

  BEGIN TRY
      BEGIN TRANSACTION;

      IF EXISTS
      (
          SELECT 1
          FROM catalogo.Exame
          WHERE LEN(Codigo) NOT BETWEEN 2 AND 50
             OR (Codigo COLLATE Latin1_General_100_BIN2)
                 LIKE N'%[^A-Z0-9-]%'
      )
      BEGIN
          THROW 51008, N'Existem exames com código incompatível com o formato URL-safe.', 1;
      END;

      ALTER TABLE catalogo.Exame WITH CHECK
      ADD CONSTRAINT CK_Exame_Codigo_Formato
      CHECK
      (
          LEN(Codigo) BETWEEN 2 AND 50
          AND (Codigo COLLATE Latin1_General_100_BIN2)
              NOT LIKE N'%[^A-Z0-9-]%'
      );

      ALTER TABLE catalogo.Exame
      CHECK CONSTRAINT CK_Exame_Codigo_Formato;

      INSERT INTO dbo.MigracaoBanco
      (
          Versao,
          Descricao
      )
      VALUES
      (
          '007',
          N'Validação do formato URL-safe do código do exame'
      );

      COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
      IF XACT_STATE() <> 0
      BEGIN
          ROLLBACK TRANSACTION;
      END;

      THROW;
  END CATCH;