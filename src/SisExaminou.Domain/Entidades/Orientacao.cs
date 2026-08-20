using SisExaminou.Domain.Entidades;

namespace SisExaminou.Domain.Entidades
{
    public class Orientacao
    {
        public int OrientacaoId { get; set; }
        public Exame ExameId { get; set; } = new Exame();
        public string Titulo { get; set; } = string.Empty;
        public string TipoOrientacao { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public int OrdemExibicao { get; set; } = 1;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEmUtc { get; set; } = null;
        public byte[] Versao { get; set; } = new byte[0];
    }
}

