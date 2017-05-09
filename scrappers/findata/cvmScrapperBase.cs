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
using System.Linq;

namespace bvmfscrapper.scrappers.findata
{
    public abstract class CvmScrapperBase : IFinDataScrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CvmScrapperBase));

        public virtual ScrappedCompany Company { get; set; }

        public abstract short CodTipoDocumento { get; }

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

            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=2&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }
            else
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=2&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }

            var client = new HttpClient();
            var content = await client.GetStringWithRetryAsync(url);

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
                        
            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=3&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }
            else
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=3&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }

            var client = new HttpClient();
            var content = await client.GetStringWithRetryAsync(url);

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

                        
            var url = "";
            if (tipo == FinInfoTipo.Individual)
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=1&Demonstracao=4&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }
            else
            {
                url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDemonstracaoFinanceiraITR.aspx?Informacao=2&Demonstracao=4&Periodo=0&Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            }

            var client = new HttpClient();
            var content = await client.GetStringWithRetryAsync(url);

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

            var title = doc.QuerySelector("#TituloTabelaSemBorda");
            if (title.TextContent.Contains("Reais Mil"))
            {
                finInfo.Multiplicador = 1000;
            }

            var table = title.NextElementSibling as IHtmlTableElement;            

            foreach (var row in table.Rows)
            {
                if (row.Cells[0].TextContent.Trim() == "Conta") // linha de título
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

        private decimal? ParseValor(string valortext)
        {
            // ponto (.) separador de milhar
            // parenteses em volta, é negativo
            if(string.IsNullOrWhiteSpace(valortext))
                return null;

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

            //TODO: DFP deve mudar CodDocumento para 4
            var url = $"https://www.rad.cvm.gov.br/ENETCONSULTA/frmDadosComposicaoCapitalITR.aspx?Grupo=&Quadro=&NomeTipoDocumento=&Titulo=&Empresa=&DataReferencia=&Versao=&CodTipoDocumento={CodTipoDocumento}&NumeroSequencialDocumento={link.NumeroSequencialDocumento}&NumeroSequencialRegistroCvm={Company.CodigoCVM}&CodigoTipoInstituicao=2";
            var client = new HttpClient();
            var content = await client.GetStringWithRetryAsync(url);

            var capital = ParseComposicaoCapital(content);
            capital.Save(fileinfo.FullName);
        }

        private ComposicaoCapital ParseComposicaoCapital(string content)
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

    }
}
