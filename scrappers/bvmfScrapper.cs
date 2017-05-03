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
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using bvmfscrapper.exceptions;
using log4net;

namespace bvmfscrapper.scrappers
{

    public static class BvmfScrapper
    {
        public const string BASE_URL = "http://bvmf.bmfbovespa.com.br/";
        public const string LIST_URL = "cias-listadas/empresas-listadas/";
        const string SEGMENTO_MERCADO_BALCAO = "MB";
        const string DATETIME_MASK = @"dd/MM/yyyy HH\hmm";

        private static readonly ILog log = LogManager.GetLogger(typeof(BvmfScrapper));


        public static async Task<List<ScrappedCompany>> GetCompanies()
        {
            Console.WriteLine("Obtendo companhias listadas");
            log.Info("Obtendo companhias listadas");
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


            foreach (var c in companies)
            {
                try
                {
                    await FillCompanyData(c);
                }catch(UnavailableDataException ex)
                {
                    // ignore this company for now
                    continue;
                }

                string file = c.GetFileName();
                if (File.Exists(file))
                {
                    log.Info($"Arquivo da empresa {c.RazaoSocial} já existe. Verificando data");
                    Console.WriteLine("Empresa já extraída.. verificando....");

                    log.Info($"Carregando empresa do arquivo {file}");
                    var deserialized = ScrappedCompany.Load(file);
                    if (c.UltimaAtualizacao <= deserialized.UltimaAtualizacao)
                    {
                        log.Info($"Empresa já está atualizada. Data última atualização: {c.UltimaAtualizacao}");
                        Console.WriteLine("Empresa está atualizada. Pulando");
                        c.NeedsUpdate = false;
                        continue;
                    }
                }

                // save file
                log.Info("Salvando arquivo da empresa");
                c.Save();

            }

            return companies;
        }

        private static List<ScrappedCompany> ParseCompanies(string html)
        {
            log.Info("Extraindo lista de empresas");
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            //doc.LoadHtml(html);

            List<ScrappedCompany> companies = new List<ScrappedCompany>();

            var nodes = doc.QuerySelectorAll("tr.GridRow_SiteBmfBovespa, tr.GridAltRow_SiteBmfBovespa");
            foreach (var node in nodes)
            {
                var tds = node.ChildNodes.Where(n => n.NodeName.ToLowerInvariant() == "td").ToArray();
                var href = ((IElement)tds[0].FirstChild).Attributes["href"].Value.Trim();
                var razao = tds[0].TextContent.Trim();
                var nomepregao = tds[1].TextContent.Trim();
                var segmento = tds[2].TextContent.Trim();

                log.Info($"Extraindo Companhia: {razao}");
                log.Info($"nomepregao = {nomepregao}");
                log.Info($"href = {href}");
                log.Info($"segmento = {segmento}");

                if (segmento != SEGMENTO_MERCADO_BALCAO)
                {
                    var company = new ScrappedCompany();
                    company.RazaoSocial = razao.Trim();
                    company.NomePregao = nomepregao.Trim();
                    company.Segmento = segmento.Trim();
                    company.CodigoCVM = GetCodigoCvm(href);
                    companies.Add(company);
                    log.Info($"codigo cvm {company.CodigoCVM}");
                }
                else
                {
                    log.Info($"segmento {SEGMENTO_MERCADO_BALCAO}. Ignorando");
                }

            }

            return companies;
        }

        private static int GetCodigoCvm(string href)
        {
            var index = href.IndexOf("=", StringComparison.Ordinal);
            return Convert.ToInt32(href.Substring(index + 1));
        }

        private static async Task FillCompanyData(ScrappedCompany c)
        {
            Console.WriteLine($"Obtendo informações básicas de {c.RazaoSocial}");
            log.Info($"Obtendo informações básicas de {c.RazaoSocial}");
            var watch = Stopwatch.StartNew();

            var url = $"{BASE_URL}/pt-br/mercados/acoes/empresas/ExecutaAcaoConsultaInfoEmp.asp?CodCVM={c.CodigoCVM}&ViewDoc=0";
            var client = new HttpClient();
            var response = await client.GetStringWithRetryAsync(url);

            ParseBasicData(response, c);
            watch.Stop();
            Console.WriteLine($"dados obtidos em {watch.Elapsed.TotalSeconds} segundos");
        }

        private static void ParseBasicData(string html, ScrappedCompany c)
        {
            log.Info($"Extraindo informações básicas de {c.RazaoSocial}");
            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            //doc.LoadHtml(html);


            // first thing: verify if response is "Dados indisponiveis"
            var alert = doc.QuerySelector("div.alert-box");
            if(alert != null)
            {
                if(alert.TextContent.ToLowerInvariant().Contains("dados indisponiveis"))
                {
                    log.Error($"Dados de {c.RazaoSocial} não disponíveis.");
                    throw new UnavailableDataException();
                }
            }


            var rows = doc.QuerySelectorAll("div.row");

            IElement p = null;
            // first row is last update datetime
            p = rows[0].QuerySelector("p.legenda");

            // text = Atualizado em 28/04/2017, às 05h13
            var date = GetDate(p.TextContent.Trim());
            c.UltimaAtualizacao = date;
            log.Info($"Última atualização {c.UltimaAtualizacao.ToString("yyyy-MM-dd HH:mm:ss")}");

            // second row is basic company data
            var tableficha = rows[1].QuerySelector("table.ficha");
            var trs = tableficha.ChildNodes[1].ChildNodes.Where(n => n.NodeName.ToLowerInvariant() == "tr").OfType<IElement>().ToArray();

            // ignore first TR, we already have this info 
            var anchors = trs[1].QuerySelectorAll("td").Last().QuerySelectorAll("a.LinkCodNeg").Select(n => n.TextContent);
            c.CodigosNegociacao = new SortedSet<string>(anchors);
            log.Info($"Códigos de negociação {string.Join(",", c.CodigosNegociacao.ToArray())}");

            var cnpj = trs[2].QuerySelectorAll("td").Last().TextContent.Trim();
            c.CNPJ = cnpj;

            var mainActivity = trs[3].QuerySelectorAll("td").Last().TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(mainActivity))
            {
                c.AtividadePrincipal = (from item in mainActivity.Split('/')
                                        select item.Trim()).ToArray();
            }

            var setorialClassfications = trs[4].QuerySelectorAll("td").Last().TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(setorialClassfications))
            {
                c.ClassificacaoSetorial = (from item in setorialClassfications.Split('/')
                                           select item.Trim()).ToArray();
            }
            if (trs.Length > 5)
            {
                c.Site = trs[5].QuerySelectorAll("td").Last().TextContent.Trim();
            }
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
