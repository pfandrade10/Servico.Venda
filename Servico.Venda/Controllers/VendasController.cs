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
            catch(Exception ex)
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
        public EstruturaVenda Post(Models.Vendas venda)
        {
            EstruturaVenda estruturaVenda = new EstruturaVenda();

            List<Models.Vendas> listVendas = new List<Models.Vendas>();

            try
            {
                venda.valorTotal = 0;

                foreach(var produto in venda.produtos)
                {
                    EstruturaProduto estruturaProduto = APIListarProdutoPorID(produto.idProduct);

                    produto.productName = estruturaProduto.Produtos[0].productName;
                    produto.description = estruturaProduto.Produtos[0].description;
                    produto.cathegory = estruturaProduto.Produtos[0].cathegory;
                    produto.price = estruturaProduto.Produtos[0].price;

                    venda.valorTotal += estruturaProduto.Produtos[0].price * produto.quantidade;
                }

                BaseVendas baseProdutos = new BaseVendas();

                if (venda == null)
                {
                    //Erro
                    throw new Exception("A venda a ser criada não pode ser nula");
                }
                //Realizar outras validações

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (!string.IsNullOrEmpty(ListaVendasSession))
                {
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);
                }
                   

                if (estruturaVenda.Vendas.Where(x => x.idVenda == venda.idVenda).SingleOrDefault() != null)
                    throw new Exception("Já existe uma venda com esse Identificador registrado");

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
        [HttpPut]
        public EstruturaVenda AlterarVenda([FromBody] Models.Vendas venda)
        {
            EstruturaVenda estruturaVenda = new EstruturaVenda();

            try
            {
                BaseVendas baseProdutos = new BaseVendas();

                if (venda == null)
                    throw new Exception("o produto a ser alterado não pode ser nulo");


                if (venda.idVenda == 0)
                    throw new Exception("Favor selecionar um produto!");               

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);            

                Models.Vendas VendaAlterada = estruturaVenda.Vendas.Where(x => x.idVenda == venda.idVenda).SingleOrDefault();

                if (VendaAlterada == null)
                    throw new Exception("produto selecionado não existe");

                estruturaVenda.Vendas.Remove(VendaAlterada);
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
                BaseVendas baseProdutos = new BaseVendas();

                if (idVenda == 0)
                {
                    //Retornar produto que contenha o id especificado

                    throw new Exception("produto selecionado não existe");
                }

                string ListaVendasSession = HttpContext.Session.GetString("Vendas");
                if (string.IsNullOrEmpty(ListaVendasSession))
                {
                    throw new Exception("Não há vendas Registradas");
                }
                else
                    estruturaVenda.Vendas = JsonConvert.DeserializeObject<List<Models.Vendas>>(ListaVendasSession);

                Models.Vendas produtoRemovido = estruturaVenda.Vendas.Where(x => x.idVenda == idVenda).SingleOrDefault();

                if (produtoRemovido == null)
                    throw new Exception("produto selecionado não existe");

                estruturaVenda.Vendas.Remove(produtoRemovido);

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

                string endpoint = $"https://localhost:44383/api/Produto?idProduto="+id;

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