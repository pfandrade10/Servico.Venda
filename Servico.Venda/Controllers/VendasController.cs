using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Servico.Venda.BaseDados;
using Servico.Venda.DTO;
using Servico.Venda.Estruturas;

namespace Servico.Venda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendasController : ControllerBase
    {
        private readonly AppSettings _appSettings;


        public VendasController(IOptions<AppSettings> appSettings)
        {
            this._appSettings = appSettings.Value;
        }

        /// <summary>
        /// Lista os produtos disponíveis
        /// </summary>
        /// <returns></returns>
        [HttpGet("ListarProdutos")]
        public EstruturaProduto ListarProdutos()
        {
            EstruturaProduto estruturaProduto = new EstruturaProduto();

            try
            {
                estruturaProduto = APIListarProduto();

                if (estruturaProduto.isError.HasValue)
                {
                    throw new Exception(estruturaProduto.descricaoErro);
                }

                return estruturaProduto;
            }
            catch (Exception ex)
            {
                estruturaProduto.isError = true;
                estruturaProduto.descricaoErro = ex.Message;

                return estruturaProduto;
            }
        }

        /// <summary>
        /// Método que busca uma ou mais vendas
        /// </summary>
        /// <param name="idVenda"></param>
        /// <returns></returns>
        [HttpGet]
        public EstruturaVenda Get(int? idVenda)
        {
            EstruturaVenda estruturaVenda = new EstruturaVenda();

            try
            {
                string ListaVendasSession = HttpContext.Session.GetString("Vendas");

                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                string listaVendas = JsonConvert.SerializeObject(estruturaVenda.Vendas);

                HttpContext.Session.SetString("Vendas", listaVendas);

                //Criar método para popular lista de produtos

                if (idVenda.HasValue)
                {
                    //Retornar produto que contenha o id especificado
                    estruturaVenda.Vendas = estruturaVenda.Vendas.Where(x => x.idVenda == idVenda).ToList();

                    if (estruturaVenda.Vendas.Count() == 0)
                    {
                        throw new Exception("A venda selecionada não existe");
                    }

                    return estruturaVenda;
                }


                return estruturaVenda;
            }
            catch (Exception ex)
            {
                estruturaVenda.isError = true;
                estruturaVenda.descricaoErro = ex.Message;

                return estruturaVenda;
            }

        }

        /// <summary>
        /// Método para criar uma nova venda
        /// </summary>
        /// <param name="venda"></param>
        /// <returns></returns>
        [HttpPost]
        public EstruturaVenda Post(EstruturaEntradaCriarVenda entrada)
        {

            EstruturaVenda estruturaVenda = new EstruturaVenda();

            string ListaVendasSession = HttpContext.Session.GetString("Vendas");
            if (!string.IsNullOrEmpty(ListaVendasSession))
            {
                estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);
            }

            List<Models.Vendas> listVendas = new List<Models.Vendas>();

            try
            {
                if (entrada == null)
                {
                    //Erro
                    throw new Exception("A venda a ser criada não pode ser nula");
                }
                //Realizar outras validações

                Models.Vendas venda = new Models.Vendas();
                venda.produtos = new List<Models.Produto>();

                foreach (var item in entrada.EntradaCriarVenda)
                {
                    EstruturaProduto estruturaProduto = APIListarProdutoPorID(item.idProduto);

                    Models.Produto produto = new Models.Produto();

                    produto.idProduct = item.idProduto;
                    produto.productName = estruturaProduto.Produtos[0].productName;
                    produto.description = estruturaProduto.Produtos[0].description;
                    produto.cathegory = estruturaProduto.Produtos[0].cathegory;
                    produto.price = estruturaProduto.Produtos[0].price;
                    produto.quantidade = item.quantidadeProdutos;

                    venda.produtos.Add(produto);

                    venda.valorTotal = venda.valorTotal + estruturaProduto.Produtos[0].price * item.quantidadeProdutos;

                    if (!string.IsNullOrEmpty(ListaVendasSession))
                    {
                        List<Models.Vendas> Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                        venda.idVenda = 0;

                        foreach (var sale in Vendas)
                        {
                            if (sale.idVenda > venda.idVenda)
                                venda.idVenda = sale.idVenda;
                        }

                        venda.idVenda++;
                    }
                    else
                        venda.idVenda = 1;


                }

                estruturaVenda.Vendas.Add(venda);

                string listaVendas = JsonConvert.SerializeObject(estruturaVenda.Vendas);

                HttpContext.Session.SetString("Vendas", listaVendas);

                return estruturaVenda;
            }
            catch (Exception ex)
            {
                estruturaVenda.isError = true;
                estruturaVenda.descricaoErro = ex.Message;

                return estruturaVenda;
            }

        }

        /// <summary>
        /// Métodos para alterar Vendas
        /// </summary>
        /// <param name="venda"></param>
        /// <returns></returns>
        [HttpPost("InserirProduto")]
        public EstruturaVenda InserirProduto([FromBody] EstruturaEntradaCriarVenda entrada, int idVenda)
        {
            EstruturaVenda estruturaVenda = new EstruturaVenda();

            try
            {
                BaseVendas baseProdutos = new BaseVendas();

                if (entrada == null)
                    throw new Exception("o produto a ser inserido não pode ser nulo");


                if (idVenda == 0)
                    throw new Exception("Favor selecionar uma venda!");

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                Models.Vendas VendaAlterada = estruturaVenda.Vendas.Where(x => x.idVenda == idVenda).SingleOrDefault();

                if (VendaAlterada == null)
                    throw new Exception("A venda selecionada não existe");

                foreach (var item in entrada.EntradaCriarVenda)
                {
                    EstruturaProduto estruturaProduto = APIListarProdutoPorID(item.idProduto);

                    Models.Produto produto = new Models.Produto();

                    produto.idProduct = item.idProduto;
                    produto.productName = estruturaProduto.Produtos[0].productName;
                    produto.description = estruturaProduto.Produtos[0].description;
                    produto.cathegory = estruturaProduto.Produtos[0].cathegory;
                    produto.price = estruturaProduto.Produtos[0].price;
                    produto.quantidade = item.quantidadeProdutos;

                    VendaAlterada.produtos.Add(produto);

                    VendaAlterada.valorTotal = VendaAlterada.valorTotal + estruturaProduto.Produtos[0].price * item.quantidadeProdutos;
                }

                string listaVendas = JsonConvert.SerializeObject(estruturaVenda.Vendas);

                HttpContext.Session.SetString("Vendas", listaVendas);

                return estruturaVenda;
            }
            catch (Exception ex)
            {
                estruturaVenda.isError = true;
                estruturaVenda.descricaoErro = ex.Message;

                return estruturaVenda;
            }

        }

        /// <summary>
        /// Método para deletar produtos
        /// </summary>
        /// <param name="idVenda"></param>
        /// <returns></returns>
        [HttpDelete]
        public EstruturaVenda Delete(int idVenda)
        {

            EstruturaVenda estruturaVenda = new EstruturaVenda();
            try
            {

                if (idVenda == 0)
                {
                    //Retornar produto que contenha o id especificado

                    throw new Exception("Venda selecionado não existe");
                }

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                Models.Vendas vendaRemovido = estruturaVenda.Vendas.Where(x => x.idVenda == idVenda).SingleOrDefault();

                if (vendaRemovido == null)
                    throw new Exception("produto selecionado não existe");

                estruturaVenda.Vendas.Remove(vendaRemovido);

                string listaVendas = JsonConvert.SerializeObject(estruturaVenda.Vendas);

                HttpContext.Session.SetString("Vendas", listaVendas);

                return estruturaVenda;
            }
            catch (Exception ex)
            {
                estruturaVenda.isError = true;
                estruturaVenda.descricaoErro = ex.Message;

                return estruturaVenda;
            }

        }

        /// <summary>
        /// Método para deletar produtos
        /// </summary>
        /// <param name="idVenda"></param>
        /// <returns></returns>
        [HttpDelete("RemoverProduto")]
        public EstruturaVenda DeleteProduto(int idVenda, int idProduto)
        {

            EstruturaVenda estruturaVenda = new EstruturaVenda();
            try
            {

                if (idVenda == 0)
                {
                    //Retornar produto que contenha o id especificado

                    throw new Exception("Venda selecionado não existe");
                }

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                Models.Vendas venda = estruturaVenda.Vendas.Where(x => x.idVenda == idVenda).SingleOrDefault();

                Models.Produto produtoRemovido = venda.produtos.Where(x => x.idProduct == idProduto).SingleOrDefault();

                if (venda == null)
                    throw new Exception("venda selecionada não existe");

                if (produtoRemovido == null)
                    throw new Exception("produto selecionado não existe");

                venda.valorTotal = venda.valorTotal - produtoRemovido.price * produtoRemovido.quantidade;

                venda.produtos.Remove(produtoRemovido);

                string listaVendas = JsonConvert.SerializeObject(estruturaVenda.Vendas);

                HttpContext.Session.SetString("Vendas", listaVendas);

                return estruturaVenda;
            }
            catch (Exception ex)
            {
                estruturaVenda.isError = true;
                estruturaVenda.descricaoErro = ex.Message;

                return estruturaVenda;
            }

        }

        private EstruturaProduto APIListarProduto()
        {
            using (var client = new HttpClient())
            {

                string endpoint = $"https://localhost:44383/api/Produto";

                var response = client.GetAsync(endpoint).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                EstruturaProduto estruturaProduto = Newtonsoft.Json.JsonConvert.DeserializeObject<EstruturaProduto>(responseString);

                if (!response.IsSuccessStatusCode)
                {
                    estruturaProduto.descricaoErro = "Erro ao listar Produtos";
                    estruturaProduto.isError = true;
                }

                return estruturaProduto;
            }
        }

        private EstruturaProduto APIListarProdutoPorID(int id)
        {
            using (var client = new HttpClient())
            {

                string endpoint = $"https://localhost:44383/api/Produto?idProduto=" + id;

                var response = client.GetAsync(endpoint).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                EstruturaProduto estruturaProduto = Newtonsoft.Json.JsonConvert.DeserializeObject<EstruturaProduto>(responseString);

                if (!response.IsSuccessStatusCode)
                {
                    estruturaProduto.descricaoErro = "Erro ao listar Produtos";
                    estruturaProduto.isError = true;
                }

                return estruturaProduto;
            }
        }

    }
}