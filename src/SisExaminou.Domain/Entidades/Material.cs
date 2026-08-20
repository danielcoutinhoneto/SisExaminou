namespace SisExaminou.Domain.Entidades
{
    public class Material
    {
        public int MaterialId { get; set; }
        public string Nome { get; set; } = null!;
        public string? Sigla { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? AtualizadoEmUtc { get; set; }
        public byte[] Versao { get; set; } = null!;

    }
}

