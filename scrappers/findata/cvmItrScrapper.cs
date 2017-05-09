using System;
using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public class CvmItrScrapper : CvmScrapperBase, IItrScrapper
    {
        public CvmItrScrapper(ScrappedCompany company)
        {
            this.Company = company;
        }
        
        public override short CodTipoDocumento => 3;
    }
}