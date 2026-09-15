using SisExaminou.Application.Catalogo.CasosDeUso;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Infrastructure.Persistence.SqlServer.Catalogo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("SisExaminou");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure a connection string 'ConnectionStrings:SisExaminou'.");
}

builder.Services.AddScoped<ICatalogoConsultaRepository>(
    _ => new CatalogoConsultaRepository(connectionString));
builder.Services.AddScoped<IPesquisarExamesUseCase, PesquisarExamesUseCase>();
builder.Services.AddScoped<IObterDetalheExameUseCase, ObterDetalheExameUseCase>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
