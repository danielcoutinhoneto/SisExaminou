using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SisExaminou.Application.Seguranca.Configuracao;

namespace SisExaminou.Web.Controllers;

[Authorize(Policy = CodigosSeguranca.PoliticaConteudoGerenciar)]
[Route("administracao")]
public sealed class AdministracaoController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();
}
