using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Servico.Venda.Models
{
    public class Vendas
    {
        [Required]
        public int idVenda { get; set; }

        [Required]
        public List<Produto> produtos { get; set; }

        [Required]
        public decimal valorTotal { get; set; }

        [Required]
        public string quantity { get; set; }
    }
}
