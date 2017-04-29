using bvmfscrapper.models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using AngleSharp;

namespace bvmfscrapper.scrappers
{

    public class BvmfScrapper
    {
        public const string BASE_URL = "http://bvmf.bmfbovespa.com.br/";
        public const string LIST_URL = "cias-listadas/empresas-listadas/";
        const string SEGMENTO_MERCADO_BALCAO = "MB";
        const string DATETIME_MASK = @"dd/MM/yyyy HH\hmm";
        public static async Task<List<Company>> GetCompanies()
        {
            Console.WriteLine("Obtendo companhias listadas");
            var watch = Stopwatch.StartNew();

            var payload = new Dictionary<string, string>
            {
                { "__EVENTTARGET","ctl00:contentPlaceHolderConteudo:BuscaNomeEmpresa1:btnTodas" }
            };

            string url = $"{BASE_URL}{LIST_URL}BuscaEmpresaListada.aspx?idioma=pt-br";
            var client = new HttpClient();
            var response = await client.PostDataAsync(url, payload);
            var companies = ParseCompanies(response);

            watch.Stop();
            Console.WriteLine($"{companies.Count} companhias encontradas em {watch.Elapsed.TotalSeconds} segundos");


            foreach (var c in companies.Take(1))
            {
                await FillCompanyData(c);
                await BvmfFinsummaryScrapper.GetFinancialInfoReferences(c);

                // ITR - Informaçõe Trimestrais                
                // DFP - Demostrações Financeiras Padronizadas (Pré 2010; Pós 2010)
                // Formulário de Referência
                // Formulário Cadastral
                // Informações Anuais


                // aba Informações relevantes
                // aba Eventos Corporativos
                // - proventos em dinheiro
                // - proventos em ativos
                // - subscrição
                // - grupamento
                // - desdobramento

                // Históricos de cotações

            }

            return companies;
        }

        
        private static List<Company> ParseCompanies(string html)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            //doc.LoadHtml(html);

            List<Company> companies = new List<Company>();

            var nodes = doc.QuerySelectorAll("tr.GridRow_SiteBmfBovespa, tr.GridAltRow_SiteBmfBovespa");
            foreach (var node in nodes)
            {
                var tds = node.ChildNodes.Where(n => n.NodeName.ToLowerInvariant() == "td").ToArray();
                var href = ((IElement)tds[0].FirstChild).Attributes["href"].Value.Trim();
                var razao = tds[0].TextContent.Trim();
                var nomepregao = tds[1].TextContent.Trim();
                var segmento = tds[2].TextContent.Trim();

                if (segmento != SEGMENTO_MERCADO_BALCAO)
                {
                    var company = new Company();
                    company.RazaoSocial = razao;
                    company.NomePregao = nomepregao;
                    company.Segmento = segmento;
                    company.CodigoCVM = GetCodigoCvm(href);
                    companies.Add(company);
                }
            }

            return companies;
        }

        private static int GetCodigoCvm(string href)
        {
            var index = href.IndexOf("=", StringComparison.Ordinal);
            return Convert.ToInt32(href.Substring(index + 1));
        }

        private static async Task FillCompanyData(Company c)
        {
            Console.WriteLine($"Obtendo informações básicas de {c.RazaoSocial}");
            var watch = Stopwatch.StartNew();

            var url = $"{BASE_URL}/pt-br/mercados/acoes/empresas/ExecutaAcaoConsultaInfoEmp.asp?CodCVM={c.CodigoCVM}&ViewDoc=0";
            var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            ParseBasicData(response, c);
            watch.Stop();
            Console.WriteLine($"dados obtidos em {watch.Elapsed.TotalSeconds} segundos");
        }

        private static void ParseBasicData(string html, Company c)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            //doc.LoadHtml(html);

            var rows = doc.QuerySelectorAll("div.row");

            // first row is last update datetime
            var p = rows[0].QuerySelector("p.legenda");
            // text = Atualizado em 28/04/2017, às 05h13
            var date = GetDate(p.TextContent.Trim());
            c.UltimaAtualizacao = date;

            // second row is basic company data
            var tableficha = rows[1].QuerySelector("table.ficha");
            var trs = tableficha.ChildNodes[1].ChildNodes.Where(n => n.NodeName.ToLowerInvariant() == "tr").OfType<IElement>().ToArray();

            // ignore first TR, we already have this info 
            var anchors = trs[1].QuerySelectorAll("td").Last().QuerySelectorAll("a.LinkCodNeg").Select(n => n.TextContent);
            c.CodigosNegociacao = new SortedSet<string>(anchors);

            var cnpj = trs[2].QuerySelectorAll("td").Last().TextContent.Trim();
            c.CNPJ = cnpj;

            var mainActivity = trs[3].QuerySelectorAll("td").Last().TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(mainActivity))
            {
                c.AtividadePrincipal = mainActivity.Split('/');
            }

            var setorialClassfications = trs[4].QuerySelectorAll("td").Last().TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(setorialClassfications))
            {
                c.ClassificacaoSetorial = setorialClassfications.Split('/');
            }

            c.Site = trs[5].QuerySelectorAll("td").Last().TextContent.Trim();
        }

        private static DateTime GetDate(string text)
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
            var timepart = text.Substring(i - 4, 5);

            var datetime = DateTime.ParseExact($"{datepart} {timepart}", DATETIME_MASK, new CultureInfo("pt-BR"));
            return datetime;

        }

        
    }
}
