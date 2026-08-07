SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM dbo.MigracaoBanco
       WHERE Versao = '001'
   )
BEGIN
    THROW 51001, N'Execute primeiro a migração 001.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.MigracaoBanco
    WHERE Versao = '002'
)
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE catalogo.TipoExame
    (
        TipoExameId int IDENTITY(1, 1) NOT NULL,
        Nome nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Descricao nvarchar(300) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_TipoExame_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_TipoExame_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_TipoExame
            PRIMARY KEY CLUSTERED (TipoExameId),
        CONSTRAINT UQ_TipoExame_Nome
            UNIQUE (Nome),
        CONSTRAINT CK_TipoExame_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_TipoExame_Descricao_NaoVazia
            CHECK (Descricao IS NULL OR LEN(LTRIM(RTRIM(Descricao))) > 0)
    );

    CREATE TABLE catalogo.Categoria
    (
        CategoriaId int IDENTITY(1, 1) NOT NULL,
        Nome nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Descricao nvarchar(300) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Categoria_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Categoria_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Categoria
            PRIMARY KEY CLUSTERED (CategoriaId),
        CONSTRAINT UQ_Categoria_Nome
            UNIQUE (Nome),
        CONSTRAINT CK_Categoria_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_Categoria_Descricao_NaoVazia
            CHECK (Descricao IS NULL OR LEN(LTRIM(RTRIM(Descricao))) > 0)
    );

    CREATE TABLE catalogo.Exame
    (
        ExameId int IDENTITY(1, 1) NOT NULL,
        TipoExameId int NOT NULL,
        CategoriaId int NOT NULL,
        Codigo nvarchar(50) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Nome nvarchar(200) COLLATE Latin1_General_100_CI_AI NOT NULL,
        NomePopular nvarchar(200) COLLATE Latin1_General_100_CI_AI NULL,
        DescricaoPopular nvarchar(1000) COLLATE Latin1_General_100_CI_AI NULL,
        PrazoResultado nvarchar(300) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Exame_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Exame_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Exame
            PRIMARY KEY CLUSTERED (ExameId),
        CONSTRAINT FK_Exame_TipoExame
            FOREIGN KEY (TipoExameId)
            REFERENCES catalogo.TipoExame (TipoExameId),
        CONSTRAINT FK_Exame_Categoria
            FOREIGN KEY (CategoriaId)
            REFERENCES catalogo.Categoria (CategoriaId),
        CONSTRAINT UQ_Exame_Codigo
            UNIQUE (Codigo),
        CONSTRAINT UQ_Exame_CategoriaId_Nome
            UNIQUE (CategoriaId, Nome),
        CONSTRAINT CK_Exame_Codigo_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
        CONSTRAINT CK_Exame_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_Exame_NomePopular_NaoVazio
            CHECK (NomePopular IS NULL OR LEN(LTRIM(RTRIM(NomePopular))) > 0),
        CONSTRAINT CK_Exame_DescricaoPopular_NaoVazia
            CHECK (DescricaoPopular IS NULL OR LEN(LTRIM(RTRIM(DescricaoPopular))) > 0),
        CONSTRAINT CK_Exame_PrazoResultado_NaoVazio
            CHECK (PrazoResultado IS NULL OR LEN(LTRIM(RTRIM(PrazoResultado))) > 0)
    );

    CREATE TABLE catalogo.ExameSinonimo
    (
        ExameSinonimoId int IDENTITY(1, 1) NOT NULL,
        ExameId int NOT NULL,
        Nome nvarchar(200) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_ExameSinonimo_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_ExameSinonimo_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_ExameSinonimo
            PRIMARY KEY CLUSTERED (ExameSinonimoId),
        CONSTRAINT FK_ExameSinonimo_Exame
            FOREIGN KEY (ExameId)
            REFERENCES catalogo.Exame (ExameId),
        CONSTRAINT UQ_ExameSinonimo_ExameId_Nome
            UNIQUE (ExameId, Nome),
        CONSTRAINT CK_ExameSinonimo_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0)
    );

    CREATE TABLE catalogo.Orientacao
    (
        OrientacaoId int IDENTITY(1, 1) NOT NULL,
        ExameId int NOT NULL,
        Titulo nvarchar(150) COLLATE Latin1_General_100_CI_AI NOT NULL,
        TipoOrientacao nvarchar(30) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Conteudo nvarchar(max) COLLATE Latin1_General_100_CI_AI NOT NULL,
        OrdemExibicao int NOT NULL
            CONSTRAINT DF_Orientacao_OrdemExibicao DEFAULT (1),
        Ativo bit NOT NULL
            CONSTRAINT DF_Orientacao_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Orientacao_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Orientacao
            PRIMARY KEY CLUSTERED (OrientacaoId),
        CONSTRAINT FK_Orientacao_Exame
            FOREIGN KEY (ExameId)
            REFERENCES catalogo.Exame (ExameId),
        CONSTRAINT UQ_Orientacao_ExameId_OrdemExibicao
            UNIQUE (ExameId, OrdemExibicao),
        CONSTRAINT CK_Orientacao_Titulo_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0),
        CONSTRAINT CK_Orientacao_TipoOrientacao_Valido
            CHECK
            (
                TipoOrientacao IN
                (
                    N'PREPARO',
                    N'COLETA',
                    N'RESTRICAO',
                    N'INFORMACAO'
                )
            ),
        CONSTRAINT CK_Orientacao_Conteudo_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Conteudo))) > 0),
        CONSTRAINT CK_Orientacao_OrdemExibicao_Positiva
            CHECK (OrdemExibicao > 0)
    );

    CREATE TABLE catalogo.Material
    (
        MaterialId int IDENTITY(1, 1) NOT NULL,
        Nome nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Sigla nvarchar(20) COLLATE Latin1_General_100_CI_AI NULL,
        Descricao nvarchar(300) COLLATE Latin1_General_100_CI_AI NULL,
        Ativo bit NOT NULL
            CONSTRAINT DF_Material_Ativo DEFAULT (1),
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_Material_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),
        AtualizadoEmUtc datetime2(0) NULL,
        Versao rowversion NOT NULL,

        CONSTRAINT PK_Material
            PRIMARY KEY CLUSTERED (MaterialId),
        CONSTRAINT UQ_Material_Nome
            UNIQUE (Nome),
        CONSTRAINT CK_Material_Nome_NaoVazio
            CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),
        CONSTRAINT CK_Material_Sigla_NaoVazia
            CHECK (Sigla IS NULL OR LEN(LTRIM(RTRIM(Sigla))) > 0),
        CONSTRAINT CK_Material_Descricao_NaoVazia
            CHECK (Descricao IS NULL OR LEN(LTRIM(RTRIM(Descricao))) > 0)
    );

    CREATE TABLE catalogo.ExameMaterial
    (
        ExameId int NOT NULL,
        MaterialId int NOT NULL,
        Principal bit NOT NULL
            CONSTRAINT DF_ExameMaterial_Principal DEFAULT (0),
        Observacao nvarchar(300) COLLATE Latin1_General_100_CI_AI NULL,
        CriadoEmUtc datetime2(0) NOT NULL
            CONSTRAINT DF_ExameMaterial_CriadoEmUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ExameMaterial
            PRIMARY KEY CLUSTERED (ExameId, MaterialId),
        CONSTRAINT FK_ExameMaterial_Exame
            FOREIGN KEY (ExameId)
            REFERENCES catalogo.Exame (ExameId),
        CONSTRAINT FK_ExameMaterial_Material
            FOREIGN KEY (MaterialId)
            REFERENCES catalogo.Material (MaterialId),
        CONSTRAINT CK_ExameMaterial_Observacao_NaoVazia
            CHECK (Observacao IS NULL OR LEN(LTRIM(RTRIM(Observacao))) > 0)
    );

    INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
    VALUES ('002', N'Criação das tabelas do catálogo');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
