using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;
using X.PagedList;

namespace InventoryManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryContext _context;

        public InventoryController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Inventory
        public async Task<IActionResult> Index(string sortOrder, string searchString, string quantityFilter, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = sortOrder == "Code" ? "code_desc" : "Code";
            ViewBag.QuantitySortParm = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewBag.CurrentFilter = searchString;
            ViewBag.QuantityFilter = quantityFilter;

            var inventoryMaterials = from m in _context.InventoryMaterials
                                     select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                inventoryMaterials = inventoryMaterials.Where(s => s.Name.ToLower().Contains(searchString.ToLower())
                                                                  || s.Code.ToLower().Contains(searchString.ToLower()));
            }

            if (!String.IsNullOrEmpty(quantityFilter))
            {
                switch (quantityFilter)
                {
                    case "low":
                        inventoryMaterials = inventoryMaterials.Where(s => s.Quantity < 10);
                        break;
                    case "medium":
                        inventoryMaterials = inventoryMaterials.Where(s => s.Quantity >= 10 && s.Quantity <= 50);
                        break;
                    case "high":
                        inventoryMaterials = inventoryMaterials.Where(s => s.Quantity > 50);
                        break;
                }
            }

            inventoryMaterials = inventoryMaterials.OrderByDescending(s => s.Code);

            switch (sortOrder)
            {
                case "name_desc":
                    inventoryMaterials = inventoryMaterials.OrderByDescending(s => s.Name);
                    break;
                case "Code":
                    inventoryMaterials = inventoryMaterials.OrderBy(s => s.Code);
                    break;
                case "code_desc":
                    inventoryMaterials = inventoryMaterials.OrderByDescending(s => s.Code);
                    break;
                case "Quantity":
                    inventoryMaterials = inventoryMaterials.OrderBy(s => s.Quantity);
                    break;
                case "quantity_desc":
                    inventoryMaterials = inventoryMaterials.OrderByDescending(s => s.Quantity);
                    break;
                default:
                    inventoryMaterials = inventoryMaterials.OrderByDescending(s => s.Code);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(await inventoryMaterials.AsNoTracking().ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryMaterial = await _context.InventoryMaterials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryMaterial == null)
            {
                return NotFound();
            }

            return View(inventoryMaterial);
        }

        // GET: Inventory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Quantity")] InventoryMaterial inventoryMaterial)
        {
            if (ModelState.IsValid)
            {
                // Temporary code
                inventoryMaterial.Code = "TEMP";
                _context.Add(inventoryMaterial);
                await _context.SaveChangesAsync();

                // Update the code to "I-(current)"
                int currentCount = _context.InventoryMaterials.Count();
                inventoryMaterial.Code = $"I-{currentCount}";
                _context.Update(inventoryMaterial);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Log or debug ModelState errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(inventoryMaterial);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryMaterial = await _context.InventoryMaterials.FindAsync(id);
            if (inventoryMaterial == null)
            {
                return NotFound();
            }
            return View(inventoryMaterial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Description,Quantity")] InventoryMaterial inventoryMaterial)
        {
            if (id != inventoryMaterial.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventoryMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryMaterialExists(inventoryMaterial.Id))
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
            return View(inventoryMaterial);
        }

        private bool InventoryMaterialExists(int id)
        {
            return _context.InventoryMaterials.Any(e => e.Id == id);
        }

        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryMaterial = await _context.InventoryMaterials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryMaterial == null)
            {
                return NotFound();
            }

            return View(inventoryMaterial);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryMaterial = await _context.InventoryMaterials.FindAsync(id);
            if (inventoryMaterial != null)
            {
                _context.InventoryMaterials.Remove(inventoryMaterial);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
