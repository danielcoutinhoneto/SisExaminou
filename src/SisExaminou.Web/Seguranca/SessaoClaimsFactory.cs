using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using SisExaminou.Application.Seguranca.Configuracao;
using SisExaminou.Application.Seguranca.Modelos;

namespace SisExaminou.Web.Seguranca;

public static class SessaoClaimsFactory
{
    public static ClaimsPrincipal Criar(
        SessaoUsuarioDto sessao,
        DateTimeOffset validadoEmUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sessao.UsuarioId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, sessao.NomeCompleto),
            new(CodigosSeguranca.ClaimPerfil, sessao.CodigoPerfil),
            new(
                CodigosSeguranca.ClaimVersaoCredencial,
                sessao.VersaoCredencial.ToString(CultureInfo.InvariantCulture)),
            new(
                CodigosSeguranca.ClaimValidadoEmUtc,
                validadoEmUtc.ToString("O", CultureInfo.InvariantCulture))
        };

        claims.AddRange(sessao.Permissoes.Select(
            permissao => new Claim(CodigosSeguranca.ClaimPermissao, permissao)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            CodigosSeguranca.ClaimPerfil);

        return new ClaimsPrincipal(identity);
    }
}
