using SisExaminou.Infrastructure.Seguranca;

namespace SisExaminou.Tests.Infrastructure;

public sealed class SenhaHasherTests
{
    [Fact]
    public void Hash_GeradoNaoContemTextoClaroEValidaSomenteASenhaCorreta()
    {
        const string senha = "uma frase senha longa de teste";
        var hasher = new SenhaHasher();

        var hash = hasher.GerarHash(senha);

        Assert.DoesNotContain(senha, hash, StringComparison.Ordinal);
        Assert.True(hasher.Verificar(hash, senha));
        Assert.False(hasher.Verificar(hash, "senha incorreta"));
        Assert.False(hasher.Verificar("hash malformado", senha));
        Assert.False(hasher.Verificar(null, senha));
    }
}
