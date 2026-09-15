# 🧪 SisExaminou

Sistema web para consulta de exames e orientações de coleta, com foco em acesso rápido, interface simples e administração segura do conteúdo.

> **Estado atual:** as Sprints 1 a 4 estão concluídas. Além da consulta pública, a aplicação possui autenticação por cookie, autorização por Claims e políticas, bloqueio temporário, revalidação de sessão e bootstrap explícito do primeiro administrador. A Sprint 5 — Administração é a próxima entrega planejada.

## Estado da implementação

### Implementado

- Solução direcionada ao .NET 10.
- Aplicação ASP.NET Core MVC com Razor Views.
- Projetos separados em Domain, Application, Infrastructure e Web.
- Projeto de testes com xUnit.
- Referências entre projetos respeitando a direção das dependências.
- Nullable reference types habilitado.
- Bootstrap e jQuery fornecidos pelo template MVC.
- Provisionamento do banco `SisExaminouDB` e scripts SQL incrementais `001` a `007`, com catálogo, segurança, índices, dados de referência, database role de menor privilégio e validação do código URL-safe.
- Carga manual de catálogo sintético, projetada para ser idempotente, exclusiva para desenvolvimento e separada das migrações.
- Build Release e execução do projeto de testes validados.
- Login pelo campo `Login`, logout e páginas de acesso negado, sessão expirada e indisponibilidade.
- Cookie `HttpOnly`, seguro fora de Development, `SameSite=Lax`, ticket de 30 minutos e renovação deslizante.
- Claims de usuário, perfil, permissões e `VersaoCredencial`, com revalidação no banco a cada cinco minutos.
- Políticas `ConteudoGerenciar`, `UsuariosGerenciar` e `PerfisGerenciar`.
- Bloqueio de 15 minutos após cinco tentativas inválidas e limpeza das falhas após sucesso.
- Bootstrap administrativo manual em `tools/SisExaminou.BootstrapAdmin`, sem senha versionada ou passada por argumento.

### Consulta pública entregue na Sprint 3

- Entidades e regras mínimas de publicação no Domain.
- DTOs, filtros, contratos e casos de uso na Application.
- Consultas ADO.NET parametrizadas, com paginação e relevância, na Infrastructure.
- Pesquisa e detalhe anônimos com Controllers, ViewModels e Razor Views responsivas.
- Respostas amigáveis para item inexistente e indisponibilidade do banco.
- Testes unitários, integrados com SQL Server e do fluxo Web em memória.

### Planejado para o MVP

- Administração de categorias, exames e orientações.
- Administração de usuários, perfis e permissões.
- Inativação de registros sem exclusão física.
- Health check da aplicação e do banco de dados.
- Integração e entrega contínuas com GitHub Actions.
- Publicação em Azure App Service e Azure SQL.

## Objetivo do MVP

O MVP deverá permitir:

- Consulta pública de exames e orientações.
- Manutenção protegida de categorias, exames e orientações.
- Administração de usuários, perfis e permissões.
- Build, testes, geração de artefato e deploy automatizados.

## Arquitetura

A solução utiliza Clean Architecture com projetos separados:

```text
SisExaminou/
├── src/
│   ├── SisExaminou.Domain/
│   ├── SisExaminou.Application/
│   ├── SisExaminou.Infrastructure/
│   │   └── Persistence/
│   │       └── SqlServer/
│   │           ├── Samples/
│   │           └── Scripts/
│   └── SisExaminou.Web/
├── tests/
│   └── SisExaminou.Tests/
├── tools/
│   └── SisExaminou.BootstrapAdmin/
├── SisExaminou.sln
└── README.md
```

### Responsabilidades

| Projeto | Responsabilidade |
|---|---|
| `SisExaminou.Domain` | Entidades, objetos de valor, enums, regras e exceções de domínio. |
| `SisExaminou.Application` | Casos de uso, DTOs, validações e contratos necessários à aplicação. |
| `SisExaminou.Infrastructure` | ADO.NET, SQL Server, repositórios e demais implementações de infraestrutura. |
| `SisExaminou.Web` | Controllers, ViewModels, Razor Views, arquivos estáticos e composição das dependências. |
| `SisExaminou.Tests` | Testes unitários e de integração. |

### Direção das dependências

```text
Web ───────────────► Application ─────────► Domain
 │
 └────────────────► Infrastructure ───────► Application
                              └────────────► Domain
```

Regras:

- Domain não referencia nenhum outro projeto da solução.
- Application referencia somente Domain.
- Infrastructure referencia Application e Domain.
- Web referencia Application e Infrastructure.
- Tests referencia apenas os projetos necessários a cada teste.
- Domain e Application não dependem de ASP.NET Core, Razor, ADO.NET ou SQL Server.

## Tecnologias

### Em uso

- .NET 10.
- C#.
- ASP.NET Core MVC.
- Razor Views.
- HTML5 e CSS3.
- Bootstrap.
- jQuery.
- xUnit.
- Injeção de dependência nativa do ASP.NET Core.
- ADO.NET com `Microsoft.Data.SqlClient`.
- SQL Server.
- Autenticação por Cookies.
- Claims e políticas de autorização.
- `PasswordHasher` do ASP.NET Core.

