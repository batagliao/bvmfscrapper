using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace bvmfscrapper
{
    public class Options
    {       
        public static Options Instance { get; protected set; }

        // Carrega arquivos de opções, caso exista
        static Options()
        {
            string file = "options.json";
            if(File.Exists(file))
            {
                string json = File.ReadAllText(file);
                var optionsInstance = JsonConvert.DeserializeObject<Options>(json);
                Instance = optionsInstance;
            }else{
                Instance = new Options();
            }
        }
       
        public bool LoadCompanyList { get; set; } = true;
        public bool UpdateCompaniesIdDb { get; set; } = true;
        public bool ExtractDocLinks { get; set; } = true;
        public bool ExtractFinData { get; set; } = true;
        public int Company { get; set; } = 0;
    }
}
