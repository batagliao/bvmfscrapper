using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using bvmfscrapper.models;
using log4net;
using System.IO;
using System.Net;
using System.Net.Http;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;
using System.Globalization;

namespace bvmfscrapper.scrappers.findata
{
    public abstract class BovespaScrapperBase : IFinDataScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BovespaScrapperBase));

        public virtual ScrappedCompany Company { get; set; }

        public async Task ScrapBalancoAtivo(DocLinkInfo link, FinInfoTipo tipo)
        {
            log.Info($"Obtendo Balanço Ativo {tipo} - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(Company.GetFinDataFileName(link, FinInfoCategoria.Passivo, tipo));
            log.Info($"Verificando se é necessário extrair Balanço Ativo {tipo}");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Balanço Ativo {tipo}");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Balanço Ativo {tipo}");
                return;
            }

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=01 - Ativo";
            }
            else
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=01%20-%20Ativo";
            }
            var content = await GetStringWithCookiesAsync(cookies, url);

            var ativo = ParseFinInfo(content, FinInfoCategoria.Ativo, tipo);
            ativo.Save(fileinfo.FullName);
        }

        public async Task ScrapBalancoPassivo(DocLinkInfo link, FinInfoTipo tipo)
        {
            log.Info($"Obtendo Balanço Passivo {tipo} - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(Company.GetFinDataFileName(link, FinInfoCategoria.Passivo, tipo));
            log.Info($"Verificando se é necessário extrair Balanço Passivo {tipo}");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Balanço Passivo {tipo}");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Balanço Passivo {tipo}");
                return;
            }

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=02 - Passivo";
            }
            else
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=02%20-%20Passivo";
            }
            var content = await GetStringWithCookiesAsync(cookies, url);

            var passivo = ParseFinInfo(content, FinInfoCategoria.Passivo, tipo);
            passivo.Save(fileinfo.FullName);
        }

        public async Task ScrapDRE(DocLinkInfo link, FinInfoTipo tipo)
        {
            log.Info($"Obtendo DRE {tipo} - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(Company.GetFinDataFileName(link, FinInfoCategoria.DRE, tipo));
            log.Info($"Verificando se é necessário extrair DRE {tipo}");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - DRE {tipo}");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - DRE {tipo}");
                return;
            }

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=C";
            }
            else
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=T";
            }
            var content = await GetStringWithCookiesAsync(cookies, url);

            var dre = ParseFinInfo(content, FinInfoCategoria.DRE, tipo);
            dre.Save(fileinfo.FullName);
        }

        private FinancialInfo ParseFinInfo(string content, FinInfoCategoria categoria, FinInfoTipo tipo)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(content);

            FinancialInfo finInfo = new FinancialInfo();
            finInfo.Categoria = categoria;
            finInfo.Tipo = tipo;

            var table = doc.QuerySelector("div.ScrollMaker").FirstElementChild as IHtmlTableElement;
            //table anterior a table é a linha que contém o multiplicador
            var multiplierText = table.PreviousElementSibling.TextContent;
            if (multiplierText.Contains("Mil"))
            {
                finInfo.Multiplicador = 1000;
            }

            foreach (var row in table.Rows)
            {
                if (row.GetAttribute("valign") == "top") // linha de título
                {
                    // pega a data da terceira célula
                    // Valor do Trimestre Atual 01/04/2009 a 30/06/2009
                    var text = row.Cells[2].TextContent;
                    var iUltimoNum = text.LastIndexOfNum();
                    var start = iUltimoNum - 10;
                    var datetext = text.Substring(start, 10);
                    finInfo.Data = DateTime.ParseExact(datetext, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                }
                else
                {
                    var codconta = row.Cells[0].TextContent;
                    var nomeconta = row.Cells[1].TextContent;
                    var valortext = row.Cells[2].TextContent;
                    FinancialItem item = new FinancialItem();
                    item.Conta = codconta.Trim();
                    item.Nome = nomeconta.Trim();
                    item.Valor = ParseValor(valortext.Trim());
                    finInfo.Items.Add(item);
                }
            }

            return finInfo;
        }

        private decimal ParseValor(string valortext)
        {
            // ponto (.) separador de milhar
            // parenteses em volta, é negativo
            return decimal.Parse(valortext, NumberStyles.Currency |
                NumberStyles.AllowThousands, new CultureInfo("pt-BR"));
        }

        public async Task ScrapComposicaoCapital(DocLinkInfo link)
        {
            log.Info($"Obtendo Composição de Capital - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(Company.GetFinDataCapitalFileName(link));
            log.Info("Verificando se é necessário extrair Composição do Capital");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair ITR {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                return;
            }

            // deve primeiro acessar a url para obter os cookies
            var cookies = await GetCookiesForBovespaAsync(link);
            var url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWG1CompCapital.asp";
            var content = await GetStringWithCookiesAsync(cookies, url);

            var capital = ParseComposicaoCapital(content);
            capital.Save(fileinfo.FullName);
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
            if (td_multiplicador.TextContent.Contains("Milhão") || td_multiplicador.TextContent.Contains("Milhões"))
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
        protected abstract Task<IEnumerable<Cookie>> GetCookiesForBovespaAsync(DocLinkInfo info);

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
    }
}
