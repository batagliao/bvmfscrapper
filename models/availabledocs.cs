using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper.models
{
    public class AvailableDocs
    {
        public bool ComposicaoCapital { get; set; } = true;
        public bool AtivoIndividual { get; set; } = true;
        public bool PassivoIndividual { get; set; } = true;
        public bool DREIndividual { get; set; } = true;
        public bool AtivoConsolidado { get; set; } = true;
        public bool PassivoConsolidado { get; set; } = true;
        public bool DREConsolidado { get; set; } = true;
    }
}
