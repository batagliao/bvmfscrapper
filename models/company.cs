using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        public bool NeedsUpdate { get; set; } = true; // by default needs to be extracted


        public void Save()
        {
            string path = this.GetFileName();

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Console.WriteLine("Salvando arquivo");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static Company Load(string filename)
        {
            
            string filecontent = File.ReadAllText(filename);
            Company deserialized = JsonConvert.DeserializeObject<Company>(filecontent);
            return deserialized;
        }

        

        //public Dictionary<DocInfoType, List<DocLinkInfo>> DocLinks { get; internal set; }
    }
}
