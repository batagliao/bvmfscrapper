using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bvmfscrapper.data
{
    public class Ticker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }
        
        [Required]
        public int CodigoCVM { get; set; }

        [ForeignKey(nameof(CodigoCVM))]
        public Empresa Empresa { get; set; }
    }
}
