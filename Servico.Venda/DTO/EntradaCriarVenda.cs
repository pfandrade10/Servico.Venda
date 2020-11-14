using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servico.Venda.DTO
{
    public class EstruturaEntradaCriarVenda
    {
        public EstruturaEntradaCriarVenda()
        {
            EntradaCriarVenda = new List<EntradaCriarVenda>();
        }

        public List<EntradaCriarVenda> EntradaCriarVenda { get; set; }
        
    }

    public class EntradaCriarVenda
    {
        public int idProduto { get; set; }

        public int quantidadeProdutos { get; set; }
    }
}
