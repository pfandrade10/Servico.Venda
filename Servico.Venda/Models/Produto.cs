using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Servico.Venda.Models
{
    public class Produto
    {
        [Required]
        public int idProduct { get; set; }

        [Required]
        public string productName { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public string cathegory { get; set; }

        [Required]
        public decimal price { get; set; }
    }
}
