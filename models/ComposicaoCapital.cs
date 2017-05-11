using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bvmfscrapper.models
{
    public class ComposicaoCapital
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ComposicaoCapital));

        public DateTime Date { get; set; }

        public int Multiplicador { get; set; }

        public decimal PrefernciaisCirculantes { get; set; }
        public decimal OrdinariasCirculantes { get; set; }
        public decimal PreferenciaisTesouraria { get; set; }
        public decimal OrdinariasTesouraria { get; set; }

        public void Save(string filename)
        {
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            log.Info($"Salvando arquivo de Composição de Capital - {filename}");
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
