using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KnapsackTest.Models
{
    public class KnapsackRequest
    {
        [Required]
        public IList<Item> Items { get; set; }

        [Required]
        public decimal PriceLimit { get; set; }

        [Required]
        public int QuantityLimit { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class KnapsackResponse
    {
        public IList<Item> Items { get; set; }

        public decimal TotalPrice { get; set; }

        public int TotalQuantity { get; set; }
    }
}