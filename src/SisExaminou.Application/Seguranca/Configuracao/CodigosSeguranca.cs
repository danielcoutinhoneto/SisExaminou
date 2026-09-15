namespace SisExaminou.Application.Seguranca.Configuracao;

public static class CodigosSeguranca
{
    public const string PerfilAdministrador = "ADMINISTRADOR";
    public const string PerfilTi = "TI";

    public const string PermissaoConteudoGerenciar = "CONTEUDO_GERENCIAR";
    public const string PermissaoUsuariosGerenciar = "USUARIOS_GERENCIAR";
    public const string PermissaoPerfisGerenciar = "PERFIS_GERENCIAR";

    public const string PoliticaConteudoGerenciar = "ConteudoGerenciar";
    public const string PoliticaUsuariosGerenciar = "UsuariosGerenciar";
    public const string PoliticaPerfisGerenciar = "PerfisGerenciar";

    public const string ClaimPerfil = "sisexaminou:perfil";
    public const string ClaimPermissao = "sisexaminou:permissao";
    public const string ClaimVersaoCredencial = "sisexaminou:versao_credencial";
    public const string ClaimValidadoEmUtc = "sisexaminou:validado_em_utc";
}
