using CloudSaba.Data;
using CloudSaba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Weather = CloudSaba.Models.Weather;


namespace CloudSaba.Controllers
{
    public class OrdersController : Controller
    {

        private readonly CloudSabaContext _context;


        public OrdersController(CloudSabaContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckAddressExistence(string city, string street)
        {
            var apiUrl = $"http://localhost:5050/api/Address/check?city={city}&street={street}";

            // Create an instance of HttpClient
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Send a GET request to the other project's endpoint
                var response = await httpClient.GetAsync(apiUrl);


                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the response content manually
                    var result = JsonConvert.DeserializeObject<bool?>(content);

                    // Use the result directly in the if statement
                    return result ?? false; // If result is null, default to false
                }
                else
                {
                    // Handle the error
                    return false; // Return false or handle the error accordingly
                }
            }
        }

        public async Task<Weather> FindWeatherAsync(string city)
        {
            // Construct the URL to the API Gateway's GetWeather endpoint
            string apiUrl = $"http://localhost:5050/Weather?city={city}";

            try
            {
                // Create an instance of HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Send a GET request to the API Gateway's GetWeather endpoint
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the JSON response into a Weather object
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var weather = JsonConvert.DeserializeObject<Weather>(jsonContent);
                        return weather;
                    }
                    else
                    {
                        // Handle errors here
                        return new Weather();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return new Weather();
            }
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

            foreach (var dateLabel in dateLabels)
            {
                totalPrices.Add(orders.Where(order => order.Date.ToShortDateString() == dateLabel).Sum(order => order.Total));
            }

            var viewModel = new OrderGraphViewModel
            {
                DateLabels = dateLabels,
                TotalPrices = totalPrices
            };

            return View(viewModel); // Pass the view model to the view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> IsItHoliday()
        {
            // Construct the URL of the other project's endpoint
            string apiUrl = $"http://localhost:5050/Get";

            try
            {
                // Create an instance of HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Send a GET request to the other project's endpoint
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response content as a boolean value
                        bool isHoliday = bool.Parse(await response.Content.ReadAsStringAsync());

                        return isHoliday;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }





        // GET: Orders
        public async Task<IActionResult> Index()
        {
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

                Weather wez = await FindWeatherAsync(order.City);
                order.FeelsLike =(double) wez.FeelsLike;
                order.Humidity = (double)wez.Humidity;

                bool isValidAddresss = await CheckAddressExistence(order.City.ToString(), order.Street.ToString());
                order.IsItHoliday = await IsItHoliday();
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

                Weather wez = await FindWeatherAsync(order.City);
                order.FeelsLike = (double)wez.FeelsLike;
                order.Humidity = (double)wez.Humidity;

                bool isValidAddresss = await CheckAddressExistence(order.City.ToString(), order.Street.ToString());
                order.IsItHoliday = await IsItHoliday();
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
