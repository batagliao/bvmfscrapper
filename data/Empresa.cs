using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using bvmfscrapper.models;

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

        [Column("Segmento")]
        public string SegmentoValue { get; set; }

        [NotMapped]
        public SegmentoEnum Segmento 
        {
            get { return SegmentoEnumExtensions.FromString(SegmentoValue); }
            set { SegmentoValue = value.AsString(); }
        }

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
