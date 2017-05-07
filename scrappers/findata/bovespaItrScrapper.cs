using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using bvmfscrapper.models;
using log4net;

namespace bvmfscrapper.scrappers.findata
{
    public class BovespaItrScrapper : IItrScrapper
    {
        private ScrappedCompany company;
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyExtensions));

        public BovespaItrScrapper(ScrappedCompany company)
        {
            this.company = company;
        }
            
        public async Task ScrapBalancoConsolidadoAtivo(DocLinkInfo link)
        {
            log.Info($"Obtendo Balanço consolidado (ativo) - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=01 - Ativo";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseBalancoAtivo(content);

            //TODO: save file
        }

        public async Task ScrapBalancoIndividualAtivo(DocLinkInfo link)
        {
            log.Info($"Obtendo Balanço individual (ativo) - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=01 - Ativo";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseBalancoAtivo(content);

            //TODO: save file
        }

        private void ParseBalancoAtivo(Task<string> content)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        public async Task ScrapBalancoConsolidadoPassivo(DocLinkInfo link)
        {
            log.Info($"Obtendo Balanço consolidado (passivo) - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=02%20-%20Passivo";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseBalancoPassivo(content);

            //TODO: save file
        }

        public async Task ScrapBalancoIndividualPassivo(DocLinkInfo link)
        {
            log.Info($"Obtendo Balanço individual (passivo) - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=02 - Passivo";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseBalancoPassivo(content);

            //TODO: save file
        }

        private void ParseBalancoPassivo(Task<string> content)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        public async Task ScrapComposicaoCapital(DocLinkInfo link)
        {
            log.Info($"Obtendo Composição de Capital - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWG1CompCapital.asp";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseComposicaoCapital(content);

            //TODO: save file
        }



        public async Task ScrapDREConsolidado(DocLinkInfo link)
        {
            log.Info($"Obtendo DRE Consolidado - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=T";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseDRE(content);

            //TODO: save file
        }

        public async Task ScrapDREIndividual(DocLinkInfo link)
        {
            log.Info($"Obtendo DRE Individual - {company.RazaoSocial} - {link.Date.Value.ToString("dd/MM/yyyy")}");
            //TODO: check if it needs to be scrapped

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=C";
            var content = GetStringWithCookiesAsync(cookies, url);

            ParseDRE(content);

            //TODO: save file
        }

        private void ParseDRE(Task<string> content)
        {
            //TODO: implement
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<Cookie>> GetCookiesForBovespaAsync(DocLinkInfo info)
        {
            // codificar para url
            var razao = WebUtility.UrlEncode(company.RazaoSocial);
            var pregao = WebUtility.UrlEncode(company.NomePregao);
            var data = info.Date.Value.ToString("dd/MM/yyyy");

            var url = $"http://www2.bmfbovespa.com.br/dxw/FrDXW.asp?site=B&mercado=18&razao={razao}&pregao={pregao}&ccvm={company.CodigoCVM}&data={data}&tipo=";
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

        private async Task<string> GetStringWithCookiesAsync(IEnumerable<Cookie> cookies, string url)
        {
            // Obtém o conteúdo da URL passando os cookies
            log.Info($"Chamando a url {url} passando cookies");
            var container = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = container;
            var client = new HttpClient(handler);

            foreach (var cookie in cookies)
            {
                log.Info($"Cookie name: {cookie.Name}; value: {cookie.Value}");
                handler.CookieContainer.Add(new Uri(url), cookie);
            }

            return await client.GetStringAsync(url);

        }


        private void ParseComposicaoCapital(Task<string> content)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

    }
}