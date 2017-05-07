using System;
using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public class CvmItrScrapper : IItrScrapper
    {
        private ScrappedCompany company;

        public CvmItrScrapper(ScrappedCompany company)
        {
            this.company = company;
        }

        public Task ScrapBalancoConsolidadoAtivo(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapBalancoConsolidadoPassivo(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapBalancoIndividualAtivo(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapBalancoIndividualPassivo(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapComposicaoCapital(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapDREConsolidado(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapDREIndividual(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }
    }
}