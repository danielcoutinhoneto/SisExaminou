# 🧪 SisExaminou

Sistema web para consulta de exames e orientações de coleta, com foco em acesso rápido, interface simples e administração segura do conteúdo.

> **Estado atual:** as Sprints 1 — Estrutura e Escopo e 2 — Banco de Dados estão concluídas. A solução possui a estrutura inicial em Clean Architecture e um esquema SQL Server incremental validado. Os fluxos funcionais do MVP serão implementados nas próximas sprints.

## Estado da implementação

### Implementado

- Solução direcionada ao .NET 10.
- Aplicação ASP.NET Core MVC com Razor Views.
- Projetos separados em Domain, Application, Infrastructure e Web.
- Projeto de testes com xUnit.
- Referências entre projetos respeitando a direção das dependências.
- Nullable reference types habilitado.
- Bootstrap e jQuery fornecidos pelo template MVC.
- Provisionamento do banco `SisExaminouDB` e scripts SQL incrementais `001` a `007`, com catálogo, segurança, índices, seeds, database role de menor privilégio e validação do código URL-safe.
- Build Release e execução do projeto de testes validados.

### Planejado para o MVP

- Consulta pública de exames por nome ou tipo.
- Visualização de detalhes e orientações de coleta.
- Persistência com ADO.NET puro e SQL Server/Azure SQL.
- Login e logout com autenticação por cookies.
- Autorização com Claims, perfis e permissões.
- Administração de categorias, exames e orientações.
- Administração de usuários, perfis e permissões.
- Inativação de registros sem exclusão física.
- Tratamento seguro de erros e logging técnico.
- Health check da aplicação e do banco de dados.
- Testes unitários e de integração dos fluxos críticos.
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
│   │           └── Scripts/
│   └── SisExaminou.Web/
├── tests/
│   └── SisExaminou.Tests/
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

### Planejadas

- ADO.NET puro, sem Entity Framework.
- SQL Server e Azure SQL.
- Autenticação com Cookies.
- Claims e políticas de autorização.
- PasswordHasher do ASP.NET Core.
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
| 3 | Consulta pública | Em andamento |
| 4 | Autenticação e autorização | Planejada |
| 5 | Administração | Planejada |
| 6 | Testes e segurança | Planejada |
| 7 | CI/CD e Azure | Planejada |

## Como executar localmente

### Pré-requisitos

- .NET 10 SDK.
- Visual Studio 2026 ou editor compatível com .NET 10.

O template MVC ainda executa sem conexão com banco porque os repositórios ADO.NET pertencem à Sprint 3. A ordem e os requisitos dos scripts estão documentados em `src/SisExaminou.Infrastructure/Persistence/SqlServer/Scripts/README.md`.

### Restaurar, compilar e testar

Na raiz do repositório:

```powershell
dotnet restore SisExaminou.sln
dotnet build SisExaminou.sln -c Release --no-restore
dotnet test tests/SisExaminou.Tests/SisExaminou.Tests.csproj -c Release --no-build
```

### Executar a aplicação Web

```powershell
dotnet run --project src/SisExaminou.Web/SisExaminou.Web.csproj --launch-profile https
```

Endereços configurados no perfil local:

- HTTPS: `https://localhost:7051`
- HTTP: `http://localhost:5187`

As portas podem ser alteradas em `src/SisExaminou.Web/Properties/launchSettings.json`.

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


