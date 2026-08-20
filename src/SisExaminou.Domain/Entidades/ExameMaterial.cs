namespace SisExaminou.Domain.Entidades
{
    public class ExameMaterial
    {
        public Exame ExameId { get; set; } = new Exame();
        public Material MaterialId { get; set; } = new Material();
        public bool Principal { get; set; } = false;
        public string Observacao { get; set; } = string.Empty;
        public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    }
}
