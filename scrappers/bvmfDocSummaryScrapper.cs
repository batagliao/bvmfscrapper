using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers
{
    public class BvmfDocSummaryScrapper
    {

        public static async Task GetDocsInfoReferences(Company c)
        {
            Console.WriteLine("Obtendo links para históricos de documentos");


            // link: finacial reports
            // http://bvmf.bmfbovespa.com.br/cias-listadas/empresas-listadas/ResumoDemonstrativosFinanceiros.aspx?codigoCvm=21903&idioma=pt-br
            // __EVENTTARGET = ctl00$contentPlaceHolderConteudo$cmbAno
            // __VIEWSTATE
            // ctl00$contentPlaceHolderConteudo$cmbAno = 2009
            var client = new HttpClient();            
            var url = $"{BvmfScrapper.BASE_URL}{BvmfScrapper.LIST_URL}ResumoDemonstrativosFinanceiros.aspx?codigoCvm={c.CodigoCVM}&idioma=pt-br";

            var response = await client.GetStringAsync(url);

            //var sectionsLinks = GetSectionHistoricLinks(response);

            // informações trimestrais            
            await GetDocumentsLinks(DocInfoType.ITR, c);
            // demonstrações financeiras padronizadas
            await GetDocumentsLinks(DocInfoType.DFP, c);
            // Formulário de Referência
            await GetDocumentsLinks(DocInfoType.FRE, c);
            // Formulário Cadastral
            await GetDocumentsLinks(DocInfoType.FCA, c);
            // Informe Trimestral de Securitzadora
            await GetDocumentsLinks(DocInfoType.SEC, c);
            // Informações Anuais
            await GetDocumentsLinks(DocInfoType.IAN, c);

        }       

        // private static Dictionary<DocInfoType, string> GetSectionHistoricLinks(string html)
        // {
        //     // ITR - Informaçõe Trimestrais                
        //     // DFP - Demostrações Financeiras Padronizadas (Pré 2010; Pós 2010)
        //     // Formulário de Referência
        //     // Formulário Cadastral
        //     // Informações Anuais
        //     Dictionary<DocInfoType, string> histLinksDict = new Dictionary<DocInfoType, string>();

        //     var parser = new HtmlParser();
        //     var doc = parser.Parse(html);

        //     var sections = doc.QuerySelectorAll("h3").OfType<IElement>();

        //     foreach(var section in sections)
        //     {
        //         var section_title = section.TextContent;

        //         var list = GetSectionList(section);
        //         var divs = list.Children.Where(e => e.TagName == "div" && e.Attributes["class"].Value != "divider");

        //         // Historico é sempre o último
        //         var histDiv = divs.Last();
        //         var a = histDiv.QuerySelector("a");
        //         var link = a.Attributes["href"].Value;
        //         var sectionType = GetSectionType(section.TextContent);
        //         histLinksDict.Add(sectionType, link);
        //     }

        //     return histLinksDict;
        // }

        private static DocInfoType GetSectionType(string title){
            var lowered = title.ToLower();
            if(lowered.Contains("informações trimestrais")){
                return DocInfoType.ITR;
            }
            if(lowered.Contains("demonstrações financeiras padronizadas")){
                return DocInfoType.DFP;
            }
            if(lowered.Contains("formulário de referência")){
                return DocInfoType.FRE;
            }
            if(lowered.Contains("formulário cadastral")){
                return DocInfoType.FCA;
            }
            if(lowered.Contains("informações anuais")){
                return DocInfoType.IAN;
            }
            return DocInfoType.Unknow;
        }

        private static async Task GetDocumentsLinks(DocInfoType docType, Company c)
        {
            // HistoricoFormularioReferencia.aspx?codigoCVM=6017&tipo=itr&ano=0
            var histUrl = $"HistoricoFormularioReferencia.aspx?codigoCVM={c.CodigoCVM}&tipo={docType.ToString().ToLower()}&ano=0";

            // name = key
            // kink = value
            Console.WriteLine($"Obtendo lista de documentos {docType}");
            var url = $"{BvmfScrapper.BASE_URL}{BvmfScrapper.LIST_URL}/{histUrl}";
            var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var parser = new HtmlParser();
            var doc = parser.Parse(response);

            // o segundo h4 que contém o que desejamos. O primeiro é o título da página
            var h4 = doc.QuerySelectorAll("h4").Skip(1).Take(1).Single();
            var list = GetSectionList(h4);
            var rows = list.Children.Where(e => e.TagName == "div" && e.Attributes["class"].Value != "divider");

            foreach (var div in rows)
            {
                // date, text, link
                var a = div.QuerySelector("a");
                var text = a.TextContent;
                var date = ParseDocLinkDate(text, docType);
                var link = ParseDocLinkHref(a.Attributes["href"].Value, docType);
            }
      
        }

        private static string ParseDocLinkHref(string value, DocInfoType docType)
        {
            // javascript:AbreFormularioCadastral('http://www.rad.cvm.gov.br/ENETCONSULTA/frmGerenciaPaginaFRE.aspx?NumeroSequencialDocumento=60599&CodigoTipoInstituicao=2')
            int i = value.IndexOf("('");
            int li = value.LastIndexOf("')");
            int start = i + 2;
            int length = li - start;
            return value.Substring(start, length);
        }

        private static DateTime ParseDocLinkDate(string text, DocInfoType docType)
        {
            var trimmed = text.Trim();
            switch (docType)
            {
                case DocInfoType.ITR: // dia, mes e ano
                    var datetext = text.Substring(0, 10);
                    return DateTime.ParseExact(datetext, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                default:
                    return DateTime.MinValue;
            }
        }

        private static IElement GetSectionList(IElement section)
        {
            IElement list = null;
            bool found = false;
            do
            {
                list = section.NextElementSibling;
                if(list.NodeName == "DIV" && list.HasAttribute("class")){
                    if(list.Attributes["class"].Value == "list-avatar"){
                        found = true;
                    }
                }
            }
            while (!found);
            return list;
        }

        

    }
}