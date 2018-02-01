using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class CartDTO
    {
        public Stationery Stationery { get; set; }
        public int Quantity { get; set; }
        public CartDTO() { }
        public CartDTO(Stationery stationery, int quantity)
        {
            this.Stationery = stationery;
            this.Quantity = quantity;
        }
    }
}