### Planejadas

- Azure SQL.
- SweetAlert2.
- GitHub Actions.
- Azure App Service.

## Perfis de acesso planejados

| Perfil | Consultar | Gerenciar conteúdo | Gerenciar usuários | Gerenciar perfis |
|---|---:|---:|---:|---:|
| Anônimo | Sim | Não | Não | Não |
| Coletador | Sim | Não | Não | Não |
| Recepcionista | Sim | Não | Não | Não |
| TI | Sim | Sim | Sim | Não |
| Administrador | Sim | Sim | Sim | Sim |

No MVP, Coletador e Recepcionista terão o mesmo acesso funcional porque a consulta será pública. Os perfis serão mantidos para auditoria e futuras regras de acesso.

## Roadmap do MVP

| Sprint | Entrega | Situação |
|---:|---|---|
| 1 | Estrutura e escopo | Concluída |
| 2 | Banco de dados | Concluída |
| 3 | Consulta pública | Concluída |
| 4 | Autenticação e autorização | Concluída |
| 5 | Administração | Próxima |
| 6 | Testes e segurança | Planejada |
| 7 | CI/CD e Azure | Planejada |

## Como executar localmente

### Pré-requisitos

- .NET 10 SDK.
- Visual Studio 2026 ou editor compatível com .NET 10.
- SQL Server com o banco `SisExaminouDB` provisionado pelas migrações `000` a `007`.

A aplicação exige `ConnectionStrings:SisExaminou`. O arquivo de desenvolvimento aponta para `SisExaminouDB` na instância local `.\SQLEXPRESS`; ajuste-o ou use User Secrets/variável de ambiente no seu computador. A ordem das migrações e a carga manual do catálogo demonstrativo estão documentadas em `src/SisExaminou.Infrastructure/Persistence/SqlServer/Scripts/README.md`.

### Restaurar, compilar e testar

Na raiz do repositório:

```powershell
dotnet restore SisExaminou.sln
dotnet build SisExaminou.sln -c Release --no-restore
dotnet test tests/SisExaminou.Tests/SisExaminou.Tests.csproj -c Release --no-build
```

Os testes de integração com SQL Server são opcionais por padrão. Para executá-los contra um banco de desenvolvimento já migrado e com o catálogo demonstrativo:

```powershell
$env:SISEXAMINOU_TEST_CONNECTION_STRING = "Server=.\SQLEXPRESS;Database=SisExaminouDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
dotnet test tests/SisExaminou.Tests/SisExaminou.Tests.csproj -c Release --filter FullyQualifiedName~Integration
```

### Executar a aplicação Web

```powershell
dotnet run --project src/SisExaminou.Web/SisExaminou.Web.csproj --launch-profile https
```

Endereços configurados no perfil local:

- HTTPS: `https://localhost:7051`
- HTTP: `http://localhost:5187`

As portas podem ser alteradas em `src/SisExaminou.Web/Properties/launchSettings.json`.

### Criar explicitamente o primeiro administrador

O bootstrap nunca roda no startup da Web. Após aplicar as migrações `001` a `007`, abra um PowerShell, defina a conexão apenas na sessão atual e execute:

```powershell
$env:SISEXAMINOU_BOOTSTRAP_CONNECTION_STRING = "Server=.\SQLEXPRESS;Database=SisExaminouDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
dotnet run --project tools/SisExaminou.BootstrapAdmin/SisExaminou.BootstrapAdmin.csproj
Remove-Item Env:SISEXAMINOU_BOOTSTRAP_CONNECTION_STRING
```

O programa solicita nome, login, email opcional e frase-senha de forma interativa. A frase-senha não aparece no terminal e não deve ser colocada no comando, em `appsettings`, User Secrets, variável de ambiente ou log. A operação cria um usuário somente se não existir administrador ativo; uma reexecução não cria outro.

Para ambientes sem autenticação integrada, obtenha a connection string de um cofre de segredos e mantenha seu valor fora do histórico do terminal e do repositório.

## Regras de qualidade e segurança

- SQL sempre parametrizado, sem concatenação de entrada do usuário.
- Senhas armazenadas somente como hash.
- Nenhum segredo ou dado sensível versionado ou registrado em log.
- Autorização validada no servidor, independentemente da visibilidade dos menus.
- Registros administrativos preferencialmente inativados, sem exclusão física.
- Domain e Application mantidos independentes de infraestrutura e apresentação.
- Build Release e testes relacionados aprovados antes da conclusão de cada sprint.

## Fora do escopo do MVP

- API pública.
- Geração de PDF.
- Aplicação offline.
- Suporte multilíngue.
- Relatórios avançados.
- Recuperação automática de senha.
- Autenticação multifator.
- Login social ou SSO.
- Integrações externas.
- Alta disponibilidade.

## Autor

**Daniel Coutinho Neto**  
Desenvolvedor .NET | C# | ASP.NET Core | APIs REST | SQL Server | Backend

📧 [danielcoutinhoneto@outlook.com](mailto:danielcoutinhoneto@outlook.com)  
🔗 [LinkedIn](https://linkedin.com/in/daniel-coutinho-neto)  
🌐 [danielcoutinho.dev.br](https://danielcoutinho.dev.br)


