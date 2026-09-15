using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SisExaminou.Application.Catalogo.CasosDeUso;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Seguranca.CasosDeUso;
using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;
using SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;
using SisExaminou.Infrastructure.Seguranca;
using SisExaminou.Web.Configuracao;
using SisExaminou.Web.Seguranca;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var segurancaOptions = builder.Configuration
    .GetSection("Seguranca")
    .Get<SegurancaWebOptions>() ?? new SegurancaWebOptions();
segurancaOptions.Validar();

builder.Services.AddSingleton(segurancaOptions);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton(
    new PoliticaAutenticacao(
        segurancaOptions.Bloqueio.TentativasMaximas,
        TimeSpan.FromMinutes(segurancaOptions.Bloqueio.DuracaoMinutos)));

var connectionString = builder.Configuration.GetConnectionString("SisExaminou");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure a connection string 'ConnectionStrings:SisExaminou'.");
}

builder.Services.AddScoped<ICatalogoConsultaRepository>(
    _ => new CatalogoConsultaRepository(connectionString));
builder.Services.AddScoped<IAutenticacaoRepository>(
    _ => new AutenticacaoRepository(connectionString));
builder.Services.AddSingleton<ISenhaHasher, SenhaHasher>();
builder.Services.AddScoped<IPesquisarExamesUseCase, PesquisarExamesUseCase>();
builder.Services.AddScoped<IObterDetalheExameUseCase, ObterDetalheExameUseCase>();
builder.Services.AddScoped<IAutenticarUsuarioUseCase, AutenticarUsuarioUseCase>();
builder.Services.AddScoped<IValidarSessaoUseCase, ValidarSessaoUseCase>();
builder.Services.AddScoped<
    ICriarPrimeiroAdministradorUseCase,
    CriarPrimeiroAdministradorUseCase>();
builder.Services.AddScoped<SessaoCookieEvents>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".SisExaminou.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = "/conta/entrar";
        options.AccessDeniedPath = "/conta/acesso-negado";
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            segurancaOptions.Sessao.ExpiracaoMinutos);
        options.SlidingExpiration =
            segurancaOptions.Sessao.RenovacaoDeslizante;
        options.EventsType = typeof(SessaoCookieEvents);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        CodigosSeguranca.PoliticaConteudoGerenciar,
        policy => policy.RequireClaim(
            CodigosSeguranca.ClaimPermissao,
            CodigosSeguranca.PermissaoConteudoGerenciar));
    options.AddPolicy(
        CodigosSeguranca.PoliticaUsuariosGerenciar,
        policy => policy.RequireClaim(
            CodigosSeguranca.ClaimPermissao,
            CodigosSeguranca.PermissaoUsuariosGerenciar));
    options.AddPolicy(
        CodigosSeguranca.PoliticaPerfisGerenciar,
        policy => policy.RequireClaim(
            CodigosSeguranca.ClaimPermissao,
            CodigosSeguranca.PermissaoPerfisGerenciar));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
