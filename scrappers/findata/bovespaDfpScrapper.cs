using System;
using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public class BovespaDfpScrapper : IDfpScrapper
    {
        private ScrappedCompany company;

        public BovespaDfpScrapper(ScrappedCompany company)
        {
            this.company = company;
        }

        public Task ScrapBalancoAtivo(DocLinkInfo link, FinInfoTipo tipo)
        {
            throw new NotImplementedException();
        }


        public Task ScrapBalancoPassivo(DocLinkInfo link, FinInfoTipo tipo)
        {
            throw new NotImplementedException();
        }

        public Task ScrapComposicaoCapital(DocLinkInfo link)
        {
            throw new NotImplementedException();
        }

        public Task ScrapDRE(DocLinkInfo link, FinInfoTipo tipo)
        {
            throw new NotImplementedException();
        }

    }
}