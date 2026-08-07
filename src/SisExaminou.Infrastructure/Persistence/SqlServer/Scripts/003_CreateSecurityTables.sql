SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM dbo.MigracaoBanco
       WHERE Versao = '002'
   )
BEGIN
    THROW 51002, N'Execute primeiro as migrações 001 e 002.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.MigracaoBanco
    WHERE Versao = '003'
)
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE seguranca.Perfil
    (
        PerfilId int IDENTITY(1, 1) NOT NULL,
        Codigo nvarchar(50) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Nome nvarchar(80) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Descricao nvarchar(250) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Perfil_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Perfil_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Perfil
            PRIMARY KEY CLUSTERED (PerfilId),
        CONSTRAINT UQ_Perfil_Codigo
            UNIQUE (Codigo),
        CONSTRAINT UQ_Perfil_Nome
            UNIQUE (Nome),
        CONSTRAINT CK_Perfil_Codigo_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
        CONSTRAINT CK_Perfil_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_Perfil_Descricao_NaoVazia
            CHECK (Descricao IS NULL OR LEN(LTRIM(RTRIM(Descricao))) > 0)
    );

    CREATE TABLE seguranca.Permissao
    (
        PermissaoId int IDENTITY(1, 1) NOT NULL,
        Codigo nvarchar(80) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Nome nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Descricao nvarchar(250) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Permissao_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Permissao_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Permissao
            PRIMARY KEY CLUSTERED (PermissaoId),
        CONSTRAINT UQ_Permissao_Codigo
            UNIQUE (Codigo),
        CONSTRAINT CK_Permissao_Codigo_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
        CONSTRAINT CK_Permissao_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_Permissao_Descricao_NaoVazia
            CHECK (Descricao IS NULL OR LEN(LTRIM(RTRIM(Descricao))) > 0)
    );

    CREATE TABLE seguranca.PerfilPermissao
    (
        PerfilId int NOT NULL,
        PermissaoId int NOT NULL,
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_PerfilPermissao_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_PerfilPermissao
            PRIMARY KEY CLUSTERED (PerfilId, PermissaoId),
        CONSTRAINT FK_PerfilPermissao_Perfil
            FOREIGN KEY (PerfilId)
            REFERENCES seguranca.Perfil (PerfilId),
        CONSTRAINT FK_PerfilPermissao_Permissao
            FOREIGN KEY (PermissaoId)
            REFERENCES seguranca.Permissao (PermissaoId)
    );

    CREATE TABLE seguranca.Usuario
    (
        UsuarioId int IDENTITY(1, 1) NOT NULL,
        PerfilId int NOT NULL,
        NomeCompleto nvarchar(150) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Login nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Email nvarchar(254) COLLATE Latin1_General_100_CI_AI NULL,
        SenhaHash nvarchar(512) COLLATE Latin1_General_100_BIN2 NOT NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Usuario_Ativo DEFAULT (1),
        TentativasFalhas int NOT NULL
            CONSTRAINT DF_Usuario_TentativasFalhas DEFAULT (0),
        BloqueadoAteUtc datetime2(0) NULL,
        VersaoCredencial int NOT NULL
            CONSTRAINT DF_Usuario_VersaoCredencial DEFAULT (1),
        UltimoAcessoEmUtc datetime2(0) NULL,
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Usuario_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Usuario
            PRIMARY KEY CLUSTERED (UsuarioId),
        CONSTRAINT FK_Usuario_Perfil
            FOREIGN KEY (PerfilId)
            REFERENCES seguranca.Perfil (PerfilId),
        CONSTRAINT UQ_Usuario_Login
            UNIQUE (Login),
        CONSTRAINT CK_Usuario_NomeCompleto_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(NomeCompleto))) > 0),
        CONSTRAINT CK_Usuario_Login_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Login))) > 0),
        CONSTRAINT CK_Usuario_Email_NaoVazio
            CHECK (Email IS NULL OR LEN(LTRIM(RTRIM(Email))) > 0),
        CONSTRAINT CK_Usuario_SenhaHash_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(SenhaHash))) > 0),
        CONSTRAINT CK_Usuario_TentativasFalhas_NaoNegativa
            CHECK (TentativasFalhas >= 0),
        CONSTRAINT CK_Usuario_VersaoCredencial_Positiva
            CHECK (VersaoCredencial >= 1)
    );

    INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
    VALUES ('003', N'Criação das tabelas de segurança');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
