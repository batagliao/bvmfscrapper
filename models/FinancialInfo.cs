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

    public enum FinInfoCategoria
    {
        Ativo,
        Passivo,
        DRE
    }

    public enum FinInfoTipo
    {
        Individual,
        Consolidado
    }

    public class FinancialInfo
    {
        public Periodo Periodo { get; set; }

        public DateTime Date { get; set; }

        public int Multiplicador { get; set; }

        public List<FinancialItem> Items { get; set; } = new List<FinancialItem>();
        
        public FinInfoCategoria Categoria { get; set; }

        public FinInfoTipo Tipo { get; set; }
    }

    public class FinancialItem
    {
        public string Conta { get; set; }
        public string Nome { get; set; }
        public decimal Valor { get; set; }
    }
}
