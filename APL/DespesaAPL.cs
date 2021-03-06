﻿using Dominio;
using EntityAcessoDados;
using EntityAcessoDados.Repositorio;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APL
{
    public class DespesaAPL : IAPLGenerica
    {
        private CtrlMoneyDbContext db;
        private IRepositorioHistorico<Despesa, int> repositorioDespesa;


            public DespesaAPL()
            {
                db = new CtrlMoneyDbContext();
                repositorioDespesa = new RepositorioDespesaEntity(db);
            }

            public void Dispose()
            {
                db.Dispose();
            }

            public void Inserir(Despesa despesa)
            {
              
                 repositorioDespesa.Inserir(despesa);
            }

            public Despesa SelecionarById(int id)
            {
               return repositorioDespesa.SelecionarPorId(id);
            }

            public void Alterar(Despesa despesa)
            {

                repositorioDespesa.Alterar(despesa);
            }

            public void Deletar(Despesa despesa)
            {

                repositorioDespesa.Excluir(despesa);
            }

            public List<Despesa> Listar (string pessoaId, int ano, int mes)
            {
               return repositorioDespesa.ListarHistorico (pessoaId,  ano, mes);
            }
            
            public List<Despesa> ListarHistoricoPorCartao(int cartaoId, int ano, int mes)
            {
                return ((RepositorioDespesaEntity)repositorioDespesa).ListarHistoricoPorCartao(cartaoId, ano, mes);
            }
        }
    }



