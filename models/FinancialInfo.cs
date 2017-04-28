using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper.models
{
    public enum Periodo
    {
        Anual,
        Trimestral
    }

    public class FinancialInfo
    {
        public Periodo Periodo { get; set; }

        public string IdentificadorPeriodo { get; set; }

        public string Link { get; set; }
    }
}
