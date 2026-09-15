using SisExaminou.Application.Seguranca.CasosDeUso;
using SisExaminou.Application.Seguranca.Excecoes;
using SisExaminou.Application.Seguranca.Modelos;
using SisExaminou.Infrastructure.Persistence.SqlServer.Seguranca;
using SisExaminou.Infrastructure.Seguranca;

const string nomeVariavelConexao = "SISEXAMINOU_BOOTSTRAP_CONNECTION_STRING";

Console.WriteLine("Bootstrap explícito do primeiro administrador do SisExaminou");
Console.WriteLine("A senha não será exibida, registrada nem recebida por argumento.");
Console.WriteLine();

if (Console.IsInputRedirected)
{
    Console.Error.WriteLine(
        "A entrada interativa é obrigatória. Execute o comando em um terminal.");
    return 2;
}

var connectionString = Environment.GetEnvironmentVariable(nomeVariavelConexao);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine(
        $"Defina a variável de ambiente {nomeVariavelConexao} somente na sessão atual.");
    return 2;
}

Console.Write("Nome completo: ");
var nomeCompleto = Console.ReadLine();
Console.Write("Login: ");
var login = Console.ReadLine();
Console.Write("E-mail opcional: ");
var email = Console.ReadLine();

var senha = LerSegredo("Frase-senha (15 a 128 caracteres): ");
var confirmacao = LerSegredo("Confirme a frase-senha: ");

if (!string.Equals(senha, confirmacao, StringComparison.Ordinal))
{
    Console.Error.WriteLine("As frases-senha não conferem. Nenhum usuário foi criado.");
    return 2;
}

try
{
    var repository = new AutenticacaoRepository(connectionString);
    var useCase = new CriarPrimeiroAdministradorUseCase(
        repository,
        new SenhaHasher(),
        TimeProvider.System);

    var resultado = await useCase.ExecutarAsync(
        new CriarAdministradorComando(nomeCompleto, login, email, senha),
        CancellationToken.None);

    if (resultado.Status == StatusBootstrapAdministrador.Criado)
    {
        Console.WriteLine("Primeiro administrador criado com segurança.");
        return 0;
    }

    if (resultado.Status == StatusBootstrapAdministrador.AdministradorAtivoJaExiste)
    {
        Console.WriteLine(
            "Já existe um administrador ativo. Nenhuma alteração foi realizada.");
        return 3;
    }

    Console.Error.WriteLine("Revise os dados informados:");
    foreach (var erro in resultado.Erros)
    {
        foreach (var mensagem in erro.Value)
        {
            Console.Error.WriteLine($"- {erro.Key}: {mensagem}");
        }
    }

    return 2;
}
catch (SegurancaIndisponivelException)
{
    Console.Error.WriteLine(
        "Não foi possível acessar o banco. Confirme a conexão e as migrações.");
    return 1;
}
catch (ArgumentException)
{
    Console.Error.WriteLine(
        "A configuração de conexão é inválida. Revise-a sem expor seu valor.");
    return 1;
}
finally
{
    senha = string.Empty;
    confirmacao = string.Empty;
}

static string LerSegredo(string prompt)
{
    Console.Write(prompt);
    var caracteres = new List<char>();

    while (true)
    {
        var tecla = Console.ReadKey(intercept: true);
        if (tecla.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return new string([.. caracteres]);
        }

        if (tecla.Key == ConsoleKey.Backspace)
        {
            if (caracteres.Count > 0)
            {
                caracteres.RemoveAt(caracteres.Count - 1);
            }

            continue;
        }

        if (!char.IsControl(tecla.KeyChar) && caracteres.Count < 128)
        {
            caracteres.Add(tecla.KeyChar);
        }
    }
}
