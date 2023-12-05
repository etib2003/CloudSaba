using CloudSaba.Data;
using CloudSaba.Models;
using CloudSaba.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace CloudSaba.Controllers
{
    public class OrdersController : Controller
    {

        private readonly CloudSabaContext _context;
        private readonly ApiServices _apiServices; // Add this line


        public OrdersController(CloudSabaContext context, ApiServices apiServices) // Add ApiServices parameter
        {
            _context = context;
            _apiServices = apiServices; // Initialize ApiServices

        }

        
        public IActionResult GraphCreate()
        {
            return View();
        }
        public IActionResult Graph(DateTime? start, DateTime? end)
        
       {
            var orders = _context.Order.Where(order => order.Date >= start && order.Date <= end).ToList();

            // Prepare data for the view model
            var dateLabels = orders.Select(order => order.Date.ToShortDateString()).Distinct().ToList();
            var totalPrices = new List<double>();
            var orderCounts = new List<int>();

            foreach (var dateLabel in dateLabels)
            {
                orderCounts.Add(orders.Count(order => order.Date.ToShortDateString() == dateLabel));
            }
            foreach (var dateLabel in dateLabels)
            {
                totalPrices.Add(orders.Where(order => order.Date.ToShortDateString() == dateLabel).Sum(order => order.Total));
            }

            var viewModel = new OrderGraphViewModel
            {
                DateLabels = dateLabels,
                TotalPrices = totalPrices,
                OrderCounts = orderCounts // Add this line
            };
            ViewBag.Place = "Graph";
            return View(viewModel); // Pass the view model to the view
        }



        // GET: Orders
        public async Task<IActionResult> Index()
        {
            ViewBag.Place = "Orders";
            return _context.Order != null ?
                        View(await _context.Order.ToListAsync()) :
                        Problem("Entity set 'CloudSabaContext.Order'  is null.");
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,Email,Street,City,HouseNumber,Total,Date,FeelsLike,Humidity,IsItHoliday,Day")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.Date = DateTime.Now;
                order.Day = (Models.DayOfWeek)DateTime.Now.DayOfWeek;

                Weather wez = await _apiServices.FindWeatherAsync(order.City);
                order.FeelsLike = (double)wez.FeelsLike;
                order.Humidity = (double)wez.Humidity; 

                bool isValidAddresss = await _apiServices.CheckAddressExistence(order.City.ToString(), order.Street.ToString());
                order.IsItHoliday = await _apiServices.IsItHoliday();
                if (isValidAddresss)
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Street", "Invalid address. Please enter a valid city and street.");
                }

            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,PhoneNumber,Email,Street,City,HouseNumber,Total,Date,FeelsLike,Humidity,IsItHoliday,Day")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                order.Date = DateTime.Now;
                order.Day = (Models.DayOfWeek)DateTime.Now.DayOfWeek;

                Weather wez = await _apiServices.FindWeatherAsync(order.City);
                order.FeelsLike = (double)wez.FeelsLike;
                order.Humidity = (double)wez.Humidity;

                bool isValidAddresss = await _apiServices.CheckAddressExistence(order.City.ToString(), order.Street.ToString());
                order.IsItHoliday = await _apiServices.IsItHoliday();
                if (isValidAddresss)
                {
                    try
                    {
                        _context.Update(order);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(order.Id))
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
                else
                {
                    ModelState.AddModelError("Street", "Invalid address. Please enter a valid city and street.");

                }

            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Order == null)
            {
                return Problem("Entity set 'CloudSabaContext.Order'  is null.");
            }
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return (_context.Order?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
