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
        private static string OUT_DIR = $"output{Path.DirectorySeparatorChar}";
        public static string BASICDATA_DIR = $"{OUT_DIR}basicdata{Path.DirectorySeparatorChar}";
        public static string LINKS_DIR = $"{OUT_DIR}links{Path.DirectorySeparatorChar}";

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        // olhar esse ftp e ver se existe algo útil:
        // ftp://ftp.bmf.com.br

        // MARKET DATA
        // ftp://ftp.bmf.com.br/MarketData/

        // isso pode ser interessante
        // https://github.com/pedrocordeiro/bovespa

        // aqui é possível obter as cotações
        // http://pregao-online.bmfbovespa.com.br/Cotacoes.aspx?idioma=pt-BR

        // CVM - Todos os documentos 
        // http://siteempresas.bovespa.com.br/consbov/ExibeTodosDocumentosCVM.asp?CCVM=5410&CNPJ=84.429.695/0001-11&TipoDoc=C

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
            if (companies == null)
            {
                companies = ScrappedCompany.LoadCompaniesFromFiles(BASICDATA_DIR);
            }

            int counter = 1;
            foreach (var c in companies)
            {
                bool shouldExtract = true;
                // Só deve extrair os links e/ou sobrepor o arquivo da empresa se a data de atualização(company.UltimaAtualizacao) for
                // maior que a data do arquivo (FileTime)
                // ou se o arquivo não existir

                var info = new FileInfo(c.GetLinksFileName());
                if (info.Exists && info.CreationTime > c.UltimaAtualizacao)
                {
                    shouldExtract = false;
                }

                if(!shouldExtract){
                    Console.WriteLine($"Empresa {c.RazaoSocial} não necessita extrair links");
                    log.Info($"Empresa {c.RazaoSocial} não necessita extrair links");
                    continue;
                }

                Console.WriteLine($"Extraindo link da empresa {c.RazaoSocial} -- {counter}/{companies.Count}");
                counter += 1;
                log.Info($"Extraindo link da empresa {c.RazaoSocial}");
                var doclinks = await BvmfDocSummaryScrapper.GetDocsInfoReferences(c);
                //save links for company
                c.SaveDocLinks(doclinks);
            }
        }
    }
}