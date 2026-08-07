SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'master'
BEGIN
    THROW 51000, N'Execute o provisionamento 000 conectado ao banco master.', 1;
END;

IF DB_ID(N'SisExaminouDB') IS NULL
BEGIN
    EXEC sys.sp_executesql
        N'CREATE DATABASE [SisExaminouDB]
          COLLATE Latin1_General_100_CI_AI;';
END;
