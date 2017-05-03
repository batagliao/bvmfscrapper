using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using bvmfscrapper.models;
using log4net;

namespace bvmfscrapper.scrappers
{
    public class BvmfDocSummaryScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BvmfDocSummaryScrapper));

        public static async Task<Dictionary<DocInfoType, List<DocLinkInfo>>> GetDocsInfoReferences(ScrappedCompany c)
        {
            Console.WriteLine("Obtendo históricos de documentos");
            log.Info($"Obtendo históricos de documentos - {c.RazaoSocial}");

            // link: finacial reports
            // http://bvmf.bmfbovespa.com.br/cias-listadas/empresas-listadas/ResumoDemonstrativosFinanceiros.aspx?codigoCvm=21903&idioma=pt-br
            // __EVENTTARGET = ctl00$contentPlaceHolderConteudo$cmbAno
            // __VIEWSTATE
            // ctl00$contentPlaceHolderConteudo$cmbAno = 2009
            var client = new HttpClient();            
            var url = $"{BvmfScrapper.BASE_URL}{BvmfScrapper.LIST_URL}ResumoDemonstrativosFinanceiros.aspx?codigoCvm={c.CodigoCVM}&idioma=pt-br";

            var response = await client.GetStringAsync(url);

            var docLinks = new Dictionary<DocInfoType, List<DocLinkInfo>>();

            // informações trimestrais            
            var itrs = await GetDocumentsLinks(DocInfoType.ITR, c);
            docLinks.Add(DocInfoType.ITR, itrs);
            // demonstrações financeiras padronizadas
            var dfps = await GetDocumentsLinks(DocInfoType.DFP, c);
            docLinks.Add(DocInfoType.DFP, dfps);
            // Formulário de Referência
            var fres = await GetDocumentsLinks(DocInfoType.FRE, c);
            docLinks.Add(DocInfoType.FRE, fres);
            // Formulário Cadastral
            var fcas = await GetDocumentsLinks(DocInfoType.FCA, c);
            docLinks.Add(DocInfoType.FCA, fcas);            
            // Informe Trimestral de Securitzadora
            var secs = await GetDocumentsLinks(DocInfoType.SEC, c);
            docLinks.Add(DocInfoType.SEC, secs);
            // Informações Anuais
            var ians = await GetDocumentsLinks(DocInfoType.IAN, c);
            docLinks.Add(DocInfoType.IAN, ians);

            return docLinks;
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

        // private static DocInfoType GetSectionType(string title){
        //     var lowered = title.ToLower();
        //     if(lowered.Contains("informações trimestrais")){
        //         return DocInfoType.ITR;
        //     }
        //     if(lowered.Contains("demonstrações financeiras padronizadas")){
        //         return DocInfoType.DFP;
        //     }
        //     if(lowered.Contains("formulário de referência")){
        //         return DocInfoType.FRE;
        //     }
        //     if(lowered.Contains("formulário cadastral")){
        //         return DocInfoType.FCA;
        //     }
        //     if(lowered.Contains("informações anuais")){
        //         return DocInfoType.IAN;
        //     }
        //     return DocInfoType.Unknow;
        // }

        private static async Task<List<DocLinkInfo>> GetDocumentsLinks(DocInfoType docType, ScrappedCompany c)
        {
            log.Info($"Obtendo documentos do tipo {docType} - {c.RazaoSocial}");
            // HistoricoFormularioReferencia.aspx?codigoCVM=6017&tipo=itr&ano=0
            var histUrl = $"HistoricoFormularioReferencia.aspx?codigoCVM={c.CodigoCVM}&tipo={docType.ToString().ToLower()}&ano=0";

            var docs = new List<DocLinkInfo>();

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
            var anchors = list.QuerySelectorAll("a");

            foreach (var a in anchors)
            {
                // date, text, link
                //var a = (IHtmlAnchorElement)div.QuerySelector("a");
                docs.Add(new DocLinkInfo(docType, (IHtmlAnchorElement)a));
            }
            return docs;
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