using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bvmfscrapper.data
{
    [Table("Empresa")]
    public class Empresa
    {
        [Key]
        public int Codigo { get; set; }

        [Required]
        public string RazaoSocial { get; set; }

        [Required]
        public string NomePregao { get; set; }

        public string Segmento { get; set; }

        [Required]
        public string CNPJ { get; set; }

        public string AtividadePrincipal { get; set; }

        public string ClassificacaoSetorial { get; set; }

        public string Site { get; set; }

        [Required]
        public DateTime UltimaAtualizacao { get; set; }

        public List<Ticker> Tickers { get; set; }
    }
}
