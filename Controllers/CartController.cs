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
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Azure.Amqp.Framing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DocumentFormat.OpenXml.Vml;
using CloudSaba.Services;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;


namespace CloudSaba.Controllers
{
    public class CartController : Controller
    {
        private readonly CloudSabaContext _context;
        private readonly ApiServices _apiServices; // Add this line

        public CartController(CloudSabaContext context, ApiServices apiServices) // Add ApiServices parameter
        {
            _context = context;
            _apiServices = apiServices; // Initialize ApiServices

            // Configure session services
            ConfigureSessionServices(services: new ServiceCollection());
        }
        private string GetOrCreateCartId()
        {
            string cartId;

            if (HttpContext.Session.TryGetValue("CartId", out var cartIdBytes))
            {
                // If the cart ID is already in the session, use it
                cartId = System.Text.Encoding.UTF8.GetString(cartIdBytes);
            }
            else
            {
                // If no cart ID in the session, generate a new one and store it
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
            }

            return cartId;
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return Json(new { success = false, message = "Product ID is required." });
                }
                var products = _context.IceCream.ToList();
                var itemInfo = products.FirstOrDefault(p => p.Id == productId);
                if (itemInfo == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }
                // Get or create a unique cart identifier for the user
                HttpContext.Session.LoadAsync().Wait();
                string cartId = GetOrCreateCartId();
                var cartItems = _context.CartItem.ToList();
                var existingItem = cartItems.FirstOrDefault(
                         cartItem => cartItem.CartId == cartId && cartItem.ItemId == productId
                         );
                var itemCount = cartItems.Count(
                         cartItem => cartItem.CartId == cartId);
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
                        OrderId = Guid.NewGuid().ToString()
                    });
                }
                if (itemCount >= 5)
                {
                    return Json(new { succes = false, message = "you can only add upto 5 products to the cart" });
                }
                else
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Product added to cart" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.ToString() });
            }

        }
        //[ValidateAntiForgeryToken] //ETI by GPT:Ensure that sensitive operations like modifying the cart are protected from cross-site request forgery (CSRF) attacks. You can do this by adding the [ValidateAntiForgeryToken] attribute to your actions.
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return Json(new { success = false, message = "Product ID is required." });
                }
                var products = _context.IceCream.ToList();
                var itemInfo = products.FirstOrDefault(p => p.Id == productId);
                if (itemInfo == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }
                HttpContext.Session.LoadAsync().Wait();
                string cartId = GetOrCreateCartId();
                var cartItems = _context.CartItem.ToList();
                var existingItem = cartItems.FirstOrDefault(
                         cartItem => cartItem.CartId == cartId && cartItem.ItemId == productId
                         );
                if (existingItem != null)
                {
                    // Update quantity if the item is already in the cart
                    existingItem.Quantity -= 1;
                    existingItem.Price -= itemInfo.Price;
                    if (existingItem.Quantity == 0)
                    {
                        _context.CartItem.Remove(existingItem);
                    }
                }
                else
                {
                    //todo: problem!
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Product removed from cart" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.ToString() });
            }

        }
        [HttpGet]
        public async Task<IActionResult> MyCart()
        {
            try
            {
                HttpContext.Session.LoadAsync().Wait();
                string cartId = GetOrCreateCartId();

                var cartItemsWithIceCream = await (
                    from cartItem in _context.CartItem
                    join iceCream in _context.IceCream on cartItem.ItemId equals iceCream.Id
                    where cartItem.CartId == cartId
                    select new CartView
                    {
                        CartItem = cartItem,
                        IceCream = iceCream
                    }
                ).ToListAsync();

                ViewBag.Place = "My Cart";
                return View(cartItemsWithIceCream);
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        [HttpPost("/Cart/PayAsync")]
        public async Task<IActionResult> PayAsync(string street, string city, int houseNumber, string phoneNumber, string fullName, string email, decimal total)
        {
            HttpContext.Session.LoadAsync().Wait();
            // Validate payment information if needed
            string cartId = GetOrCreateCartId();

            bool isValidAddresss = await _apiServices.CheckAddressExistence(city, street);
            bool _IsItHoliday = await _apiServices.IsItHoliday();
            if(isValidAddresss==false)
            {
                ModelState.AddModelError("Street", "Invalid address. Please enter a valid city and street.");
                return Json(new { success = false, errorCode = "InvalidAddress", message = "Invalid address. Please enter a valid city and street." });
            }
            Weather wez = await _apiServices.FindWeatherAsync(city);
            double _FeelsLike = (double)wez.FeelsLike;
            double _Humidity = (double)wez.Humidity;

            // Create a new order with the provided information
            var newOrder = new Models.Order
            {
                FirstName = fullName.Split(' ')[0],
                LastName = fullName.Split(' ')[1],
                PhoneNumber = phoneNumber,
                Email = email,
                Street = street,
                City = city,
                HouseNumber = houseNumber,
                Total = (double)total,
                Products = _context.CartItem.Where(ci => ci.CartId == cartId).ToList(),
                Date = DateTime.Now,
                FeelsLike = _FeelsLike,
                Humidity = _Humidity,
                IsItHoliday = _IsItHoliday,
                Day = (Models.DayOfWeek)DateTime.Now.DayOfWeek
                // Add other properties as needed
            };

            // Add the order to the database
            _context.Order.Add(newOrder);
            _context.CartItem.RemoveRange(_context.CartItem);
            _context.SaveChanges();
            // Clear the entire session after processing the payment
            HttpContext.Session.Clear();
            //return View("~/Views/Home/Ordering.cshtml", newOrder);
            return Json(new { success = true, message = "Payment successful" });
        }

        // Add this method to configure session services
        private void ConfigureSessionServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        // Add this method to configure session middleware
        private void ConfigureSessionMiddleware(IApplicationBuilder app)
        {
            app.UseSession();
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