﻿using eTickets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace eTickets.Data.Cart
{
    public class ShoppingCart
    {
        public AppDbContext _context { get; set; }
        public string ShoppingCartId { get; set; }
        public List<ShoppingCartItem> ShoppingCartItems { get; set; }

        public ShoppingCart(AppDbContext context)
        {
            _context = context;
        }

        public static ShoppingCart GetShoppingCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            var context = services.GetService<AppDbContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public void AddItemToCart(Movie movie)
        {
            //does the movie exist already in shopping cart
            var shoppingCartItem = _context.ShoppingCartItems
                .FirstOrDefault(n => n.Movie.Id == movie.Id && n.ShoppingCartId == ShoppingCartId);

            if(shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem()
                {
                    ShoppingCartId = ShoppingCartId,
                    Movie = movie,
                    Amount = 1
                };
                _context.ShoppingCartItems.Add(shoppingCartItem);
            } else
            {
                shoppingCartItem.Amount++;
            }
            _context.SaveChanges();
        }

        public void RemoveItemFromCart(Movie movie)
        {
            var item = _context.ShoppingCartItems
                .FirstOrDefault(n => n.Movie.Id == movie.Id && n.ShoppingCartId == ShoppingCartId);

            if (item != null)
            {
                if(item.Amount > 1)
                {
                    item.Amount--;
                } else
                {
                    _context.ShoppingCartItems.Remove(item);
                }
            }
            _context.SaveChanges();
        }

        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            /*SELECT *
            FROM ShoppingCartItem JOIN Movie ON ShoppingCartItem.ShoppingCartId = Movie.ShoppingCartId;*/
            return ShoppingCartItems ?? (ShoppingCartItems = _context.ShoppingCartItems
                .Where(n => n.ShoppingCartId == ShoppingCartId)
                .Include(n => n.Movie)
                .ToList());
        }

        public double GetShoppingCartTotal()
        {
            var total = _context.ShoppingCartItems
                .Where(n => n.ShoppingCartId == ShoppingCartId)
                .Select(n => n.Movie.Price * n.Amount)
                .Sum();
            return total;
        }

        public async Task ClearShoppingCartAsync()
        {
            var items = _context.ShoppingCartItems.Where(n => n.ShoppingCartId == ShoppingCartId).ToList();
            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            ShoppingCartItems = new List<ShoppingCartItem>();
        }
    }
}
