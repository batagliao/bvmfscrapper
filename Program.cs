using bvmfscrapper.scrappers;
using System;
using System.Threading.Tasks;

namespace bvmfscrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var companies = await BvmfScrapper.GetCompanies().ConfigureAwait(false);
            }).GetAwaiter().GetResult();
        }


    }
}