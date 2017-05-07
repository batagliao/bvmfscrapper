using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using bvmfscrapper.models;
using bvmfscrapper.scrappers.findata;
using log4net;
using Newtonsoft.Json;
using static bvmfscrapper.models.DocLinkInfo;

namespace bvmfscrapper.scrappers
{
    public static class FinancialDataScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyExtensions));

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
            foreach(var itr in itrs)
            {
                //TODO: check before everything if it needs to be extracted
                var itrScrapper = CreateItrScrapper(itr, company);
                await itrScrapper.ScrapComposicaoCapital(itr);
                await itrScrapper.ScrapBalancoIndividualAtivo(itr);
                await itrScrapper.ScrapBalancoIndividualPassivo(itr);
                await itrScrapper.ScrapDREIndividual(itr);
                await itrScrapper.ScrapBalancoConsolidadoAtivo(itr);
                await itrScrapper.ScrapBalancoConsolidadoPassivo(itr);
                await itrScrapper.ScrapDREConsolidado(itr);
            }
            


            // extract DFP
            Console.WriteLine($"Extraindo informações de DFP da empresa {company.RazaoSocial}");
            log.Info($"Extraindo informações de DFP da empresa {company.RazaoSocial}");
            var dfps = links[DocInfoType.DFP];
            foreach (var dfp in dfps)
            {
                //TODO: check before everything if it needs to be extracted
                var dfpScrapper = CreateDFPScrapper(dfp, company);
                await dfpScrapper.ScrapComposicaoCapital(dfp);
                await dfpScrapper.ScrapBalancoIndividualAtivo(dfp);
                await dfpScrapper.ScrapBalancoIndividualPassivo(dfp);
                await dfpScrapper.ScrapDREIndividual(dfp);
                await dfpScrapper.ScrapBalancoConsolidadoAtivo(dfp);
                await dfpScrapper.ScrapBalancoConsolidadoPassivo(dfp);
                await dfpScrapper.ScrapDREConsolidado(dfp);
            }
        }

        private static IDfpScrapper CreateDFPScrapper(DocLinkInfo dfp, ScrappedCompany company)
        {
            if (dfp.LinkType == LinkTypeEnum.Bovespa)
                return new BovespaDfpScrapper(company);

            return new CvmDfpScrapper(company);
        }

        private static IItrScrapper CreateItrScrapper(DocLinkInfo itr, ScrappedCompany company)
        {
            if(itr.LinkType == LinkTypeEnum.Bovespa)
                return new BovespaItrScrapper(company);

            return new CvmItrScrapper(company);
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