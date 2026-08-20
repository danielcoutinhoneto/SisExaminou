namespace SisExaminou.Domain.Entidades
{
    public class Exame
    {
        public int ExameId { get; set; }
        public TipoExame TipoExameId { get; set; } = new TipoExame();
        public Categoria CategoriaId { get; set; } = new Categoria();
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string NomePopular { get; set; } = string.Empty;
        public string DescricaoPopular { get; set; } = string.Empty;
        public string PrazoResultado { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? AtualizadoEmUtc { get; set; } = DateTime.MinValue;
        public byte[] Versao { get; set; } = new byte[0];
    }
}


