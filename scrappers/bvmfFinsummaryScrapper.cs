using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers
{

    public class BvmfFinsummaryScrapper
    {
        public static async Task GetFinancialInfoReferences(Company c)
        {
            Console.WriteLine("Obtendo referências das informações financeiras");


            // link: finacial reports
            // http://bvmf.bmfbovespa.com.br/cias-listadas/empresas-listadas/ResumoDemonstrativosFinanceiros.aspx?codigoCvm=21903&idioma=pt-br
            // __EVENTTARGET = ctl00$contentPlaceHolderConteudo$cmbAno
            // __VIEWSTATE
            // ctl00$contentPlaceHolderConteudo$cmbAno = 2009
            var client = new HttpClient();            
            var url = $"{BvmfScrapper.BASE_URL}{BvmfScrapper.LIST_URL}ResumoDemonstrativosFinanceiros.aspx?codigoCvm={c.CodigoCVM}&idioma=pt-br";

            var response = await client.GetStringAsync(url);

            // load years
            var viewstate = "";
            var years = ParseYears(response, out viewstate).ToList();

            foreach (var year in years)
            {
                // ITR - Informaçõe Trimestrais                
                // DFP - Demostrações Financeiras Padronizadas (Pré 2010; Pós 2010)
                // Formulário de Referência
                // Formulário Cadastral
                // Informações Anuais
                await GetDocLinks(c, year, viewstate);
            } 
        }

        private static List<int> ParseYears(string html, out string viewstate)
        {

            var parser = new HtmlParser();
            var doc = parser.Parse(html);
            //doc.LoadHtml(html);            

            // TODO: need to sabe the viewstate of this page
            // to be able to change the year

            viewstate = "";
            var vselement = doc.QuerySelector("#__VIEWSTATE");
            if (vselement != null)
            {
                var value = vselement.Attributes["value"].Value;
                viewstate = value;
            }

            List<int> years = new List<int>();

            var dropdown = doc.QuerySelector("select");
            var options = dropdown.ChildNodes.OfType<IElement>();
            foreach (var o in options)
            {
                var value = o.Attributes["value"].Value;
                if (value == "0")
                {
                    continue;
                }
                //Console.WriteLine($"year = {value}");
                years.Add(Convert.ToInt32(value));
            }
            years.Sort();
            return years;
        }

        private static async Task GetDocLinks(Company c, int year, string viewstate)
        {

            var url = $"{BvmfScrapper.BASE_URL}{BvmfScrapper.LIST_URL}ResumoDemonstrativosFinanceiros.aspx?codigoCvm={c.CodigoCVM}&idioma=pt-br";
            var payload = new Dictionary<string, string>
            {
                { "__EVENTTARGET", "ctl00$contentPlaceHolderConteudo$cmbAno" },
                {"__VIEWSTATE", viewstate},
                {"ctl00$contentPlaceHolderConteudo$cmbAno", year.ToString()}
            };
            var client = new HttpClient();
            var response = await client.PostDataAsync(url, payload);


            // ITR - Informaçõe Trimestrais                
            // DFP - Demostrações Financeiras Padronizadas (Pré 2010; Pós 2010)
            // Formulário de Referência
            // Formulário Cadastral
            // Informações Anuais
        }
    }
}