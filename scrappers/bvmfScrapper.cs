using bvmfscrapper.models;
using HtmlAgilityPack.CssSelectors.NetCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace bvmfscrapper.scrappers
{

    public class BvmfScrapper
    {
        const string BASE_URL = "http://bvmf.bmfbovespa.com.br/";
        const string LIST_URL = "cias-listadas/empresas-listadas/";
        const string SEGMENTO_MERCADO_BALCAO = "MB";
        const string DATETIME_MASK = @"dd/MM/yyyy HH\hmm";

        public async static Task<List<Company>> GetCompanies()
        {
            Console.WriteLine("Obtendo companhias listadas");
            var watch = Stopwatch.StartNew();

            var payload = new Dictionary<string, string>
            {
                { "__EVENTTARGET","ctl00:contentPlaceHolderConteudo:BuscaNomeEmpresa1:btnTodas" }
            };

            string url = $"{BASE_URL}{LIST_URL}BuscaEmpresaListada.aspx?idioma=pt-br";
            var client = new System.Net.Http.HttpClient();
            var response = await client.PostDataAsync(url, payload);
            var companies = ParseCompanies(response);

            watch.Stop();
            Console.WriteLine($"{companies.Count} companhias encontradas em {watch.Elapsed.TotalSeconds} segundos");

            foreach (var c in companies)
            {
                fillCompanyData(c);
            }

            return companies;
        }

        private static List<Company> ParseCompanies(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            List<Company> companies = new List<Company>();

            var nodes = doc.QuerySelectorAll("tr.GridRow_SiteBmfBovespa, tr.GridAltRow_SiteBmfBovespa");
            foreach (var node in nodes)
            {
                var tds = node.ChildNodes.Where(n => n.Name == "td").ToArray();
                var href = tds.First().FirstChild.Attributes["href"].Value.Trim();
                var razao = tds.First().InnerText.Trim();
                var nomepregao = tds[1].InnerText.Trim();
                var segmento = tds[2].InnerText.Trim();

                if (segmento != SEGMENTO_MERCADO_BALCAO)
                {
                    var company = new Company();
                    company.RazaoSocial = razao;
                    company.NomePregao = nomepregao;
                    company.Segmento = segmento;
                    company.CodigoCVM = getCodigoCVM(href);
                    companies.Add(company);
                }
            }

            return companies;
        }

        private static int getCodigoCVM(string href)
        {
            var index = href.IndexOf("=");
            return Convert.ToInt32(href.Substring(index + 1));
        }

        private static async Task fillCompanyData(Company c)
        {
            Console.WriteLine($"Obtendo informações básicas de {c.RazaoSocial}");
            var watch = Stopwatch.StartNew();

            var url = $"{BASE_URL}/pt-br/mercados/acoes/empresas/ExecutaAcaoConsultaInfoEmp.asp?CodCVM={c.CodigoCVM}&ViewDoc=0";
            var client = new System.Net.Http.HttpClient();
            var response = await client.GetStringAsync(url);

            parseBasicData(response, c);
            watch.Stop();
            Console.WriteLine($"dados obtidos em {watch.Elapsed.TotalSeconds} segundos");
        }

        private static void parseBasicData(string html, Company c)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.QuerySelectorAll("div.rows");

            // first row is last update datetime
            var p = rows[0].QuerySelector("p.legenda");
            // text = Atualizado em 28/04/2017, às 05h13
            var date = getDate(p.InnerText.Trim());
            c.UltimaAtualizacao = date;

            // second row is basic company data
            var tableficha = rows[1].QuerySelector("table.ficha");
            var trs = tableficha.ChildNodes.Where(n => n.Name == "tr").ToArray();

            // ignore first TR, we already have this info 
            var anchors = trs[1].QuerySelectorAll("td").Last().QuerySelectorAll("a.LinkCodNeg").Select(n => n.InnerText);
            c.CodigosNegociacao = new SortedSet<string>(anchors);

            var cnpj = trs[2].QuerySelectorAll("td").Last().InnerText.Trim();
            c.CNPJ = cnpj;

            var mainActivity = trs[3].QuerySelectorAll("td").Last().InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(mainActivity))
            {
                c.AtividadePrincipal = mainActivity.Split('/');
            }

            var setorialClassfications = trs[4].QuerySelectorAll("td").Last().InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(setorialClassfications))
            {
                c.ClassificacaoSetorial = setorialClassfications.Split('/');
            }

            c.Site = trs[5].QuerySelectorAll("td").Last().InnerText.Trim();
        }

        private static DateTime getDate(string text)
        {
            // find first num that appears on string
            // count 10 positions
            // parse date
            var i = text.IndexOfNum();
            var datepart = text.Substring(i, 10);

            // find last num on string
            // count 5 positions backwards
            // parse hour
            i = text.LastIndexOfNum();
            var timepart = text.Substring(i - 5, 5);

            var datetime = DateTime.ParseExact($"{datepart} {timepart}", DATETIME_MASK, new CultureInfo("pt-BR"));
            return datetime;

        }
    }
}
