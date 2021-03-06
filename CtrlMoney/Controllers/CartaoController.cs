﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using APL;
using AutoMapper;
using CtrlMoney.Annotations;
using CtrlMoney.ViewModel;
using Dominio;
using EntityAcessoDados;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CtrlMoney.Controllers
{
    public class CartaoController : Controller
    {
        CartaoAPL apl = new CartaoAPL();
        PessoaUsuarioAPL apl_pessoa = new PessoaUsuarioAPL();
        DespesaAPL apl_despesa = new DespesaAPL();

        // GET: Cartao
        [Authorize]
        [SelecionadorMesFilter]
        public ActionResult Index(int ano,int mes)
        {
            string id_usuario = User.Identity.GetUserId();

            DateTime dataAtual = DateTime.Now;
            DateTime dataMes = new DateTime(ano, mes, 1);
            int anoAtual = dataAtual.Year;
            int mesAtual = dataAtual.Month;

            if (ano == anoAtual && mes == mesAtual)
            {
                ViewBag.MesAnoIgual = true;
            } else {
                ViewBag.MesAnoIgual = false;
            }

            ViewBag.MesSelecionado = --mes;
            ViewBag.AnoSelecionado = ano;



            List<Cartao> cartoes = apl_pessoa.SelecionarById(id_usuario).Pessoa.Cartoes.ToList(); 
            List<CartaoViewModel> cartoesVM = new List<CartaoViewModel>();

            List<decimal> despesasCartao = new List<decimal>(); 
            List<decimal> limites = new List<decimal>();
            foreach (Cartao item in cartoes)
            {
                List<Parcelamento> parcelamentos = apl_despesa.ListarHistoricoPorCartao(item.Id, ano, mes + 1).Cast<Parcelamento>().ToList();
                
                despesasCartao.Add(parcelamentos.Sum(p => p.Valor/p.NumParcelas * (p.NumParcelas 
                - (int) Math.Floor(Math.Abs(dataMes.Subtract(p.DataCompra).Days) / (365.25 / 12)))));
                limites.Add(item.Limite);
                cartoesVM.Add(Mapper.Map<Cartao, CartaoViewModel>(item));
            }
            ViewBag.DespesasCartao = despesasCartao;
            ViewBag.Limites = limites;

            return View(cartoesVM);
        }

        // GET: Cartao/Details/5
        public ActionResult Details(int? id)
        {
            /*if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cartao cartao = db.Cartoes.Find(id);
            if (cartao == null)
            {
                return HttpNotFound();
            }*/
            return View(/*cartao*/);
        }

        // GET: Cartao/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Cartao/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nome,Limite,DiaFechamento,DiaVencimento,Numero")] CartaoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Cartao cartao = Mapper.Map<CartaoViewModel, Cartao>(viewModel);
                Cartao cartaoExistente = apl.SelecionarPorNumero(cartao.Numero);
                if (cartaoExistente != null)
                {
                    cartao = cartaoExistente;
                } else {
                    apl.Inserir(cartao);
                }
                
                string id_usuario = User.Identity.GetUserId();
                Usuario usuario = apl_pessoa.SelecionarById(id_usuario);
                Pessoa pessoa = usuario.Pessoa;
                apl.InserirPessoa(cartao, pessoa);
                return RedirectToAction("Index");
            } else {
                ModelState.AddModelError("erro_identity", "Não foi possivel salvar");
                return View(viewModel);
            }
            
        }

        // GET: Cartao/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Cartao cartao = apl.SelecionarPorId((int)id);
            CartaoViewModel viewModel = Mapper.Map<Cartao, CartaoViewModel>(cartao);
            if (cartao == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }

        // POST: Cartao/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nome,Limite,DiaFechamento,DiaVencimento,Numero")] CartaoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Cartao cartao = Mapper.Map<CartaoViewModel, Cartao>(viewModel);
                apl.Alterar(cartao);
                return RedirectToAction("Index");
            }
            return View(viewModel);
        }

        // GET: Cartao/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cartao cartao = apl.SelecionarPorId((int)id);
            CartaoViewModel viewModel = Mapper.Map<Cartao, CartaoViewModel>(cartao);

            if (cartao == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }

        // POST: Cartao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            string id_usuario = User.Identity.GetUserId();

            Cartao cartao; // = apl.SelecionarPorId(id);
            Usuario usuario = apl_pessoa.SelecionarById(id_usuario);
            Pessoa pessoa = usuario.Pessoa;
            cartao = pessoa.Cartoes.SingleOrDefault(c => c.Id == id);
            pessoa.Cartoes.Remove(cartao);
            apl_pessoa.Alterar(pessoa, usuario);
            //apl.ExcluirPessoa(cartao,pessoa);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            /*if (disposing)
            {
                db.Dispose();
            }*/
            base.Dispose(disposing);
        }
    }
}
