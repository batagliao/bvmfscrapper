using System;
using System.Collections.Generic;
using System.Linq;
using bvmfscrapper.models;
using Microsoft.EntityFrameworkCore;

namespace bvmfscrapper.data.repositories
{
    public class EmpresaRepository
    {
        public EmpresaRepository()
        {
        }


        public static void InsertOrUpdate(IList<ScrappedCompany> companies)
        {
            using (var db = new DataContext())
            {
                var ids = companies.Select(c => c.CodigoCVM);
                var companiesInDb = db.Empresas.Where(
                    e => ids.Contains(e.Codigo))
                                      .ToList(); // single db query


                foreach (var company in companies)
                {
                    var empresaIdDb = db.Empresas
                    .SingleOrDefault(e => e.Codigo == company.CodigoCVM); // runs in memory

                    if (empresaIdDb != null) //update
                    {
                        FillEmpresaWithCompany(empresaIdDb, company);
                        db.Entry(empresaIdDb).State = EntityState.Modified;
                        // TODO: insert or update tickers
                    }
                    else
                    {
                        var e = new Empresa();
                        FillEmpresaWithCompany(e, company);
                        db.Empresas.Add(e);

                        //TODO: add tickers
                    }
                }
                db.SaveChanges();
            }
        }

        private static void FillEmpresaWithCompany(Empresa e, ScrappedCompany c){
            e.Codigo = c.CodigoCVM;
            e.RazaoSocial = c.RazaoSocial;
            e.NomePregao = c.NomePregao;
            e.Segmento = c.Segmento;
            e.CNPJ = c.CNPJ;
            e.AtividadePrincipal = string.Join(",", c.AtividadePrincipal);
            e.ClassificacaoSetorial = string.Join(",", c.ClassificacaoSetorial);
            e.Site = c.Site;
            e.UltimaAtualizacao = c.UltimaAtualizacao;
        }
    }
}
