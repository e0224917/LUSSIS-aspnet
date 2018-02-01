using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Cui Runze
    public class ShoppingCartDTO
    {
        public List<CartDTO> shoppingCart;
        public ShoppingCartDTO()
        {
            shoppingCart = new List<CartDTO>();
        }
        public void AddToCart(CartDTO cart)
        {
            bool status = false;
            foreach (CartDTO c in shoppingCart)
            {
                
                if (c.stationery.ItemNum.Equals(cart.stationery.ItemNum))
                {
                    c.quantity = c.quantity + cart.quantity;
                    status = true;
                }               
            }
            if (status == false)
            {
                shoppingCart.Add(cart);
            }
        }
        public void DeleteCart(string id)
        {           
            for(int i = 0; i < shoppingCart.Count; i++)
            {
                if (shoppingCart[i].stationery.ItemNum == id)
                {
                    shoppingCart.RemoveAt(i);
                }
            }            
        }
        public List<CartDTO> GetAllCartItem()
        {
            return shoppingCart.ToList();
        }
        public int GetCartItemCount()
        {
            return shoppingCart.Count();
        }
    }
}