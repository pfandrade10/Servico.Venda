using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servico.Venda.Estruturas
{
    public class EstruturaVenda : EstruturaErro
    {
        public EstruturaVenda()
        {
            Vendas = new List<Models.Vendas>();
        }

        public List<Models.Vendas> Vendas { get; set; }
    }
}
