using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper.models
{
    public class Company
    {
        public string RazaoSocial { get; set; }
        public string NomePregao { get; set; }
        public string Segmento { get; set; }
        public int CodigoCVM { get; set; }
        public string CNPJ { get; set; }
        public string[] AtividadePrincipal { get; set; }
        public string[] ClassificacaoSetorial { get; set; }
        public string Site { get; set; }
        public SortedSet<string> CodigosNegociacao { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
    }
}
