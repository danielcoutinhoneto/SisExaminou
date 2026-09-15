using SisExaminou.Application.Seguranca.CasosDeUso;
using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Application.Seguranca.Modelos;
using SisExaminou.Tests.Doubles;

namespace SisExaminou.Tests.Application;

public sealed class SegurancaUseCaseTests
{
    private static readonly DateTimeOffset Agora =
        new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Autenticar_ComCredenciaisValidas_CriaSessaoELimpaFalhas()
    {
        var repository = new FakeAutenticacaoRepository
        {
            Usuario = CriarUsuario(permissoes:
                ["CONTEUDO_GERENCIAR", "CONTEUDO_GERENCIAR"])
        };
        var hasher = new FakeSenhaHasher();
        var useCase = CriarAutenticador(repository, hasher);

        var resultado = await useCase.ExecutarAsync(
            new AutenticarUsuarioComando(" admin ", hasher.SenhaEsperada),
            CancellationToken.None);

        Assert.True(resultado.Autenticado);
        Assert.NotNull(resultado.Sessao);
        Assert.Equal(7, resultado.Sessao.UsuarioId);
        Assert.Single(resultado.Sessao.Permissoes);
        Assert.Equal(1, repository.LimpezasRegistradas);
        Assert.Equal(1, repository.AtualizacoesUltimoAcesso);
        Assert.True(resultado.UltimoAcessoAtualizado);
    }

    [Fact]
    public async Task Autenticar_LoginInexistenteESenhaErrada_UsamMensagemGenerica()
    {
        var hasherInexistente = new FakeSenhaHasher();
        var inexistente = await CriarAutenticador(
                new FakeAutenticacaoRepository(),
                hasherInexistente)
            .ExecutarAsync(
                new AutenticarUsuarioComando("desconhecido", "errada"),
                CancellationToken.None);

        var repositoryExistente = new FakeAutenticacaoRepository
        {
            Usuario = CriarUsuario()
        };
        var existente = await CriarAutenticador(
                repositoryExistente,
                new FakeSenhaHasher())
            .ExecutarAsync(
                new AutenticarUsuarioComando("admin", "errada"),
                CancellationToken.None);

        Assert.Equal(AutenticarUsuarioUseCase.MensagemCredenciaisInvalidas, inexistente.Mensagem);
        Assert.Equal(inexistente.Mensagem, existente.Mensagem);
        Assert.Null(hasherInexistente.UltimoHashVerificado);
        Assert.Equal(1, repositoryExistente.FalhasRegistradas);
        Assert.Equal(5, repositoryExistente.UltimasTentativasMaximas);
        Assert.Equal(TimeSpan.FromMinutes(15), repositoryExistente.UltimaDuracaoBloqueio);
    }

    [Fact]
    public async Task Autenticar_UsuarioBloqueadoOuInativo_NaoCriaSessao()
    {
        foreach (var usuario in new[]
        {
            CriarUsuario(bloqueadoAteUtc: Agora.AddMinutes(1).UtcDateTime),
            CriarUsuario(ativo: false),
            CriarUsuario(perfilAtivo: false)
        })
        {
            var repository = new FakeAutenticacaoRepository { Usuario = usuario };
            var hasher = new FakeSenhaHasher();

            var resultado = await CriarAutenticador(repository, hasher)
                .ExecutarAsync(
                    new AutenticarUsuarioComando("admin", hasher.SenhaEsperada),
                    CancellationToken.None);

            Assert.False(resultado.Autenticado);
            Assert.Equal(AutenticarUsuarioUseCase.MensagemCredenciaisInvalidas, resultado.Mensagem);
            Assert.Equal(0, repository.LimpezasRegistradas);
        }
    }

    [Fact]
    public async Task Autenticar_BloqueioExpiradoELocalizacaoDoUltimoAcessoFalha_AindaCriaSessao()
    {
        var repository = new FakeAutenticacaoRepository
        {
            Usuario = CriarUsuario(
                bloqueadoAteUtc: Agora.AddSeconds(-1).UtcDateTime),
            ExcecaoAoAtualizarUltimoAcesso = new SegurancaIndisponivelException(
                "falha controlada",
                new InvalidOperationException())
        };
        var hasher = new FakeSenhaHasher();

        var resultado = await CriarAutenticador(repository, hasher)
            .ExecutarAsync(
                new AutenticarUsuarioComando("admin", hasher.SenhaEsperada),
                CancellationToken.None);

        Assert.True(resultado.Autenticado);
        Assert.False(resultado.UltimoAcessoAtualizado);
        Assert.Equal(1, repository.LimpezasRegistradas);
    }

