using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class ShoppingCart
    {
        List<Cart> shoppingCart;
        public ShoppingCart()
        {
            shoppingCart = new List<Cart>();
        }
        public void addToCart(Cart cart)
        {
            foreach (Cart c in shoppingCart)
            {
                if (c.stationery.Equals(cart.stationery))
                {
                    c.quantity = c.quantity + cart.quantity;

                }
                else if (c.Equals(shoppingCart.Last()))
                {
                    shoppingCart.Add(cart);

                }
            }

        }
        public void deleteCart(Cart cart)
        {
            shoppingCart.Remove(cart);
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