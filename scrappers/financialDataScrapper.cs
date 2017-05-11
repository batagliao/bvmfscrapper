using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using bvmfscrapper.models;
using bvmfscrapper.scrappers.findata;
using log4net;
using Newtonsoft.Json;
using static bvmfscrapper.models.DocLinkInfo;
using System.Linq;
using MoreLinq;

namespace bvmfscrapper.scrappers
{
    public static class FinancialDataScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FinancialDataScrapper));

        public static async Task ExtractFinancialInfo(ScrappedCompany company)
        {
            // load links
            var links = LoadLinks(company);
            if(links == null)
            {
                await Task.FromResult<object>(null);
            }

            // extract ITR
            Console.WriteLine($"Extraindo informações de ITR da empresa {company.RazaoSocial}");
            log.Info($"Extraindo informações de ITR da empresa {company.RazaoSocial}");
            var itrs = links[DocInfoType.ITR];
            itrs = RemoveDuplicates(itrs);

            foreach(var itr in itrs)
            {
                var scrapper = new ItrDfpDataScrapper(company);
                await scrapper.ScrapComposicaoCapital(itr);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Individual, FinInfoCategoria.Ativo);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Individual, FinInfoCategoria.Passivo);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Individual, FinInfoCategoria.DRE);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Consolidado, FinInfoCategoria.Ativo);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Consolidado, FinInfoCategoria.Passivo);
                await scrapper.ScrapDoc(itr, FinInfoTipo.Consolidado, FinInfoCategoria.DRE);
            }

            // extract DFP
            Console.WriteLine($"Extraindo informações de DFP da empresa {company.RazaoSocial}");
            log.Info($"Extraindo informações de DFP da empresa {company.RazaoSocial}");
            var dfps = links[DocInfoType.DFP];
            dfps = RemoveDuplicates(dfps);
            foreach (var dfp in dfps)
            {
                var scrapper = new ItrDfpDataScrapper(company);
                await scrapper.ScrapComposicaoCapital(dfp);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Individual, FinInfoCategoria.Ativo);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Individual, FinInfoCategoria.Passivo);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Individual, FinInfoCategoria.DRE);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Consolidado, FinInfoCategoria.Ativo);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Consolidado, FinInfoCategoria.Passivo);
                await scrapper.ScrapDoc(dfp, FinInfoTipo.Consolidado, FinInfoCategoria.DRE);
            }
        }

        private static List<DocLinkInfo> RemoveDuplicates(List<DocLinkInfo> links)
        {
            List<DocLinkInfo> filteredLinks = new List<DocLinkInfo>();
            // primeiro encontra os duplicados agrupando
            var groups = links.GroupBy(l => l.Data);

            // verifica os grupos para encontrar os duplicados e remove o mais antigo da lista principal
            foreach (var group in groups)
            {
                if(group.Count() > 1)
                {
                    var itemPermanece = group.MaxBy(g => g.DataApresentacao);
                    filteredLinks.Add(itemPermanece);
                }
                else
                {
                    filteredLinks.Add(group.First());
                }
            }
            return filteredLinks;
        }

        private static Dictionary<DocInfoType, List<DocLinkInfo>> LoadLinks(ScrappedCompany company)
        {
            var file = company.GetLinksFileName();
            if(!File.Exists(file))
            {
                Console.WriteLine($"arquivo de links da empresa {company.RazaoSocial} não existe");
                log.Error($"arquivo de links da empresa {company.RazaoSocial} não existe");
                return null;
            }
            string content = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<Dictionary<DocInfoType, List<DocLinkInfo>>>(content);
        }
    }
}