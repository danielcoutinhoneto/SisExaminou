namespace SisExaminou.Domain.Entidades
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? AtualizadoEmUtc { get; set; } = DateTime.MinValue;
        public byte[] Versao { get; set; } = new byte[0];
    }
}

