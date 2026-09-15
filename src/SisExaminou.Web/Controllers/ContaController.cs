using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SisExaminou.Application.Seguranca.Contratos;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Application.Seguranca.Modelos;
using SisExaminou.Web.Models.Seguranca;
using SisExaminou.Web.Seguranca;

namespace SisExaminou.Web.Controllers;

[Route("conta")]
public sealed class ContaController : Controller
{
    private readonly IAutenticarUsuarioUseCase _autenticar;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ContaController> _logger;

    public ContaController(
        IAutenticarUsuarioUseCase autenticar,
        TimeProvider timeProvider,
        ILogger<ContaController> logger)
    {
        _autenticar = autenticar;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("entrar")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Entrar(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirecionarLocalOuAdministracao(returnUrl);
        }

        return View(new LoginInputModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("entrar")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Entrar(
        LoginInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        try
        {
            var resultado = await _autenticar.ExecutarAsync(
                new AutenticarUsuarioComando(input.Login, input.Senha),
                cancellationToken);

            if (!resultado.Autenticado || resultado.Sessao is null)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensagem!);
                input.Senha = null;
                return View(input);
            }

            var principal = SessaoClaimsFactory.Criar(
                resultado.Sessao,
                _timeProvider.GetUtcNow());
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = false
                });

            if (!resultado.UltimoAcessoAtualizado)
            {
                _logger.LogWarning(
                    "A sessão do usuário {UsuarioId} foi criada, mas o último acesso não foi atualizado.",
                    resultado.Sessao.UsuarioId);
            }

            _logger.LogInformation(
                "Autenticação concluída para o usuário {UsuarioId}.",
                resultado.Sessao.UsuarioId);
            return RedirecionarLocalOuAdministracao(input.ReturnUrl);
        }
        catch (SegurancaIndisponivelException exception)
        {
            _logger.LogError(exception, "Falha técnica durante a autenticação.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            input.Senha = null;
            return View("Indisponivel", input);
        }
    }

    [Authorize]
    [HttpPost("sair")]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("acesso-negado")]
    public IActionResult AcessoNegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [AllowAnonymous]
    [HttpGet("sessao-expirada")]
    public IActionResult SessaoExpirada(string? returnUrl = null)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View();
    }

    private IActionResult RedirecionarLocalOuAdministracao(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Administracao")!;
    }
}
