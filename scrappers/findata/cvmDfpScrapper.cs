using System;
using System.Threading.Tasks;
using bvmfscrapper.models;

namespace bvmfscrapper.scrappers.findata
{
    public class CvmDfpScrapper : CvmScrapperBase, IDfpScrapper
    {
        public CvmDfpScrapper(ScrappedCompany company)
        {
            this.Company = company;
        }
        public override short CodTipoDocumento => 4;
    }
}