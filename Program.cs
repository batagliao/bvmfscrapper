using bvmfscrapper.scrappers;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using bvmfscrapper.models;
using log4net;
using System.Xml;
using System.Reflection;
using bvmfscrapper.data.repositories;
using System.Text;

namespace bvmfscrapper
{
    class Program
    {
        public static string OUT_DIR = $"output{Path.DirectorySeparatorChar}basicdata{Path.DirectorySeparatorChar}";

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));


        static void Main(string[] args)
        {


            // Configure Windows console
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Configure log4net
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            log.Info("Aplicação iniciada. Log configurado");

            var options = Options.ParseOptions(args);
            //try
            //{


                Task.Run(async () =>
                {
                    List<ScrappedCompany> companies = null;
                    if (options.ShouldLoadCompanyList)
                    {
                        // step 1 - get companies basic data and links
                        log.Info("Iniciando a extração de empresas");
                        log.Info("---------------------------------");
                        companies = await BvmfScrapper.GetCompanies().ConfigureAwait(false);
                        log.Info("---------------------------------");
                        log.Info("Finalizada a extração de empresas");
                        log.Info("*********************************");
                    }

                    if (options.ShouldUpdateCompaniesIdDb)
                    {
                        // step 2 - save companies on database
                        log.Info("Iniciando a atualização de empresas no banco de dados");
                        log.Info("---------------------------------");
                        UpdateCompaniesInDatabase(companies);
                        log.Info("---------------------------------");
                        log.Info("Finalizada a atualização de empresas no banco de dados");
                        log.Info("*********************************");
                    }


                    if (options.ShouldExtractDocLinks)
                    {
                        // step 3 - get doc links
                        log.Info("Iniciando a extração de links de docs das empresas");
                        log.Info("---------------------------------");
                        await ExtractDocLinksAsync(companies);
                        log.Info("---------------------------------");
                        log.Info("Finalizada a extração de links de docs");
                        log.Info("*********************************");
                    }


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

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    log.Fatal(ex.ToString());
            //}
        }

        static void UpdateCompaniesInDatabase(List<ScrappedCompany> companies)
        {
            if (companies == null)
            {
                companies = ScrappedCompany.LoadCompaniesFromFiles(OUT_DIR);
            }

            EmpresaRepository.InsertOrUpdate(companies);
        }

        static async Task ExtractDocLinksAsync(List<ScrappedCompany> companies)
        {
            foreach (var c in companies)
            {
                log.Info($"Extraindo link da empresa {c.RazaoSocial}");
                var doclinks = await BvmfDocSummaryScrapper.GetDocsInfoReferences(c);
                //save links for company
                c.SaveDocLinks(doclinks);
            }
        }
    }
}