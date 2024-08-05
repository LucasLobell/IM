using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly InventoryContext _context;

        public ReportsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Reports/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/History
        public async Task<IActionResult> History(string sortOrder, string searchString, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.ActionSortParm = String.IsNullOrEmpty(sortOrder) ? "action_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = searchString;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var histories = from h in _context.Histories
                            .Include(h => h.Material)
                            select h;

            if (!String.IsNullOrEmpty(searchString))
            {
                histories = histories.Where(h => h.Material.Name.Contains(searchString) || h.Material.Code.Contains(searchString));
            }

            if (startDate.HasValue)
            {
                histories = histories.Where(h => h.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                histories = histories.Where(h => h.Timestamp <= endDate.Value);
            }

            // Default to sorting by Timestamp descending
            histories = histories.OrderByDescending(h => h.Timestamp);

            switch (sortOrder)
            {
                case "action_desc":
                    histories = histories.OrderByDescending(h => h.ActionType);
                    break;
                case "Date":
                    histories = histories.OrderBy(h => h.Timestamp);
                    break;
                case "date_desc":
                    histories = histories.OrderByDescending(h => h.Timestamp);
                    break;
                default:
                    histories = histories.OrderByDescending(h => h.Timestamp); // Ensure default is correct
                    break;
            }

            return View(await histories.AsNoTracking().ToListAsync());
        }
    }
}
