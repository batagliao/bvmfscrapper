using System;
using System.Collections.Generic;
using System.Linq;
using bvmfscrapper.models;
using Microsoft.EntityFrameworkCore;
using log4net;

namespace bvmfscrapper.data.repositories
{
    // TODO: LOG 4 NET the WHOLE file


    public class EmpresaRepository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyExtensions));


        public EmpresaRepository()
        {
        }


        public static void InsertOrUpdate(IList<ScrappedCompany> companies)
        {
            log.Info("Iniciando atualização das empresas no banco");

            using (var db = new DataContext())
            {
                var ids = companies.Select(c => c.CodigoCVM);
                var companiesInDb = db.Empresas.Include(nameof(Empresa.Tickers)).Where(
                    e => ids.Contains(e.Codigo))
                                      .ToList(); // single db query


                foreach (var company in companies)
                {
                    var empresaInDb = db.Empresas
                    .SingleOrDefault(e => e.Codigo == company.CodigoCVM); // runs in memory

                    log.Info($"Gravando empresa {company.RazaoSocial} no banco");

                    if (empresaInDb != null) //update
                    {
                        log.Info($"A empresa {company.RazaoSocial} já existe. Atualizando");
                        FillEmpresaWithCompany(empresaInDb, company);
                        db.Entry(empresaInDb).State = EntityState.Modified;
                        UpdateExistentsTickers(empresaInDb, company);
                    }
                    else
                    {
                        log.Info($"A empresa {company.RazaoSocial} não existe. Inserindo");
                        var e = new Empresa();
                        FillEmpresaWithCompany(e, company);
                        var tickers = GetTickers(company);
                        if (tickers.Count > 0)
                        {
                            e.Tickers = tickers;
                        }
                        db.Empresas.Add(e);
                    }
                }
                db.SaveChanges();
            }
        }

        private static void UpdateExistentsTickers(Empresa empresaInDb, ScrappedCompany company)
        {
            log.Info($"Tickers = {string.Join(",", company.CodigosNegociacao)}");

            // tickers que existem n banco mas não em company > delete
            empresaInDb.Tickers.RemoveAll(t => !company.CodigosNegociacao.Contains(t.Nome));


            foreach (var cod in company.CodigosNegociacao)
            {
                // tickers que existem em company mas não no banco > insert
                if (empresaInDb.Tickers.Any(t => t.Nome == cod) && !string.IsNullOrWhiteSpace(cod))
                {
                    empresaInDb.Tickers.Add(
                        new Ticker()
                        {
                            CodigoCVM = empresaInDb.Codigo,
                            Nome = cod
                        });
                }
            }
        }

        private static void FillEmpresaWithCompany(Empresa e, ScrappedCompany c)
        {
            e.Codigo = c.CodigoCVM;
            e.RazaoSocial = c.RazaoSocial;
            e.NomePregao = c.NomePregao;
            e.Segmento = c.Segmento;
            e.CNPJ = c.CNPJ;
            if (c.AtividadePrincipal != null)
            {
                e.AtividadePrincipal = string.Join(",", c.AtividadePrincipal);
            }
            if (c.ClassificacaoSetorial != null)
            {
                e.ClassificacaoSetorial = string.Join(",", c.ClassificacaoSetorial);
            }
            e.Site = c.Site;
            e.UltimaAtualizacao = c.UltimaAtualizacao;
        }

        private static List<Ticker> GetTickers(ScrappedCompany c)
        {
            log.Info($"Tickers = {string.Join(",", c.CodigosNegociacao)}");

            List<Ticker> tickers = new List<Ticker>();
            foreach (var ticker in c.CodigosNegociacao)
            {
                if (!string.IsNullOrWhiteSpace(ticker))
                {
                    tickers.Add(new Ticker
                    {
                        Nome = ticker,
                        CodigoCVM = c.CodigoCVM
                    });
                }
            }
            return tickers;
        }
    }
}
