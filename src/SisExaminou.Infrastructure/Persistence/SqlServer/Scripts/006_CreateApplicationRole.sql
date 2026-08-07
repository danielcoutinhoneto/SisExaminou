SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM dbo.MigracaoBanco
       WHERE Versao = '005'
   )
BEGIN
    THROW 51005, N'Execute primeiro as migrações 001 a 005.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.MigracaoBanco
    WHERE Versao = '006'
)
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF DATABASE_PRINCIPAL_ID(N'SisExaminouAplicacao') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.database_principals
           WHERE principal_id = DATABASE_PRINCIPAL_ID(N'SisExaminouAplicacao')
             AND type = 'R'
       )
    BEGIN
        THROW 51006, N'Já existe um principal chamado SisExaminouAplicacao que não é uma database role.', 1;
    END;

    IF DATABASE_PRINCIPAL_ID(N'SisExaminouAplicacao') IS NULL
    BEGIN
        CREATE ROLE SisExaminouAplicacao AUTHORIZATION dbo;
    END;

    GRANT SELECT ON SCHEMA::catalogo TO SisExaminouAplicacao;
    GRANT SELECT ON SCHEMA::seguranca TO SisExaminouAplicacao;

    GRANT INSERT, UPDATE ON OBJECT::catalogo.TipoExame TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::catalogo.Categoria TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::catalogo.Exame TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::catalogo.ExameSinonimo TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::catalogo.Orientacao TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::catalogo.Material TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE, DELETE ON OBJECT::catalogo.ExameMaterial TO SisExaminouAplicacao;

    GRANT INSERT, UPDATE ON OBJECT::seguranca.Perfil TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE, DELETE ON OBJECT::seguranca.PerfilPermissao TO SisExaminouAplicacao;
    GRANT INSERT, UPDATE ON OBJECT::seguranca.Usuario TO SisExaminouAplicacao;

    INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
    VALUES ('006', N'Criação da database role da aplicação');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
