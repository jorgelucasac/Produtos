﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Estudos.App.Business.Interfaces;
using Estudos.App.Business.Models;
using Estudos.App.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Estudos.App.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Estudos.App.Web.Controllers
{
    public class ProdutoController : BaseController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ProdutoController(IProdutoRepository produtoRepository, IMapper mapper, IFornecedorRepository fornecedorRepository, IConfiguration configuration)
        {
            _produtoRepository = produtoRepository;
            _mapper = mapper;
            _fornecedorRepository = fornecedorRepository;
            _configuration = configuration;
        }

        #region Actions

        public async Task<IActionResult> Index()
        {
            var consulta = await _produtoRepository.ObeterProdutosFornecedores();
            var lista = _mapper.Map<IEnumerable<ProdutoViewModel>>(consulta);
            return View(lista);
        }

        public async Task<IActionResult> Details(Guid id)
        {

            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel == null)
                return NotFound();

            return View(produtoViewModel);
        }

        public async Task<IActionResult> Create()
        {
            var produtoViewModel = await PopularFornecedores(new ProdutoViewModel());
            ViewBag.FornecedorId = new SelectList(produtoViewModel.Fornecedores, "Id", "Nome");
            return View(produtoViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProdutoViewModel produtoViewModel)
        {
            var caminho = await FileHelper.UploadArquivo(produtoViewModel.ImagemUpload, _configuration);
            if (!ModelState.IsValid || string.IsNullOrEmpty(caminho))
            {
                produtoViewModel = await PopularFornecedores(produtoViewModel);
                ViewBag.FornecedorId = new SelectList(produtoViewModel.Fornecedores, "Id", "Nome");
                return View(produtoViewModel);
            }
            
            var produto = _mapper.Map<Produto>(produtoViewModel);
            produto.Imagem = caminho;
            await _produtoRepository.Adicionar(produto);
            return RedirectToAction(nameof(Index));

        }



        public async Task<IActionResult> Edit(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel == null)
                return NotFound();

            return View(produtoViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id) return NotFound();

            ModelState.Remove(nameof(produtoViewModel.ImagemUpload));

            if (!ModelState.IsValid) return View(produtoViewModel);

            var produto = _mapper.Map<Produto>(produtoViewModel);
            await _produtoRepository.Atualizar(produto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {

            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel == null)
                return NotFound();

            return View(produtoViewModel);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var existe = await _produtoRepository.Existe(id);
            if (!existe) return NotFound();

            await _produtoRepository.Remover(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Privates

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            var produto = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObeterProdutoFornecedor(id));
            produto.Fornecedores =
                _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());
            return produto;
        }

        private async Task<ProdutoViewModel> PopularFornecedores(ProdutoViewModel produto)
        {
            produto.Fornecedores =
                _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());
            return produto;
        }



        #endregion
    }
}