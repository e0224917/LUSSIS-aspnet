using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class ShoppingCartDTO
    {
        public List<CartDTO> Carts;
        public ShoppingCartDTO()
        {
            Carts = new List<CartDTO>();
        }
        public void AddToCart(CartDTO cart)
        {
            bool status = false;
            foreach (CartDTO c in Carts)
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

        public List<CartDTO> GetAllCartItem()
        {
            return Carts.ToList();
        }

        public int GetCartItemCount()
        {
            return Carts.Count();
        }
    }
}