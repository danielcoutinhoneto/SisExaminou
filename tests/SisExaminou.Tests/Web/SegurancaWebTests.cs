using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Tests.Application;
using SisExaminou.Tests.Doubles;
using SisExaminou.Web.Seguranca;

namespace SisExaminou.Tests.Web;

public sealed partial class SegurancaWebTests
{
    [Fact]
    public async Task Administracao_AnonimoEhRedirecionadoParaLogin()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/administracao");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/conta/entrar", response.Headers.Location?.AbsolutePath);
        Assert.Contains("returnUrl", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LoginValido_EmiteCookieClaimsEAcessaPaginaProtegida()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var login = await PostarLoginAsync(client, "frase-senha-valida");

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        var cookie = Assert.Single(login.Headers.GetValues("Set-Cookie"));
        Assert.Contains(".SisExaminou.Auth=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);

        var administracao = await client.GetAsync("/administracao");
        var html = WebUtility.HtmlDecode(
            await administracao.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, administracao.StatusCode);
        Assert.Contains("Área protegida", html);
        Assert.Contains("Administrador", html);
    }

    [Fact]
    public async Task LoginInvalido_ExibeMensagemGenericaSemRevelarOUsuario()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await PostarLoginAsync(client, "senha-errada");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Login ou senha inválidos", html);
        Assert.DoesNotContain("usuário não existe", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("senha incorreta", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UsuarioSemPermissao_RecebePaginaComStatus403()
    {
        var repository = SegurancaWebApplicationFactory.CriarRepository();
        repository.Usuario = SegurancaUseCaseTests.CriarUsuario(permissoes: []);
        repository.SessaoValida = SegurancaUseCaseTests.CriarSessao(permissoes: []);
        await using var factory = new SegurancaWebApplicationFactory(repository);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });

        await PostarLoginAsync(client, "frase-senha-valida");
        var response = await client.GetAsync("/administracao");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("Acesso negado", html);
    }

    [Fact]
    public async Task VersaoCredencialDivergente_InvalidaCookieEExplicaSessaoExpirada()
    {
        var repository = SegurancaWebApplicationFactory.CriarRepository();
        await using var factory = new SegurancaWebApplicationFactory(repository);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        await PostarLoginAsync(client, "frase-senha-valida");
        repository.SessaoValida = SegurancaUseCaseTests.CriarSessao(
            versaoCredencial: 4);
        factory.Relogio.AgoraUtc = factory.Relogio.AgoraUtc.AddMinutes(6);

        var response = await client.GetAsync("/administracao");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(
            "/conta/sessao-expirada",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_RemoveCookieEProtegeNovamenteAAdministracao()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        await PostarLoginAsync(client, "frase-senha-valida");
        var pagina = await client.GetAsync("/administracao");
        var token = ExtrairAntiforgery(await pagina.Content.ReadAsStringAsync());
        var logout = await client.PostAsync(
            "/conta/sair",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        var administracao = await client.GetAsync("/administracao");
        Assert.Equal(
            "/conta/entrar",
            administracao.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task FalhaDoBanco_Retorna503SemDetalhesTecnicos()
    {
        var repository = SegurancaWebApplicationFactory.CriarRepository();
        repository.ExcecaoAoObter = new SegurancaIndisponivelException(
            "credencial SQL interna",
            new InvalidOperationException("servidor-interno-01"));
        await using var factory = new SegurancaWebApplicationFactory(repository);
        using var client = factory.CreateClient();

        var response = await PostarLoginAsync(client, "frase-senha-valida");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("temporariamente indisponível", html);
        Assert.DoesNotContain("credencial SQL interna", html);
        Assert.DoesNotContain("servidor-interno-01", html);
    }

    [Fact]
    public async Task ClaimsEPoliticas_RefletemSomentePermissoesDaSessao()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        var autorizacao = factory.Services.GetRequiredService<IAuthorizationService>();
        var principal = SessaoClaimsFactory.Criar(
            SegurancaUseCaseTests.CriarSessao(permissoes:
                [CodigosSeguranca.PermissaoConteudoGerenciar]),
            DateTimeOffset.UtcNow);

        Assert.Equal("7", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Administrador", principal.Identity?.Name);
        Assert.Equal(
            CodigosSeguranca.PerfilAdministrador,
            principal.FindFirstValue(CodigosSeguranca.ClaimPerfil));
        Assert.Equal("3", principal.FindFirstValue(CodigosSeguranca.ClaimVersaoCredencial));
        Assert.True((await autorizacao.AuthorizeAsync(
            principal,
            null,
            CodigosSeguranca.PoliticaConteudoGerenciar)).Succeeded);
        Assert.False((await autorizacao.AuthorizeAsync(
            principal,
            null,
            CodigosSeguranca.PoliticaUsuariosGerenciar)).Succeeded);
        Assert.False((await autorizacao.AuthorizeAsync(
            principal,
            null,
            CodigosSeguranca.PoliticaPerfisGerenciar)).Succeeded);
    }

    [Fact]
    public async Task Politicas_RespeitamMatrizDosPerfisDoMvp()
    {
        await using var factory = new SegurancaWebApplicationFactory();
        var autorizacao = factory.Services.GetRequiredService<IAuthorizationService>();
        var administrador = SessaoClaimsFactory.Criar(
            SegurancaUseCaseTests.CriarSessao(),
            DateTimeOffset.UtcNow);
        var ti = SessaoClaimsFactory.Criar(
            SegurancaUseCaseTests.CriarSessao(permissoes:
            [
                CodigosSeguranca.PermissaoConteudoGerenciar,
                CodigosSeguranca.PermissaoUsuariosGerenciar
            ]),
            DateTimeOffset.UtcNow);
        var operacional = SessaoClaimsFactory.Criar(
            SegurancaUseCaseTests.CriarSessao(permissoes: []),
            DateTimeOffset.UtcNow);

        Assert.True(await TemPoliticaAsync(
            autorizacao,
            administrador,
            CodigosSeguranca.PoliticaConteudoGerenciar));
        Assert.True(await TemPoliticaAsync(
            autorizacao,
            administrador,
            CodigosSeguranca.PoliticaUsuariosGerenciar));
        Assert.True(await TemPoliticaAsync(
            autorizacao,
            administrador,
            CodigosSeguranca.PoliticaPerfisGerenciar));
        Assert.True(await TemPoliticaAsync(
            autorizacao,
            ti,
            CodigosSeguranca.PoliticaConteudoGerenciar));
        Assert.True(await TemPoliticaAsync(
            autorizacao,
            ti,
            CodigosSeguranca.PoliticaUsuariosGerenciar));
        Assert.False(await TemPoliticaAsync(
            autorizacao,
            ti,
            CodigosSeguranca.PoliticaPerfisGerenciar));
        Assert.False(await TemPoliticaAsync(
            autorizacao,
            operacional,
            CodigosSeguranca.PoliticaConteudoGerenciar));
    }

    private static async Task<bool> TemPoliticaAsync(
        IAuthorizationService autorizacao,
        ClaimsPrincipal principal,
        string politica) =>
        (await autorizacao.AuthorizeAsync(principal, null, politica)).Succeeded;

    [Fact]
    public void Cookie_EmProducaoExigeTransporteHttps()
    {
        using var factory = new SegurancaWebApplicationFactory(producao: true);
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.True(options.SlidingExpiration);
        Assert.Equal(TimeSpan.FromMinutes(30), options.ExpireTimeSpan);
    }

    private static async Task<HttpResponseMessage> PostarLoginAsync(
        HttpClient client,
        string senha)
    {
        var pagina = await client.GetAsync("/conta/entrar");
        var token = ExtrairAntiforgery(await pagina.Content.ReadAsStringAsync());

        return await client.PostAsync(
            "/conta/entrar",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Login"] = "admin",
                ["Senha"] = senha,
                ["ReturnUrl"] = "/administracao",
                ["__RequestVerificationToken"] = token
            }));
    }
}
