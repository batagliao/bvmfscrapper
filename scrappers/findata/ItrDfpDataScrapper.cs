using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using bvmfscrapper.models;
using log4net;

namespace bvmfscrapper.scrappers.findata
{
    public class ItrDfpDataScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ItrDfpDataScrapper));

        public ScrappedCompany Company { get; private set; }
        public IEnumerable<Cookie> CookiesBovespa { get; private set; }
        public IEnumerable<Cookie> CookiesCvm { get; private set; }

        public ItrDfpDataScrapper(ScrappedCompany company)
        {
            this.Company = company;
        }


        public async Task ScrapDoc(DocLinkInfo link, FinInfoTipo tipo, FinInfoCategoria categoria)
        {
            log.Info($"Obtendo {categoria} {tipo} - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");
            Console.WriteLine($"Obtendo {categoria} {tipo} - {Company.RazaoSocial} - {link.Data.ToString("dd/MM/yyyy")}");

            bool shouldExtract = true;
            // se não existir o arquivo ou
            // se o arquivo existir mas a data de entrega do arquivo for menor que a data do arquivo
            var fileinfo = new FileInfo(Company.GetFinDataFileName(link, FinInfoCategoria.Passivo, tipo));
            log.Info($"Verificando se é necessário extrair {categoria} {tipo}");
            Console.WriteLine($"Verificando se é necessário extrair {categoria} {tipo}");
            if (fileinfo.Exists && fileinfo.CreationTime > link.DataApresentacao)
            {
                shouldExtract = false;
            }

            if (!shouldExtract)
            {
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair {link.DocType} {link.Data.ToString("dd/MM/yyyy")} -{categoria} {tipo}");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair {link.DocType} {link.Data.ToString("dd/MM/yyyy")} - {categoria}{tipo}");
                return;
                await Task.CompletedTask;
            }

            var url = "";
            if (categoria == FinInfoCategoria.Ativo)
            {
                url = GetBalancoAtivoUrl(link, tipo);
            }
            if (categoria == FinInfoCategoria.Passivo)
            {
                url = GetBalancoPassivoUrl(link, tipo);
            }
            if (categoria == FinInfoCategoria.DRE)
            {
                url = GetDREUrl(link, tipo);
            }

            var content = await GetAsync(link, url);

            var fininfo = ParseFinInfo(content, link.LinkType, categoria, tipo);
            fininfo.Save(fileinfo.FullName);
        }

        private string GetBalancoAtivoUrl(DocLinkInfo link, FinInfoTipo tipo)
        {

            // deve primeiro acessar a url para obter os cookies
            var url = "";
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                // itr ou dfp - mesma url
                if (tipo == FinInfoTipo.Individual)
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=01 - Ativo";
                }
                else //consolidado
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=01%20-%20Ativo";
                }
            }
            else // cvm
            {
                var codTipoDocumento = 0;
                if (link.DocType == DocInfoType.ITR)
                {
                    codTipoDocumento = 3;
                }
                if (link.DocType == DocInfoType.DFP)
                {
                    codTipoDocumento = 4;
                }
                // o numero sequencial diz se o documento é itr ou dfp
                if (tipo == FinInfoTipo.Individual)
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=2&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
                else
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=2&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
            }

            return url;
        }

        private string GetBalancoPassivoUrl(DocLinkInfo link, FinInfoTipo tipo)
        {
            var url = "";
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                // itr e dfp são o mesmo link
                if (tipo == FinInfoTipo.Individual)
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=C&Tipo=02 - Passivo";
                }
                else
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWBalanco.asp?TipoInfo=T&Tipo=02%20-%20Passivo";
                }
            }
            else //cvm
            {
                var codTipoDocumento = 0;
                if (link.DocType == DocInfoType.ITR)
                {
                    codTipoDocumento = 3;
                }
                if (link.DocType == DocInfoType.DFP)
                {
                    codTipoDocumento = 4;
                }

                if (tipo == FinInfoTipo.Individual)
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=3&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
                else
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=3&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
            }
            return url;
        }

        private string GetDREUrl(DocLinkInfo link, FinInfoTipo tipo)
        {
            var url = "";
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                // itr e dfp tem a mesma url
                if (tipo == FinInfoTipo.Individual)
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=C";
                }
                else
                {
                    url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWDRE.asp?TipoInfo=T";
                }
            }
            else //cvm
            {
                var codTipoDocumento = 0;
                if (link.DocType == DocInfoType.ITR)
                {
                    codTipoDocumento = 3;
                }
                if (link.DocType == DocInfoType.DFP)
                {
                    codTipoDocumento = 4;
                }

                if (tipo == FinInfoTipo.Individual)
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=4&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
                else
                {
                    url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=4&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
                }
            }
            return url;
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
                Console.WriteLine($"Empresa {Company.RazaoSocial} não necessita extrair {link.DocType} {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                log.Info($"Empresa {Company.RazaoSocial} não necessita extrair {link.DocType} {link.Data.ToString("dd/MM/yyyy")} - Capital Consolidado");
                return;
            }

            var url = "";
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                url = "http://www2.bmfbovespa.com.br/dxw/FormDetalheDXWG1CompCapital.asp";
            }
            else //cvm
            {
                var codTipoDocumento = 0;
                if (link.DocType == DocInfoType.ITR)
                {
                    codTipoDocumento = 3;
                }
                if (link.DocType == DocInfoType.DFP)
                {
                    codTipoDocumento = 4;
                }
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDadosComposicaoCapitalITR.aspx?Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={codTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }

            var content = await GetAsync(link, url);

            ComposicaoCapital capital = null;
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                capital = ParseComposicaoCapitalBovespa(content);
            }
            else //cvm
            {
                capital = ParseComposicaoCapitalCvm(content);
            }

            capital.Save(fileinfo.FullName);
        }

        private ComposicaoCapital ParseComposicaoCapitalBovespa(string content)
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
                capital.OrdinariasCirculantes = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToDecimal(text));
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Preferenciais mercado
            if (tr.Cells[0].TextContent.Contains("Preferenciais"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.PrefernciaisCirculantes = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToDecimal(text));
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
                capital.OrdinariasTesouraria = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToDecimal(text));
                tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            // Preferenciais tesouraria
            if (tr.Cells[0].TextContent.Contains("Preferenciais"))
            {
                var text = tr.Cells[1].TextContent.Trim();
                capital.PreferenciaisTesouraria = (string.IsNullOrWhiteSpace(text) ? 0 : Convert.ToDecimal(text));
                //tr = tr.NextElementSibling as IHtmlTableRowElement;
            }

            return capital;
        }

        private ComposicaoCapital ParseComposicaoCapitalCvm(string content)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(content);

            ComposicaoCapital capital = new ComposicaoCapital();
            var div = doc.QuerySelector("#UltimaTabela");
            var table = div.Children.Where(e => e.LocalName == "table").First() as IHtmlTableElement;

            bool tesouraria = false;
            foreach (var row in table.Rows)
            {
                if (row.Cells[0].TextContent.Contains("Ações")) //primeira linha
                {
                    //obtem a data da segunda célula
                    capital.Date = DateTime.ParseExact(row.Cells[1].TextContent.Trim(), "dd/MM/yyyy", new CultureInfo("pt-BR"));
                }
                if (row.Cells[0].TextContent.Contains("Capital")) //segunda linha
                {
                    // acoes no mercado
                    tesouraria = false;
                    continue;
                }
                if (row.Cells[0].TextContent.Contains("Tesouraria"))
                {
                    // acoes em tesouraria
                    tesouraria = true;
                    continue;
                }
                if (row.Cells[0].TextContent.Contains("Ordinárias"))
                {
                    var value = Convert.ToDecimal(row.Cells[1].TextContent.Trim());
                    if (tesouraria)
                    {
                        capital.OrdinariasTesouraria = value;
                    }
                    else
                    {
                        capital.OrdinariasCirculantes = value;
                    }
                }
                if (row.Cells[0].TextContent.Contains("Preferenciais"))
                {
                    var value = Convert.ToDecimal(row.Cells[1].TextContent.Trim());
                    if (tesouraria)
                    {
                        capital.PreferenciaisTesouraria = value;
                    }
                    else
                    {
                        capital.PrefernciaisCirculantes = value;
                    }
                }
            }

            return capital;
        }

        private async Task<string> GetAsync(DocLinkInfo link, string url)
        {
            IEnumerable<Cookie> cookies = null;
            // é necessário obter os cookies antes da chamada
            if (link.LinkType == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                if (CookiesBovespa == null)
                {
                    CookiesBovespa = await GetCookiesForBovespaAsync(link);
                }
                cookies = CookiesBovespa;
            }
            else //cvm
            {
                if(CookiesCvm == null)
                {
                    CookiesCvm = await GetCookiesForCvmAsync(link);
                }
                cookies = CookiesCvm;
            }

            var container = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = container;
            var client = new HttpClient(handler);

            foreach (var cookie in cookies)
            {
                log.Info($"Cookie name: {cookie.Name}; value: {cookie.Value}");
                handler.CookieContainer.Add(new Uri(url), cookie);
            }

            return await client.GetStringWithRetryAsync(url);
        }

        private async Task<IEnumerable<Cookie>> GetCookiesForBovespaAsync(DocLinkInfo info)
        {
            // codificar para url
            var razao = WebUtility.UrlEncode(Company.RazaoSocial);
            var pregao = WebUtility.UrlEncode(Company.NomePregao);
            var data = info.Data.ToString("dd/MM/yyyy");

            int tipo = 0;
            if (info.DocType == DocInfoType.ITR)
            {
                tipo = 4;
            }
            if (info.DocType == DocInfoType.DFP)
            {
                tipo = 2;
            }

            var url = $"http://www2.bmfbovespa.com.br/dxw/FrDXW.asp?site=B&mercado=18&razao={razao}&pregao={pregao}&ccvm={Company.CodigoCVM}&data={data}&tipo={tipo}";

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

        private async Task<IEnumerable<Cookie>> GetCookiesForCvmAsync(DocLinkInfo info)
        {
            var url = "http://www.rad.cvm.gov.br/ENETCONSULTA/frmGerenciaPaginaFRE.aspx?NumeroSequencialDocumento=65179&CodigoTipoInstituicao=2";
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

        private FinancialInfo ParseFinInfo(string content, DocLinkInfo.LinkTypeEnum linktype, FinInfoCategoria categoria, FinInfoTipo tipo)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(content);

            FinancialInfo finInfo = new FinancialInfo();
            finInfo.Categoria = categoria;
            finInfo.Tipo = tipo;


            IHtmlTableElement table = null;
            if (linktype == DocLinkInfo.LinkTypeEnum.Bovespa)
            {
                var div = doc.QuerySelector("div.ScrollMaker");
                table = div.FirstElementChild as IHtmlTableElement;
                //table anterior a table é a linha que contém o multiplicador
                var multiplierText = div.PreviousElementSibling.TextContent;
                if (multiplierText.Contains("Mil"))
                {
                    finInfo.Multiplicador = 1000;
                }
            }
            else //cvm
            {
                var title = doc.QuerySelector("#TituloTabelaSemBorda");
                if (title.TextContent.Contains("Reais Mil"))
                {
                    finInfo.Multiplicador = 1000;
                }

                table = title.NextElementSibling as IHtmlTableElement;
            }

            foreach (var row in table.Rows)
            {
                bool isTopLine = false;
                if (linktype == DocLinkInfo.LinkTypeEnum.Bovespa)
                {
                    isTopLine = row.GetAttribute("valign") == "top";
                }
                else
                {
                    isTopLine = row.Cells[0].TextContent.Trim() == "Conta";
                }

                if (isTopLine) // linha de título
                {
                    // pega a data da terceira célula
                    // Valor do Trimestre Atual 01/04/2009 a 30/06/2009
                    var text = row.Cells[2].TextContent;
                    var iUltimoNum = text.LastIndexOfNum();
                    var start = iUltimoNum - 9;
                    var datetext = text.Substring(start, 10).Trim();
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

        private decimal? ParseValor(string valortext)
        {
            // ponto (.) separador de milhar
            // parenteses em volta, é negativo
            if (string.IsNullOrWhiteSpace(valortext))
                return null;

            return decimal.Parse(valortext, NumberStyles.Currency |
                NumberStyles.AllowThousands, new CultureInfo("pt-BR"));
        }
    }
}
