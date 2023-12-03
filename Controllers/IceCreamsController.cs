﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudSaba.Data;
using CloudSaba.Models;
using Newtonsoft.Json;

namespace CloudSaba.Controllers
{
    public class IceCreamsController : Controller
    {
        private readonly CloudSabaContext _context;

        public IceCreamsController(CloudSabaContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckImage(string imageURL)
        {
            var apiUrl = $"http://localhost:5050/ImaggaApi/CheckImage?imageUrl={imageURL}";

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


        // GET: IceCreams
        public async Task<IActionResult> Index()
        {
              return _context.IceCream != null ? 
                          View(await _context.IceCream.ToListAsync()) :
                          Problem("Entity set 'CloudSabaContext.IceCream'  is null.");
        }

        // GET: IceCreams/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.IceCream == null)
            {
                return NotFound();
            }

            var iceCream = await _context.IceCream
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iceCream == null)
            {
                return NotFound();
            }

            return View(iceCream);
        }

        // GET: IceCreams/Create
        public IActionResult Create()
        {
            return View();
        }

        
        // POST: IceCreams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,ImageUrl,Details")] IceCream iceCream)
        {
            if (ModelState.IsValid)
            {
                bool isIceCream = await CheckImage(iceCream.ImageUrl.ToString());
                if (isIceCream)
                {
                    _context.Add(iceCream);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("ImageUrl", "Image does not contain ice cream. Please provide a valid image.");

                }
            }
            return View(iceCream);
        }


        // GET: IceCreams/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.IceCream == null)
            {
                return NotFound();
            }

            var iceCream = await _context.IceCream.FindAsync(id);
            if (iceCream == null)
            {
                return NotFound();
            }
            return View(iceCream);
        }

        // POST: IceCreams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Price,ImageUrl,Details")] IceCream iceCream)
        {
            if (id != iceCream.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool isIceCream = await CheckImage(iceCream.ImageUrl.ToString());
                if (isIceCream)
                {
                    try
                    {
                        _context.Update(iceCream);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!IceCreamExists(iceCream.Id))
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
                    ModelState.AddModelError("ImageUrl", "Image does not contain ice cream. Please provide a valid image.");
                }
            }
            return View(iceCream);
        }

        // GET: IceCreams/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.IceCream == null)
            {
                return NotFound();
            }

            var iceCream = await _context.IceCream
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iceCream == null)
            {
                return NotFound();
            }

            return View(iceCream);
        }

        // POST: IceCreams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.IceCream == null)
            {
                return Problem("Entity set 'CloudSabaContext.IceCream'  is null.");
            }
            var iceCream = await _context.IceCream.FindAsync(id);
            if (iceCream != null)
            {
                _context.IceCream.Remove(iceCream);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IceCreamExists(string id)
        {
          return (_context.IceCream?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
