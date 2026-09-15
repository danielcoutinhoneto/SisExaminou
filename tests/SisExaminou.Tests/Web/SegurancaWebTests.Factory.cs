using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SisExaminou.Application.Catalogo.Contratos;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Tests.Application;
using SisExaminou.Tests.Doubles;

namespace SisExaminou.Tests.Web;

public sealed partial class SegurancaWebTests
{
    private sealed class SegurancaWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private readonly FakeAutenticacaoRepository _repository;
        private readonly bool _producao;

        public SegurancaWebApplicationFactory(
            FakeAutenticacaoRepository? repository = null,
            bool producao = false)
        {
            _repository = repository ?? CriarRepository();
            _producao = producao;
            Relogio = new FakeTimeProvider(
                new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero));
        }

        public FakeTimeProvider Relogio { get; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:SisExaminou",
                "Server=(local);Database=Teste;Integrated Security=True;TrustServerCertificate=True");

            if (_producao)
            {
                builder.UseEnvironment("Production");
            }

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAutenticacaoRepository>();
                services.RemoveAll<ISenhaHasher>();
                services.RemoveAll<ICatalogoConsultaRepository>();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<IAutenticacaoRepository>(_repository);
                services.AddSingleton<ISenhaHasher, FakeSenhaHasher>();
                services.AddSingleton<TimeProvider>(Relogio);
                services.AddSingleton<ICatalogoConsultaRepository>(
                    new FakeCatalogoConsultaRepository());
            });
        }

        public static FakeAutenticacaoRepository CriarRepository() =>
            new()
            {
                Usuario = SegurancaUseCaseTests.CriarUsuario(),
                SessaoValida = SegurancaUseCaseTests.CriarSessao()
            };
    }
}
