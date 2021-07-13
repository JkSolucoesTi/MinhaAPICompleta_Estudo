using AutoMapper;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
    [Route("api/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser user) : base(notificador,user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }


        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar ([FromBody]ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            //receber a imagem em base 64
            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
            if (!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome)) return CustomResponse(produtoViewModel);

            produtoViewModel.Imagem = imagemNome;
            //

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }



        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produto = await ObterPorId(id);

            if (produto == null) return NotFound();

            await _produtoRepository.Remover(id);

            return CustomResponse(produto);
        }


        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
            
        }

        public async Task<IActionResult> Atualizar (Guid id , ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id) return NotFound();

            var produtoAtualizacao = await ObterPorId(id);
            produtoViewModel.Imagem = produtoAtualizacao.Value.Imagem;

            if (ModelState.IsValid) return CustomResponse(ModelState);

            if(produtoViewModel.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
                if(!UploadArquivo(produtoViewModel.ImagemUpload,imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Value.Imagem = imagemNome;
            }

            produtoAtualizacao.Value.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Value.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Value.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Value.Ativo = produtoViewModel.Ativo;

            await _produtoRepository.Atualizar(_mapper.Map<Produto>(produtoAtualizacao.Value));

            return CustomResponse(produtoViewModel);
        }

 

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imageDataByteArray = Convert.FromBase64String(arquivo);

            if(string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto");
                return false;
            }

            var filepath = Path.Combine(@"C:\Users\Gonçalves\Desktop\Estudo\Labs\AppAngularDemo\app\demo-webapi\src\assets", imgNome);


            if(System.IO.File.Exists(filepath))
            {
                NotificarErro("Já existe um arquivo com este nome");
                return false;
            }

            System.IO.File.WriteAllBytes(filepath, imageDataByteArray);

            return true;

        }


    }
}
