# 🧪 SisExaminou 

**Sistema de Orientações de Coleta de Exames**

Com um foco em **acesso rápido**, **interface simples** e **controle de permissões por perfil**, o sistema visa padronizar e facilitar o acesso às informações essenciais para a preparação e coleta correta de exames.

---

## 🛠️ Tecnologias Utilizadas

- ASP.NET Core MVC 8  
- ADO.NET puro (sem Entity Framework)  
- SQL Server 2022  
- HTML5, CSS3 e Razor Views  
- Bootstrap 5.3  
- SweetAlert2 para alertas e feedbacks visuais  
- Autenticação com Cookies + Claims  
- Organização inspirada em Clean Architecture (em projeto único)  
- Injeção de dependência nativa do ASP.NET Core  
- Camadas lógicas separadas por pasta (Domain, Application, Infrastructure, Presentation)  

---

## 🎯 Funcionalidades

- 🔍 Pesquisa rápida por nome ou tipo de exame com acesso livre  
- 📋 Visualização de orientações específicas e detalhadas  
- 🔐 Login seguro com controle de acesso por **perfis (roles)**  
- ⚙️ Painel administrativo para:  
  - Cadastro de exames  
  - Gerenciamento de categorias  
  - Definição de orientações  
  - Controle de usuários e permissões  
- 📁 Organização de menus baseada em permissões  
- 🧩 Estrutura clara, testável e de fácil manutenção  

---

## 👥 Perfis de Acesso

| Perfil           | Permissões principais                             |
|------------------|---------------------------------------------------|
| **Administrador**| Acesso completo ao sistema                        |
| **TI**           | Administração técnica, manutenção de dados        |
| **Recepcionista**| Acesso à pesquisa e visualização de exames        |
| **Coletador**    | Visualização rápida das instruções de coleta      |

---

## 📁 Estrutura de Pastas (padrão ASP.NET + Clean Code)

```
SisExaminou/
├── Controllers/        # Lógica de interface (MVC)
├── Views/              # Páginas Razor (.cshtml)
├── ViewModels/         # Dados exibidos nas Views
├── Models/             # Entidades de domínio
├── Services/           # Regras de negócio
├── Data/
│   ├── Interfaces/     # Contratos dos repositórios
│   └── Repositories/   # Implementações com ADO.NET
├── wwwroot/            # Recursos estáticos (CSS, JS, Imagens)
├── appsettings.json    # Configurações (ex: string de conexão)
└── Program.cs
```

---

---

## 🚀 Como Executar Localmente

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/SisExaminou.git
cd SisExaminou
```

---

### 2. Abra o projeto

#### 🖥️ Se estiver usando o **Visual Studio (2022 ou superior)**:
- Abra o arquivo da solução `.sln` no Visual Studio:
  - Menu `Arquivo` → `Abrir` → `Projeto/Solução...`
  - Selecione o arquivo `SisExaminou.sln`
- Aguarde o carregamento e restauração dos pacotes NuGet.
- Configure a **string de conexão** em `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;Database=SisExaminou;Trusted_Connection=True;"
}
```

- Pressione `Ctrl + F5` para executar sem depuração  
  *(ou clique em “Iniciar” na parte superior da IDE)*

---

#### 💻 Se estiver usando o **Visual Studio Code**:
- Abra o terminal integrado (`` Ctrl + ` ``)
- Verifique se o SDK do .NET está instalado:

```bash
dotnet --version
```

- Restaure os pacotes:

```bash
dotnet restore
```

- Configure sua **string de conexão** no `appsettings.json` como acima.
- Execute o projeto:

```bash
dotnet run
```

- Acesse no navegador:

```
https://localhost:5001
```

*(ou outra porta informada no terminal)*

---

✔️ Pronto! O sistema estará rodando localmente, com a interface acessível pelo navegador.

---

## ✅ Boas Práticas Aplicadas

- 🔸 Separação clara por responsabilidades  
- 🔸 Uso de ViewModels nas Views (nunca as entidades)  
- 🔸 Acesso a dados via Repositórios com ADO.NET  
- 🔸 Lógica de negócio encapsulada em Services  
- 🔸 Injeção de dependência via Program.cs  
- 🔸 Alertas visuais com SweetAlert2 para melhor experiência  
- 🔸 Controle de acesso via Claims por perfil  

---

## 💡 Melhorias Futuras

- API REST para integração externa  
- Geração de PDF das orientações  
- Suporte multilíngue (PT-BR, EN, ES)  
- Modo mobile offline para tablets em coleta  
- Logs de acesso e relatórios de uso  

---

## 🧠 Autor

**Daniel Coutinho Neto**  
Desenvolvedor .Net | C# | .Net Core | .Net Framework | SQL Server | Bootstrap  
📧 daniel@exemplo.com  
🔗 [linkedin.com/in/daniel-coutinho-neto](https://linkedin.com/in/daniel-coutinho-neto)
