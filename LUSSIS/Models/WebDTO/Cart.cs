using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class Cart
    {
        public Stationery stationery { get; set; }
        public int quantity { get; set; }
        public Cart() { }
        public Cart(Stationery stationery, int quantity)
        {
            this.stationery = stationery;
            this.quantity = quantity;
        }
    }
}