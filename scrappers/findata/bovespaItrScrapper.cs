using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using bvmfscrapper.models;
using log4net;
using System.IO;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;

namespace bvmfscrapper.scrappers.findata
{
    public class BovespaItrScrapper : IItrScrapper
    {
        private ScrappedCompany company;
        private static readonly ILog log = LogManager.GetLogger(typeof(BovespaItrScrapper));

        public BovespaItrScrapper(ScrappedCompany company)
        {
            this.company = company;
        }

        public async Task ScrapBalancoConsolidadoAtivo(DocLinkInfo link)
        {
            log.Info($"Obtendo Balanço consolidado (ativo) - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            log.Info($"Obtendo Balanço individual (ativo) - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            log.Info($"Obtendo Balanço consolidado (passivo) - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            log.Info($"Obtendo Balanço individual (passivo) - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            log.Info($"Obtendo Composição de Capital - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(company.GetFinDataCapitalFileName(link));
            log.Info("Verificando se é necessário extrair Composição do Capital");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                log.Info($"Empresa {company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                return;
            }
            

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWG1CompCapital.asp";
            var content = await GetStringWithCookiesAsync(cookies, url);

            var capital = ParseComposicaoCapital(content);
            capital.Save(fileinfo.FullName);
        }



        public async Task ScrapDREConsolidado(DocLinkInfo link)
        {
            log.Info($"Obtendo DRE Consolidado - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            log.Info($"Obtendo DRE Individual - {company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
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
            var data = info.Data.ToString("dd/MM/yyyy");

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


        private ComposicaoCapital ParseComposicaoCapital(string content)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(content);

            ComposicaoCapital capital = new ComposicaoCapital();
            var tds_titles = doc.QuerySelectorAll("td.label");

            var td_multiplicador = tds_titles[0];
            var td_date = tds_titles[2];

            if (td_multiplicador.TextContent.Contains("Mil") || td_multiplicador.TextContent.Contains("Milhares"))
            {
                capital.Multiplicador = 1000;
            }
            if(td_multiplicador.TextContent.Contains("Milhão") || td_multiplicador.TextContent.Contains("Milhões"))
            {
                capital.Multiplicador = 1000 * 1000;
            }

            var tr_title = td_multiplicador.ParentElement;
            // walk the rows

            var tr = tr_title.NextElementSibling as IHtmlTableRowElement;
            if (tr.Cells[0].TextContent.Contains("Integralizado"))
            {
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Ordinárias mercado
            if (tr.Cells[0].TextContent.Contains("Ordinárias"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.OrdinariasCirculantes = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToInt32(text));
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Preferenciais mercado
            if (tr.Cells[0].TextContent.Contains("Preferenciais"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.PrefernciaisCirculantes = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToInt32(text));
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            if (tr.Cells[0].TextContent.Contains("Tesouraria"))
            {
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Ordinárias tesouraria
            if (tr.Cells[0].TextContent.Contains("Ordinárias"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.OrdinariasTesouraria = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToInt32(text));
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Preferenciais tesouraria
            if (tr.Cells[0].TextContent.Contains("Preferenciais"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.PreferenciaisTesouraria = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToInt32(text));
                //tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            return capital;
        }

    }
}