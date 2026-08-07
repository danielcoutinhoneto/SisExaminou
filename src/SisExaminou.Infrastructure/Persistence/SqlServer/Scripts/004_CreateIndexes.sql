SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM dbo.MigracaoBanco
       WHERE Versao = '003'
   )
BEGIN
    THROW 51003, N'Execute primeiro as migrações 001 a 003.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.MigracaoBanco
    WHERE Versao = '004'
)
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE NONCLUSTERED INDEX IX_Exame_TipoExameId
        ON catalogo.Exame (TipoExameId);

    CREATE NONCLUSTERED INDEX IX_Exame_Ativo_Nome
        ON catalogo.Exame (Nome)
        INCLUDE (Codigo, TipoExameId, CategoriaId, NomePopular, PrazoResultado)
        WHERE Ativo = 1;

    CREATE NONCLUSTERED INDEX IX_Exame_TipoExameId_Ativo_Nome
        ON catalogo.Exame (TipoExameId, Nome)
        INCLUDE (Codigo, CategoriaId, NomePopular, PrazoResultado)
        WHERE Ativo = 1;

    CREATE NONCLUSTERED INDEX IX_Exame_CategoriaId_Ativo_Nome
        ON catalogo.Exame (CategoriaId, Nome)
        INCLUDE (Codigo, TipoExameId, NomePopular, PrazoResultado)
        WHERE Ativo = 1;

    CREATE NONCLUSTERED INDEX IX_ExameSinonimo_Ativo_Nome
        ON catalogo.ExameSinonimo (Nome, ExameId)
        WHERE Ativo = 1;

    CREATE NONCLUSTERED INDEX IX_Orientacao_ExameId_Ativo_OrdemExibicao
        ON catalogo.Orientacao (ExameId, Ativo, OrdemExibicao);

    CREATE UNIQUE NONCLUSTERED INDEX UQ_Material_Sigla
        ON catalogo.Material (Sigla)
        WHERE Sigla IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_ExameMaterial_MaterialId
        ON catalogo.ExameMaterial (MaterialId);

    CREATE UNIQUE NONCLUSTERED INDEX UQ_ExameMaterial_ExameId_Principal
        ON catalogo.ExameMaterial (ExameId)
        WHERE Principal = 1;

    CREATE NONCLUSTERED INDEX IX_PerfilPermissao_PermissaoId
        ON seguranca.PerfilPermissao (PermissaoId);

    CREATE UNIQUE NONCLUSTERED INDEX UQ_Usuario_Email
        ON seguranca.Usuario (Email)
        WHERE Email IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_Usuario_PerfilId_Ativo
        ON seguranca.Usuario (PerfilId, Ativo);

    INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
    VALUES ('004', N'Criação dos índices de consulta e integridade');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
