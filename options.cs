using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bvmfscrapper
{
    public class Options
    {
        const string ARG_DONT_EXTRACT_COMPANY = "--dont-extract-companies";
        const string ARG_DONT_UPDATE_DB = "--dont-update-db";
        const string ARG_DONT_EXTRACT_DOC_LINKS = "--dont-extract-doc-links";

        public static Options ParseOptions(string[] args)
        {
            /*
             * args:
             * --dont-extract-companies
             * --dont-update-db
             * --dont-extract-doc-links
             */
            var option = new Options();
            option.ShouldLoadCompanyList = !args.Contains(ARG_DONT_EXTRACT_COMPANY);
            option.ShouldUpdateCompaniesIdDb = !args.Contains(ARG_DONT_UPDATE_DB);
            option.ShouldExtractDocLinks = !args.Contains(ARG_DONT_EXTRACT_DOC_LINKS);
            return option;
        }


        public bool ShouldLoadCompanyList { get; set; } = true;
        public bool ShouldUpdateCompaniesIdDb { get; set; } = true;
        public bool ShouldExtractDocLinks { get; set; } = true;
    }
}
