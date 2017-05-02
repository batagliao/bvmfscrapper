using bvmfscrapper.scrappers;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using bvmfscrapper.models;

namespace bvmfscrapper
{
    class Program
    {
        public static string OUT_DIR = $"output{Path.DirectorySeparatorChar}basicdata{Path.DirectorySeparatorChar}";

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {

                // TODO: implement log4net
                // TODO: accept argumento to start on step 2 and load info from companies files

                // step 1 - get companies basic data and links
                var companies = await BvmfScrapper.GetCompanies().ConfigureAwait(false);

                // step 2 - get doc links
                await ExtractDocLinksAsync(companies);


                // step 2 - parse the aditional links
                // load files and operate over them

                // aba Informações relevantes
                // aba Eventos Corporativos
                // - proventos em dinheiro
                // - proventos em ativos
                // - subscrição
                // - grupamento
                // - desdobramento

                // Históricos de cotações

                // historico cotacoes
                // http://bvmf.bmfbovespa.com.br/sig/FormConsultaMercVista.asp?strTipoResumo=RES_MERC_VISTA&strSocEmissora=ITSA&strDtReferencia=03-2017&strIdioma=P&intCodNivel=2&intCodCtrl=160#

            }).GetAwaiter().GetResult();
        }

        static async Task ExtractDocLinksAsync(List<Company> companies)
        {
            foreach (var c in companies.Where(c => c.NeedsUpdate))
            {
                var doclinks = await BvmfDocSummaryScrapper.GetDocsInfoReferences(c);
                //save links for company
                c.SaveDocLinks(doclinks);
            }
        }
    }
}