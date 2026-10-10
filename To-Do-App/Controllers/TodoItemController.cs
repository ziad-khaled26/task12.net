using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using To_Do_App.Data;
using To_Do_App.Models;
namespace To_Do_App.Controllers
{
    public class TodoItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TodoItemController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(DateTime? searchDate, int page = 1)
        {
            int pageSize = 5;
            var query = _context.TodoItems.AsQueryable();

            if (searchDate.HasValue)
            {
                query = query.Where(t => t.Deadline.Date == searchDate.Value.Date);
            }

            query = query.OrderBy(t => t.Deadline);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItem item, IFormFile file)

        {
            ModelState.Remove("FilePath");
            ModelState.Remove("FileName");
            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    item.FilePath = "/uploads/" + uniqueFileName;
                    item.FileName = file.FileName;
                }

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TodoItem item, IFormFile file)
        {
            if (id != item.Id) return NotFound();
            ModelState.Remove("FilePath");
            ModelState.Remove("FileName");
            ModelState.Remove("file");

            if (ModelState.IsValid)
            {
                var existingItem = await _context.TodoItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                item.FilePath = existingItem.FilePath;
                item.FileName = existingItem.FileName;

                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    item.FilePath = "/uploads/" + uniqueFileName;
                    item.FileName = file.FileName;
                }

                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item != null)
            {
                _context.TodoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
