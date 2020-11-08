using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servico.Venda.Estruturas
{
    public class EstruturaProduto : EstruturaErro
    {
        public EstruturaProduto()
        {
            Produtos = new List<Models.Vendas>();
        }

        public List<Models.Vendas> Produtos { get; set; }
    }
}
