using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public interface IFinDataScrapper
    {
        Task ScrapComposicaoCapital(DocLinkInfo link);
        Task ScrapBalancoAtivo(DocLinkInfo link, FinInfoTipo tipo);
        Task ScrapBalancoPassivo(DocLinkInfo link, FinInfoTipo tipo);
        Task ScrapDRE(DocLinkInfo link, FinInfoTipo tipo);
    }

    public interface IItrScrapper : IFinDataScrapper
    {

    }

    public interface IDfpScrapper : IFinDataScrapper
    {

    }
}