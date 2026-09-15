using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Web.Configuracao;

namespace SisExaminou.Web.Seguranca;

public sealed class SessaoCookieEvents : CookieAuthenticationEvents
{
    private const string SessaoInvalidadaItem = "SisExaminou.SessaoInvalidada";

    private readonly IValidarSessaoUseCase _validarSessao;
    private readonly SegurancaWebOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SessaoCookieEvents> _logger;

    public SessaoCookieEvents(
        IValidarSessaoUseCase validarSessao,
        SegurancaWebOptions options,
        TimeProvider timeProvider,
        ILogger<SessaoCookieEvents> logger)
    {
        _validarSessao = validarSessao;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var identificador = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var versao = context.Principal?.FindFirstValue(
            CodigosSeguranca.ClaimVersaoCredencial);
        var validadoEm = context.Principal?.FindFirstValue(
            CodigosSeguranca.ClaimValidadoEmUtc);

        if (!int.TryParse(identificador, NumberStyles.None, CultureInfo.InvariantCulture, out var usuarioId)
            || !int.TryParse(versao, NumberStyles.None, CultureInfo.InvariantCulture, out var versaoCredencial)
            || !DateTimeOffset.TryParseExact(
                validadoEm,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var ultimaValidacao))
        {
            await RejeitarAsync(context);
            return;
        }

        var agora = _timeProvider.GetUtcNow();
        var intervalo = TimeSpan.FromMinutes(
            _options.Sessao.IntervaloRevalidacaoMinutos);
        if (agora - ultimaValidacao < intervalo)
        {
            return;
        }

        try
        {
            var sessao = await _validarSessao.ExecutarAsync(
                usuarioId,
                versaoCredencial,
                context.HttpContext.RequestAborted);

            if (sessao is null)
            {
                await RejeitarAsync(context);
                return;
            }

            context.ReplacePrincipal(SessaoClaimsFactory.Criar(sessao, agora));
            context.ShouldRenew = true;
        }
        catch (SegurancaIndisponivelException exception)
        {
            _logger.LogError(
                exception,
                "Não foi possível revalidar a sessão do usuário {UsuarioId}.",
                usuarioId);
            await RejeitarAsync(context);
        }
    }

    public override Task RedirectToLogin(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.HttpContext.Items.ContainsKey(SessaoInvalidadaItem))
        {
            var retorno = context.Request.PathBase
                + context.Request.Path
                + context.Request.QueryString;
            var destino = QueryHelpers.AddQueryString(
                "/conta/sessao-expirada",
                "returnUrl",
                retorno);
            context.Response.Redirect(destino);
            return Task.CompletedTask;
        }

        return base.RedirectToLogin(context);
    }

    private static async Task RejeitarAsync(
        CookieValidatePrincipalContext context)
    {
        context.HttpContext.Items[SessaoInvalidadaItem] = true;
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
