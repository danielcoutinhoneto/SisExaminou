# Scripts SQL Server

Provisionamento e scripts incrementais do banco `SisExaminouDB`. Eles criam somente a estrutura prevista para o MVP e não implementam repositórios ADO.NET, telas, login ou administração.

## Pré-requisitos

- SQL Server ou Azure SQL compatível com os recursos utilizados.
- Identidade de implantação com permissão para criar schemas, tabelas, índices e database roles.
- Identidade de implantação separada da identidade usada pela aplicação em execução.

O arquivo `000_CreateDatabase.sql` deve ser executado em `master` e cria `SisExaminouDB` apenas quando ele ainda não existe. Os scripts `001` a `006` devem ser executados conectados ao `SisExaminouDB` e recusam execução nos bancos de sistema.

## Ordem obrigatória

0. `000_CreateDatabase.sql` — provisiona o banco; não é uma migração.
1. `001_CreateSchemasAndVersioning.sql`
2. `002_CreateCatalogTables.sql`
3. `003_CreateSecurityTables.sql`
4. `004_CreateIndexes.sql`
5. `005_SeedProfilesAndPermissions.sql`
6. `006_CreateApplicationRole.sql`

Não pule números. `001` a `006` validam a migração anterior e registram o sucesso em `dbo.MigracaoBanco`. Portanto, são seis migrações aplicadas dentro de um único banco, e não seis bancos.

## Execução

Os arquivos não contêm `GO` e podem ser enviados individualmente como um único batch por SSMS, Azure Data Studio, `sqlcmd` ou um executor de implantação.

Exemplo com autenticação integrada:

```powershell
sqlcmd -S "<servidor>" -d "master" -E -b -i ".\000_CreateDatabase.sql"
sqlcmd -S "<servidor>" -d "SisExaminouDB" -E -b -i ".\001_CreateSchemasAndVersioning.sql"
```

Repita o segundo comando para as migrações `002` a `006`, respeitando a ordem. A opção `-b` faz o processo retornar falha quando o SQL Server informar erro.

Não execute migrações automaticamente no startup da aplicação. A aplicação pode iniciar com várias instâncias simultâneas e não deve possuir privilégios de DDL.

## Reexecução e alteração

- `000` não recria nem altera `SisExaminouDB` quando ele já existe.
- `001` a `004` e `006` encerram sem alteração quando sua versão já está registrada.
- `005` revalida os seeds por código e pode ser executado novamente sem duplicá-los.
- Um script aplicado em qualquer ambiente torna-se imutável.
- Qualquer mudança posterior recebe um novo número; não edite nem reutilize uma versão aplicada.

## Automação planejada

A execução ainda é manual. A Sprint 7 prevê `scripts/database/Apply-Database.ps1`, que executará o provisionamento `000` em `master` e todas as migrações pendentes no `SisExaminouDB` com um único comando. O orquestrador reutilizará estes mesmos arquivos SQL e não armazenará credenciais no repositório.

## Database role

`006` cria a role `SisExaminouAplicacao`, mas não cria login nem usuário, pois esses nomes e mecanismos variam entre ambientes.

Exemplo de mapeamento para um login já provisionado:

```sql
CREATE USER [usuario_do_ambiente] FOR LOGIN [login_do_ambiente];
ALTER ROLE [SisExaminouAplicacao] ADD MEMBER [usuario_do_ambiente];
```

Para Azure SQL com Microsoft Entra ID, o provisionamento poderá usar `CREATE USER ... FROM EXTERNAL PROVIDER`, conforme a identidade escolhida na Sprint 7.

A role permite leitura dos dois schemas, escrita nos cadastros administrados e exclusão apenas nas tabelas de associação. Ela não recebe DDL nem participação em `db_owner` ou `db_ddladmin`. Ao validar, confirme que o usuário de ambiente não herdou permissões adicionais por outras roles.

## Segurança

- Nenhum script contém usuário administrador, senha, hash real, token ou connection string.
- O primeiro administrador será criado pelo bootstrap seguro da Sprint 4.
- A aplicação deve armazenar somente `SenhaHash`, gerado por `PasswordHasher`.
- O executor de migrações e a conta da aplicação devem usar segredos externos ao repositório.

## Evidências necessárias

Antes de encerrar a Sprint 2:

1. Execute todos os scripts em um banco vazio.
2. Confirme as seis versões em `dbo.MigracaoBanco`.
3. Reexecute `005` e confirme os mesmos quatro perfis, três permissões e cinco associações.
4. Valide PKs, FKs, checks, unicidades, índices filtrados, inativação e `rowversion`.
5. Teste a role com uma conta dedicada e confirme que DDL e `DELETE` nos cadastros principais são negados.

As justificativas completas do modelo estão em `docs-internos/sprints/sprints_2_detalhamento.md`.

## Estado local

Em 2026-08-07, `SisExaminouDB` já existia na instância `.\SQLEXPRESS` apenas com `dbo.sysdiagrams`. O provisionamento foi preservado de forma idempotente e as migrações `001` a `006` foram aplicadas com sucesso nesse banco.
