using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public interface IFinDataScrapper
    {
        Task ScrapComposicaoCapital(DocLinkInfo link);
        Task ScrapBalancoIndividualAtivo(DocLinkInfo link);
        Task ScrapBalancoIndividualPassivo(DocLinkInfo link);
        Task ScrapDREIndividual(DocLinkInfo link);
        Task ScrapBalancoConsolidadoAtivo(DocLinkInfo link);
        Task ScrapBalancoConsolidadoPassivo(DocLinkInfo link);
        Task ScrapDREConsolidado(DocLinkInfo link);
    }

    public interface IItrScrapper : IFinDataScrapper
    {

    }

    public interface IDfpScrapper : IFinDataScrapper
    {

    }
}