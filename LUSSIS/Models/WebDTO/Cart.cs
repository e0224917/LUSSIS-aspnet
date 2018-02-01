using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class Cart
    {
        public Stationery Stationery { get; set; }
        public int Quantity { get; set; }
        public Cart() { }
        public Cart(Stationery stationery, int quantity)
        {
            this.Stationery = stationery;
            this.Quantity = quantity;
        }
    }
}