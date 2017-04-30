using System;
using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;

namespace bvmfscrapper.models
{
    public class DocLinkInfo
    {
        
        public DateTime? Date { get; protected set; }
        public string Link { get; protected set; }
        public string Title{ get; protected set; }

        public DocInfoType DocType{ get; protected set; }

        public DocLinkInfo(DocInfoType doctype, IHtmlAnchorElement a)
        {
            Title = a.Text;
            DocType = doctype;
            Date = ParseDocLinkDate(a.Text);
            Link = ParseDocLinkHref(a.Href);
        }

        private string ParseDocLinkHref(string value)
        {
            // javascript:AbreFormularioCadastral('http://www.rad.cvm.gov.br/ENETCONSULTA/frmGerenciaPaginaFRE.aspx?NumeroSequencialDocumento=60599&CodigoTipoInstituicao=2')
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
                    var datetext = text.Substring(0, 10);
                    return DateTime.ParseExact(datetext, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                default:
                    return null;
            }
        }

    }
}