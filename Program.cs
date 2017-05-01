using bvmfscrapper.scrappers;
using System;
using System.Threading.Tasks;
using System.IO;

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

                // step 1 - get companies basic data and links
                var companies = await BvmfScrapper.GetCompanies().ConfigureAwait(false);

                // step 2 - parse the aditional links
                // load files and operate over them

                // historico cotacoes
                // http://bvmf.bmfbovespa.com.br/sig/FormConsultaMercVista.asp?strTipoResumo=RES_MERC_VISTA&strSocEmissora=ITSA&strDtReferencia=03-2017&strIdioma=P&intCodNivel=2&intCodCtrl=160#

            }).GetAwaiter().GetResult();
        }


    }
}