using bvmfscrapper.models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bvmfscrapper
{
    public static class CompanyExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyExtensions));


        public static string GetFileName(this ScrappedCompany c)
        {
            string path = $"{Program.BASICDATA_DIR}{c.CodigoCVM}.json";
            return path;
        }

        public static string GetLinksFileName(this ScrappedCompany c)
        {
            string path = $"{Program.LINKS_DIR}{c.CodigoCVM}.json";
            return path;
        }

        public static void SaveDocLinks(this ScrappedCompany c, Dictionary<DocInfoType, List<DocLinkInfo>> links)
        {
            log.Info($"Salvando arquivo de links para a empresa {c.RazaoSocial}");

            var filename = c.GetLinksFileName();

            log.Info($"File={filename}");

            string json = JsonConvert.SerializeObject(links, Formatting.Indented);

            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filename, json);
        }

        public static Dictionary<DocInfoType, List<DocLinkInfo>> LoadDocLinksForCompany(this ScrappedCompany company)
        {
            var filename = company.GetLinksFileName();
            log.Info($"Carregando links da empresa {company.RazaoSocial} - {filename}");
            string json = File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<Dictionary<DocInfoType, List<DocLinkInfo>>>(json);
        }
    }
}
