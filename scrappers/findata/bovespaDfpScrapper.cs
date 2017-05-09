using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using bvmfscrapper.models;
using log4net;
using System.Net.Http;
using System.Linq;

namespace bvmfscrapper.scrappers.findata
{
    public class BovespaDfpScrapper : BovespaScrapperBase, IDfpScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BovespaItrScrapper));

        public BovespaDfpScrapper(ScrappedCompany company)
        {
            this.Company = company;
        }

        protected override async Task<IEnumerable<Cookie>> GetCookiesForBovespaAsync(DocLinkInfo info)
        {
            // codificar para url
            var razao = WebUtility.UrlEncode(Company.RazaoSocial);
            var pregao = WebUtility.UrlEncode(Company.NomePregao);
            var data = info.Data.ToString("dd/MM/yyyy");

            var url = $"http://www2.bmfbovespa.com.br/dxw/FrDXW.asp?site=B&mercado=18&razao={razao}&pregao={pregao}&ccvm={Company.CodigoCVM}&data={data}&tipo=2";
            log.Info($"Acessando url para obtenção dos cookies");
            log.Info($"url = {url}");

            var container = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = container;

            var client = new HttpClient(handler);
            var response = await client.GetAsync(url);
            var cookies = container.GetCookies(new Uri(url)).Cast<Cookie>().ToList();

            return await Task.FromResult(cookies);
        }
    }
}