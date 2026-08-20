namespace SisExaminou.Domain.Entidades
{
    public class ExameSinonimo
    {
        public int ExameSinonimoId { get; set; }
        public Exame ExameId { get; set; } = new Exame();
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? AtualizadoEmUtc { get; set; } = DateTime.MinValue;
        public byte[] Versao { get; set; } = new byte[0];
    }
}


