using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class ShoppingCart
    {
        private List<Cart> Carts;
        public ShoppingCart()
        {
            Carts = new List<Cart>();
        }
        public void AddToCart(Cart cart)
        {
            bool status = false;
            foreach (Cart c in Carts)
            {
                
                if (c.Stationery.ItemNum.Equals(cart.Stationery.ItemNum))
                {
                    c.Quantity = c.Quantity + cart.Quantity;
                    status = true;
                }               
            }
            if (status == false)
            {
                Carts.Add(cart);
            }
        }
        public void DeleteCart(string id)
        {           
            for(int i = 0; i < Carts.Count; i++)
            {
                if (Carts[i].Stationery.ItemNum == id)
                {
                    Carts.RemoveAt(i);
                }
            }
        }

        public List<Cart> GetAllCartItem()
        {
            return Carts.ToList();
        }

        public int GetCartItemCount()
        {
            return Carts.Count();
        }
    }
}