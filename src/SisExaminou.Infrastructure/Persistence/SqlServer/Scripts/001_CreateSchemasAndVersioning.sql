SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
BEGIN
    THROW 51000, N'Execute a migração em um banco de dados da aplicação.', 1;
END;

DECLARE @Migracao001Aplicada bit = 0;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql
        N'
            SELECT @Aplicada =
                CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.MigracaoBanco
                        WHERE Versao = ''001''
                    )
                    THEN 1
                    ELSE 0
                END;
        ',
        N'@Aplicada bit OUTPUT',
        @Aplicada = @Migracao001Aplicada OUTPUT;
END;

IF @Migracao001Aplicada = 1
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'catalogo') IS NULL
    BEGIN
        EXEC sys.sp_executesql N'CREATE SCHEMA [catalogo] AUTHORIZATION [dbo];';
    END;

    IF SCHEMA_ID(N'seguranca') IS NULL
    BEGIN
        EXEC sys.sp_executesql N'CREATE SCHEMA [seguranca] AUTHORIZATION [dbo];';
    END;

    IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MigracaoBanco
        (
            Versao varchar(10) COLLATE Latin1_General_100_CI_AI NOT NULL,
            Descricao nvarchar(200) COLLATE Latin1_General_100_CI_AI NOT NULL,
            AplicadaEmUtc datetime2(0) NOT NULL
                CONSTRAINT DF_MigracaoBanco_AplicadaEmUtc DEFAULT (SYSUTCDATETIME()),

            CONSTRAINT PK_MigracaoBanco
                PRIMARY KEY CLUSTERED (Versao),
            CONSTRAINT CK_MigracaoBanco_Versao_NaoVazia
                CHECK (LEN(LTRIM(RTRIM(Versao))) > 0),
            CONSTRAINT CK_MigracaoBanco_Descricao_NaoVazia
                CHECK (LEN(LTRIM(RTRIM(Descricao))) > 0)
        );
    END;

    EXEC sys.sp_executesql
        N'
            IF NOT EXISTS
            (
                SELECT 1
                FROM dbo.MigracaoBanco WITH (UPDLOCK, HOLDLOCK)
                WHERE Versao = ''001''
            )
            BEGIN
                INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
                VALUES (''001'', N''Criação dos schemas e do controle de migrações'');
            END;
        ';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