    [Fact]
    public async Task ValidarSessao_ExigeUsuarioEComoVersaoCredencialAtuais()
    {
        var repository = new FakeAutenticacaoRepository
        {
            SessaoValida = CriarSessao(versaoCredencial: 3)
        };
        var useCase = new ValidarSessaoUseCase(repository);

        var valida = await useCase.ExecutarAsync(7, 3, CancellationToken.None);
        var divergente = await useCase.ExecutarAsync(7, 2, CancellationToken.None);
        var identificadorInvalido = await useCase.ExecutarAsync(0, 3, CancellationToken.None);

        Assert.NotNull(valida);
        Assert.Null(divergente);
        Assert.Null(identificadorInvalido);
    }

    [Fact]
    public async Task Bootstrap_ValidaNormalizaEGeraHashSemPersistirASenha()
    {
        var repository = new FakeAutenticacaoRepository();
        var hasher = new FakeSenhaHasher();
        var useCase = new CriarPrimeiroAdministradorUseCase(
            repository,
            hasher,
            new FakeTimeProvider(Agora));

        var resultado = await useCase.ExecutarAsync(
            new CriarAdministradorComando(
                " Administrador Inicial ",
                " ADMIN.Inicial ",
                " ADMIN@EXEMPLO.TEST ",
                "frase senha longa e exclusiva"),
            CancellationToken.None);

        Assert.True(resultado.Criado);
        Assert.Equal(1, hasher.HashesGerados);
        Assert.Equal("admin.inicial", repository.UltimoAdministrador!.Login);
        Assert.Equal("admin@exemplo.test", repository.UltimoAdministrador.Email);
        Assert.Null(repository.UltimoAdministrador.Senha);
        Assert.StartsWith("HASH::", repository.UltimoHashAdministrador);
    }

    [Fact]
    public async Task Bootstrap_DadosInvalidosNaoAcessamRepositorio_EReexecucaoEhIdempotente()
    {
        var repository = new FakeAutenticacaoRepository { ResultadoBootstrap = false };
        var hasher = new FakeSenhaHasher();
        var useCase = new CriarPrimeiroAdministradorUseCase(
            repository,
            hasher,
            new FakeTimeProvider(Agora));

        var invalido = await useCase.ExecutarAsync(
            new CriarAdministradorComando("x", "!", "email-invalido", "curta"),
            CancellationToken.None);

        Assert.Equal(StatusBootstrapAdministrador.DadosInvalidos, invalido.Status);
        Assert.Equal(0, repository.BootstrapExecutados);
        Assert.Equal(0, hasher.HashesGerados);

        var reexecucao = await useCase.ExecutarAsync(
            new CriarAdministradorComando(
                "Administrador Inicial",
                "admin",
                null,
                "frase senha longa e exclusiva"),
            CancellationToken.None);

        Assert.Equal(
            StatusBootstrapAdministrador.AdministradorAtivoJaExiste,
            reexecucao.Status);
        Assert.Equal(1, repository.BootstrapExecutados);
    }

    private static AutenticarUsuarioUseCase CriarAutenticador(
        FakeAutenticacaoRepository repository,
        FakeSenhaHasher hasher) =>
        new(
            repository,
            hasher,
            new PoliticaAutenticacao(5, TimeSpan.FromMinutes(15)),
            new FakeTimeProvider(Agora));

    internal static UsuarioAutenticacaoDto CriarUsuario(
        bool ativo = true,
        bool perfilAtivo = true,
        DateTime? bloqueadoAteUtc = null,
        IReadOnlyList<string>? permissoes = null) =>
        new(
            7,
            1,
            "Administrador",
            "admin",
            FakeSenhaHasher.HashValido,
            ativo,
            0,
            bloqueadoAteUtc,
            3,
            CodigosSeguranca.PerfilAdministrador,
            perfilAtivo,
            permissoes ??
            [
                CodigosSeguranca.PermissaoConteudoGerenciar,
                CodigosSeguranca.PermissaoUsuariosGerenciar,
                CodigosSeguranca.PermissaoPerfisGerenciar
            ]);

    internal static SessaoUsuarioDto CriarSessao(
        int versaoCredencial = 3,
        IReadOnlyList<string>? permissoes = null) =>
        new(
            7,
            "Administrador",
            CodigosSeguranca.PerfilAdministrador,
            versaoCredencial,
            permissoes ??
            [
                CodigosSeguranca.PermissaoConteudoGerenciar,
                CodigosSeguranca.PermissaoUsuariosGerenciar,
                CodigosSeguranca.PermissaoPerfisGerenciar
            ]);
}
