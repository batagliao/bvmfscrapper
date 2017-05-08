using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper.models
{
    public class ComposicaoCapital
    {
        public Periodo Periodo { get; set; }

        public DateTime Date { get; set; }

        public int Multiplicador { get; set; }

        public decimal PrefernciaisCirculantes { get; set; }
        public decimal OrdinariasCirculantes { get; set; }
        public decimal PreferenciaisTesouraria { get; set; }
        public decimal OrdinariasTesouraria { get; set; }
    }
}
