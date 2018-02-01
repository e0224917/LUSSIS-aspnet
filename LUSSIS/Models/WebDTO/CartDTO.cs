using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class CartDTO
    {
        public Stationery stationery { get; set; }
        public int quantity { get; set; }
        public CartDTO() { }
        public CartDTO(Stationery stationery, int quantity)
        {
            this.stationery = stationery;
            this.quantity = quantity;
        }
    }
}