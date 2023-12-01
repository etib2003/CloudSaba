using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudSaba.Data;
using CloudSaba.Models;
namespace CloudSaba.Controllers
{
        public class HomeController : Controller
        {
            private readonly CloudSabaContext _context;

            public HomeController(CloudSabaContext context)
            {
                _context = context;
            }

            // GET: HomeController
            public ActionResult Index()
            {
                ViewBag.Place = "Home";
                return View("Index");
            }

            public ActionResult About()
            {
                ViewBag.Place = "About";
                return View("About");
            }

            // GET: HomeController/Product
            public async Task<IActionResult> Product()
            {
                ViewBag.Place = "Product";

                // Fetch products from the database
                var products = await _context.IceCream
                    .Select(p => new IceCream
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl,
                        Details = p.Details
                    })
                    .ToListAsync();

                return View(products);
            }

            public ActionResult Gallery()
            {
                ViewBag.Place = "Gallery";
                return View("Gallery");
            }

            public ActionResult Service()
            {
                ViewBag.Place = "Service";
                return View("Service");
            }

            public ActionResult Contact()
            {
                ViewBag.Place = "Contact";
                return View("Contact");
            }

            // GET: HomeController/Details/5
            public ActionResult Details(int id)
            {
                return View();
            }

            // GET: HomeController/Create
            public ActionResult Create()
            {
                return View();
            }

            // POST: HomeController/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            // GET: HomeController/Edit/5
            public ActionResult Edit(int id)
            {
                return View();
            }

            // POST: HomeController/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(int id, IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            // GET: HomeController/Delete/5
            public ActionResult Delete(int id)
            {
                return View();
            }

            // POST: HomeController/Delete/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Delete(int id, IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
        }
    }




    /*Eti switched between this and the one above
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    namespace IceCream8218_9018_4713.Controllers
    {
        public class HomeController : Controller
        {
            // GET: HomeController
            public ActionResult Index()
            {
                ViewBag.Place = "Home";
                return View("Index");
            }
            public ActionResult About()
            {
                ViewBag.Place = "About";
                return View("About");
            }
            public ActionResult Product()
            {
                ViewBag.Place = "Product";
                return View("Product");
            }
            public ActionResult Gallery()
            {
                ViewBag.Place = "Gallery";
                return View("Gallery");
            }
            public ActionResult Service()
            {
                ViewBag.Place = "Service";
                return View("Service");
            }
            public ActionResult Contact()
            {
                ViewBag.Place = "Contact";
                return View("Contact");
            }

            // GET: HomeController/Details/5
            public ActionResult Details(int id)
            {
                return View();
            }

            // GET: HomeController/Create
            public ActionResult Create()
            {
                return View();
            }

            // POST: HomeController/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            // GET: HomeController/Edit/5
            public ActionResult Edit(int id)
            {
                return View();
            }

            // POST: HomeController/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(int id, IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            // GET: HomeController/Delete/5
            public ActionResult Delete(int id)
            {
                return View();
            }

            // POST: HomeController/Delete/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Delete(int id, IFormCollection collection)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
        }
    }
    */


