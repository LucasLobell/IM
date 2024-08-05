using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;
using X.PagedList;

namespace InventoryManagement.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly InventoryContext _context;

        public MaterialsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Materials
        public IActionResult Index()
        {
            return View("Index");
        }

        // GET: Materials/MarketingArtifacts
        public async Task<IActionResult> MarketingArtifacts(string sortOrder, string searchString, string quantityFilter, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = sortOrder == "Code" ? "code_desc" : "Code";
            ViewBag.QuantitySortParm = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewBag.CurrentFilter = searchString;
            ViewBag.QuantityFilter = quantityFilter;

            var materials = from m in _context.Materials
                            select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                materials = materials.Where(s => s.Name.ToLower().Contains(searchString.ToLower())
                                                 || s.Code.ToLower().Contains(searchString.ToLower()));
            }

            if (!String.IsNullOrEmpty(quantityFilter))
            {
                switch (quantityFilter)
                {
                    case "low":
                        materials = materials.Where(s => s.Quantity < 10);
                        break;
                    case "medium":
                        materials = materials.Where(s => s.Quantity >= 10 && s.Quantity <= 50);
                        break;
                    case "high":
                        materials = materials.Where(s => s.Quantity > 50);
                        break;
                }
            }

            // Order by Code descending
            materials = materials.OrderByDescending(s => s.Code);

            switch (sortOrder)
            {
                case "name_desc":
                    materials = materials.OrderByDescending(s => s.Name);
                    break;
                case "Code":
                    materials = materials.OrderBy(s => s.Code);
                    break;
                case "code_desc":
                    materials = materials.OrderByDescending(s => s.Code);
                    break;
                case "Quantity":
                    materials = materials.OrderBy(s => s.Quantity);
                    break;
                case "quantity_desc":
                    materials = materials.OrderByDescending(s => s.Quantity);
                    break;
                default:
                    materials = materials.OrderByDescending(s => s.Code);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(await materials.AsNoTracking().ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Quantity")] Material material)
        {
            if (ModelState.IsValid)
            {
                // Temporary code
                material.Code = "TEMP";
                _context.Add(material);
                await _context.SaveChangesAsync();

                // Update the code to "M-(current)"
                int currentCount = _context.Materials.Count();
                material.Code = $"M-{currentCount}";
                _context.Update(material);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Log or debug ModelState errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(material);
        }

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Description,Quantity")] Material material)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            var existingMaterial = await _context.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existingMaterial == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    material.Code = existingMaterial.Code; // Preserve the code
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
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
            return View(material);
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Materials/Enter/5
        public async Task<IActionResult> Enter(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Enter/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(int id, int amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            material.Enter(amount);
            _context.Update(material);

            // Log the entry in history
            var history = new History
            {
                MaterialId = id,
                ActionType = "Enter",
                Amount = amount,
                Timestamp = DateTime.Now
            };
            _context.Histories.Add(history);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Materials/Exit/5
        public async Task<IActionResult> Exit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Exit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Exit(int id, int amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            material.Exit(amount);
            _context.Update(material);

            // Log the exit in history
            var history = new History
            {
                MaterialId = id,
                ActionType = "Exit",
                Amount = amount,
                Timestamp = DateTime.Now
            };
            _context.Histories.Add(history);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
