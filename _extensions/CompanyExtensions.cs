using bvmfscrapper.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bvmfscrapper
{
    public static class CompanyExtensions
    {
        public static string GetFileName(this Company c)
        {
            string path = $"{Program.OUT_DIR}{c.CodigoCVM}.json";
            return path;
        }

        public static void SaveDocLinks(this Company c, Dictionary<DocInfoType, List<DocLinkInfo>> links)
        {
            var filename = c.GetFileName();
            filename = Path.ChangeExtension(filename, ".links.json");
            string json = JsonConvert.SerializeObject(links, Formatting.Indented);

            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filename, json);
        }
    }
}
