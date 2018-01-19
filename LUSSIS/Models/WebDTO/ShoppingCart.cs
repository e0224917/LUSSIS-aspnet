using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class ShoppingCart
    {
        public List<Cart> shoppingCart;
        public ShoppingCart()
        {
            shoppingCart = new List<Cart>();
        }
        public void addToCart(Cart cart)
        {
            bool status = false;
            foreach (Cart c in shoppingCart)
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
        public void deleteCart(string id)
        {           
            for(int i = 0; i <= shoppingCart.Count; i++)
            {
                if (shoppingCart[i].stationery.ItemNum == id)
                {
                    shoppingCart.RemoveAt(i);
                }
            }
            //foreach (Cart c in shoppingCart)
            //{

            //    if (c.stationery.ItemNum.Equals(id))
            //    {
            //        shoppingCart.Remove(c);
            //    }
            //}
        }
        public List<Cart> GetAllCartItem()
        {
            return shoppingCart.ToList();
        }
        public int GetCartItemCount()
        {
            return shoppingCart.Count();
        }
    }
}