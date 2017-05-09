using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bvmfscrapper.models
{
   
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
        private static readonly ILog log = LogManager.GetLogger(typeof(FinancialInfo));

        public DateTime Data { get; set; }

        public int Multiplicador { get; set; }

        public List<FinancialItem> Items { get; set; } = new List<FinancialItem>();
        
        public FinInfoCategoria Categoria { get; set; }

        public FinInfoTipo Tipo { get; set; }

        public void Save(string filename)
        {
            log.Info($"Salvando arquivo de {Categoria} {Tipo} - {filename}");
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }

    public class FinancialItem
    {
        public string Conta { get; set; }
        public string Nome { get; set; }
        public decimal? Valor { get; set; }
    }
}
