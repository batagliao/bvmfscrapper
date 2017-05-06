using System;
using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;

namespace bvmfscrapper.models
{
    public class DocLinkInfo
    {
        public enum LinkTypeEnum
        {
            Bovespa,
            CVM
        }
        public DateTime? Date { get; protected set; }
        public string Link { get; protected set; }
        public string Title { get; protected set; }
        public DocInfoType DocType { get; protected set; }
        public LinkTypeEnum LinkType { get; protected set; }
        public int NumeroSequencialDocumento { get; protected set; }

        public DocLinkInfo(DocInfoType doctype, IHtmlAnchorElement a)
        {
            Title = a.Text;
            DocType = doctype;
            Date = ParseDocLinkDate(a.Text);
            Link = ParseDocLinkHref(a.Href);
            LinkType = ParseLinkType(Link);
            if (LinkType == LinkTypeEnum.CVM)
            {
                NumeroSequencialDocumento = ParseNumSequencial(Link);
            }
        }



        private string ParseDocLinkHref(string value)
        {
            // javascript:AbreFormularioCadastral('http://www.rad.cvm.gov.br/ENETCONSULTA/frmGerenciaPaginaFRE.aspx?NumeroSequencialDocumento=60599&CodigoTipoInstituicao=2')
            // ref="javascript:ConsultarDXW('http://www2.bmfbovespa.com.br/dxw/FrDXW.asp?site=B&mercado=18&razao=FIBRIA CELULOSE S.A.&pregao=FIBRIA&ccvm=12793&data=31/03/2010&tipo=4'

            /* **************** */
            /*  ITR             */
            /* Quando o link for AbreFormulário cadastral o importante é a Data, CodigoCVM e IdDocumento
            /* Quando o link for ConsultarDXW >>> TODO: Descobrir
            /*
            /* **************** */
            /* DFP              */
            /* Quando o link for AbreFormulário cadastral o importante é a Data, CodigoCVM e IdDocumento
            /* Quando o link for ConsultarDXW >>> TODO: Descobrir
            /*
            /* **************** */
            /* FRE              */
            /* Obter somente o último com texto Ativo
            /* Guardar data
            /* TODO: Identificar o padrão de links da CVM            
            /* PODE SER IGNORADO POR HORA
            /* 
            /* **************** */
            /* FCA
            /* Obter somente o último com texto Ativo
            /* Guardar data
            /* TODO: Identificar o padrão de links da CVM            
            /* PODE SER IGNORADO POR HORA
            /* 
            /* **************** */
            /* IAN
            /* IGNORAR POR HORA
            /* 
            */



            int i = value.IndexOf("('");
            int li = value.LastIndexOf("')");
            int start = i + 2;
            int length = li - start;
            return value.Substring(start, length);
        }

        private DateTime? ParseDocLinkDate(string text)
        {
            var trimmed = text.Trim();
            switch (DocType)
            {
                case DocInfoType.ITR: // dia, mes e ano
                case DocInfoType.DFP:
                case DocInfoType.IAN:
                    var datetext = text.Substring(0, 10);
                    return DateTime.ParseExact(datetext, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                default:
                    return null;
            }
        }

        private LinkTypeEnum ParseLinkType(string link)
        {
            if (link.StartsWith("http://www.rad.cvm.gov.br"))
            {
                return LinkTypeEnum.CVM;
            }
            return LinkTypeEnum.Bovespa;
        }

        private int ParseNumSequencial(string link)
        {
            // http://www.rad.cvm.gov.br/ENETCONSULTA/frmGerenciaPaginaFRE.aspx?NumeroSequencialDocumento=60599&CodigoTipoInstituicao=2
            var subject = "NumeroSequencialDocumento=";
            var i = link.IndexOf(subject);
            if (i < 0)
            {
                throw new InvalidOperationException($"link não estava no formato correto. Não foi encontrado o índice de '{subject}'");
            }

            var iend = link.IndexOf('&', i);
            if (iend < 0) //not found, so goes to the end
            {
                iend = link.Length - 1;
            }

            var start = i + subject.Length;
            var num = link.Substring(start, iend - start);
            return Convert.ToInt32(num);
        }
    }
}