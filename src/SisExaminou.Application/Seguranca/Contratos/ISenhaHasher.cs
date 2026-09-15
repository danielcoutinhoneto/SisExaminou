namespace SisExaminou.Application.Seguranca.Contratos;

public interface ISenhaHasher
{
    string GerarHash(string senha);
    bool Verificar(string? senhaHash, string senhaInformada);
}
