using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudSaba.Data;
using CloudSaba.Models;
using CloudSaba.Migrations;

namespace CloudSaba.Controllers
{
    public class CartController : Controller
    {
        private readonly CloudSabaContext _context;

        public CartController(CloudSabaContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId)
        {
            var products = _context.IceCream.ToList();
            var itemInfo = products.FirstOrDefault(p => p.Id == productId);
            if (itemInfo == null)
            {
                //todo: problem! throw...
            }
            // Get or create a unique cart identifier for the user
            string cartId = "123";//GetOrCreateCartId();
            var cartItems = _context.CartItem.ToList();
            var existingItem = cartItems.FirstOrDefault(
                     cartItem => cartItem.CartId == cartId && cartItem.ItemId == productId
                     );
            if (existingItem != null)
            {
                // Update quantity if the item is already in the cart
                existingItem.Quantity += 1;
                existingItem.Price += itemInfo.Price;
            }
            else
            {
                // Add a new item to the cart
                _context.Add(new CartItem
                {
                    ItemId = productId,
                    CartId = cartId,
                    Quantity = 1,
                    Price = itemInfo.Price,
                    Weight = 1,
                    DateCreated = DateTime.Now,
                    OrderId = "1234", //todo: update with the order
                });
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product added to cart" });
        }
        //[ValidateAntiForgeryToken] //ETI by GPT:Ensure that sensitive operations like modifying the cart are protected from cross-site request forgery (CSRF) attacks. You can do this by adding the [ValidateAntiForgeryToken] attribute to your actions.
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            var products = _context.IceCream.ToList();
            var itemInfo = products.FirstOrDefault(p => p.Id == productId);
            if (itemInfo == null)
            {
                //todo: problem! throw...
            }
            string cartId = "123";//GetOrCreateCartId();
            var cartItems = _context.CartItem.ToList();
            var existingItem = cartItems.FirstOrDefault(
                     cartItem => cartItem.CartId == cartId && cartItem.ItemId == productId
                     );
            if (existingItem != null)
            {
                // Update quantity if the item is already in the cart
                existingItem.Quantity -= 1;
                existingItem.Price -= itemInfo.Price;
            }
            else
            {
                //todo: problem!
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product added to cart" });
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
              return _context.CartItem != null ? 
                          View(await _context.CartItem.ToListAsync()) :
                          Problem("Entity set 'CloudSabaContext.CartItem'  is null.");
        }

        // GET: Cart/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.CartItem == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // GET: Cart/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cart/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,CartId,Weight,Quantity,Price,DateCreated,FlavourId,OrderId")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cartItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cartItem);
        }

        // GET: Cart/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.CartItem == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItem.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }
            return View(cartItem);
        }

        // POST: Cart/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ItemId,CartId,Weight,Quantity,Price,DateCreated,FlavourId,OrderId")] CartItem cartItem)
        {
            if (id != cartItem.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartItemExists(cartItem.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cartItem);
        }

        // GET: Cart/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.CartItem == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // POST: Cart/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.CartItem == null)
            {
                return Problem("Entity set 'CloudSabaContext.CartItem'  is null.");
            }
            var cartItem = await _context.CartItem.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItem.Remove(cartItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartItemExists(string id)
        {
          return (_context.CartItem?.Any(e => e.ItemId == id)).GetValueOrDefault();
        }
    }
}
