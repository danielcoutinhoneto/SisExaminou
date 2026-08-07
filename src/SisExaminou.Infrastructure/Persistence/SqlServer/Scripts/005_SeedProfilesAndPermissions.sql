SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.MigracaoBanco', N'U') IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM dbo.MigracaoBanco
       WHERE Versao = '004'
   )
BEGIN
    THROW 51004, N'Execute primeiro as migrações 001 a 004.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO seguranca.Perfil (Codigo, Nome, Descricao)
    SELECT
        Origem.Codigo,
        Origem.Nome,
        Origem.Descricao
    FROM
    (
        VALUES
            (N'ADMINISTRADOR', N'Administrador', N'Acesso a todas as funções administrativas do MVP.'),
            (N'TI', N'TI', N'Gerencia conteúdo e usuários, sem administrar perfis e permissões.'),
            (N'RECEPCIONISTA', N'Recepcionista', N'Perfil reservado para evolução; possui somente a consulta pública no MVP.'),
            (N'COLETADOR', N'Coletador', N'Perfil reservado para evolução; possui somente a consulta pública no MVP.')
    ) AS Origem (Codigo, Nome, Descricao)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM seguranca.Perfil AS Destino WITH (UPDLOCK, HOLDLOCK)
        WHERE Destino.Codigo = Origem.Codigo
    );

    INSERT INTO seguranca.Permissao (Codigo, Nome, Descricao)
    SELECT
        Origem.Codigo,
        Origem.Nome,
        Origem.Descricao
    FROM
    (
        VALUES
            (N'CONTEUDO_GERENCIAR', N'Gerenciar conteúdo', N'Permite manter tipos, categorias, exames, sinônimos, orientações e materiais.'),
            (N'USUARIOS_GERENCIAR', N'Gerenciar usuários', N'Permite manter usuários e executar redefinição administrativa de senha.'),
            (N'PERFIS_GERENCIAR', N'Gerenciar perfis', N'Permite manter perfis e suas associações com permissões.')
    ) AS Origem (Codigo, Nome, Descricao)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM seguranca.Permissao AS Destino WITH (UPDLOCK, HOLDLOCK)
        WHERE Destino.Codigo = Origem.Codigo
    );

    INSERT INTO seguranca.PerfilPermissao (PerfilId, PermissaoId)
    SELECT
        Perfil.PerfilId,
        Permissao.PermissaoId
    FROM
    (
        VALUES
            (N'ADMINISTRADOR', N'CONTEUDO_GERENCIAR'),
            (N'ADMINISTRADOR', N'USUARIOS_GERENCIAR'),
            (N'ADMINISTRADOR', N'PERFIS_GERENCIAR'),
            (N'TI', N'CONTEUDO_GERENCIAR'),
            (N'TI', N'USUARIOS_GERENCIAR')
    ) AS Origem (CodigoPerfil, CodigoPermissao)
    INNER JOIN seguranca.Perfil AS Perfil
        ON Perfil.Codigo = Origem.CodigoPerfil
    INNER JOIN seguranca.Permissao AS Permissao
        ON Permissao.Codigo = Origem.CodigoPermissao
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM seguranca.PerfilPermissao AS Destino WITH (UPDLOCK, HOLDLOCK)
        WHERE Destino.PerfilId = Perfil.PerfilId
          AND Destino.PermissaoId = Permissao.PermissaoId
    );

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.MigracaoBanco WITH (UPDLOCK, HOLDLOCK)
        WHERE Versao = '005'
    )
    BEGIN
        INSERT INTO dbo.MigracaoBanco (Versao, Descricao)
        VALUES ('005', N'Carga idempotente de perfis e permissões');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
