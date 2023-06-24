using Microsoft.AspNetCore.Hosting;

namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.OrderByDescending(c => c.ModifiedAt).ToListAsync();

            return View(categories);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(CategoryViewModel categoryViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            if (await _context.Categories.AnyAsync(c => c.Name == categoryViewModel.Name))
            {
                ModelState.AddModelError("Name", "Name already exist");
                return View();
            }

            if (categoryViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "Image bosh ola bilmez");
                return View();
            }
            if (!categoryViewModel.Image.CheckFileSize(300))
            {
                ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                return View();
            }
            if (!categoryViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
            {
                ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{categoryViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
            using (FileStream stream = new(path, FileMode.Create))
            {
                await categoryViewModel.Image.CopyToAsync(stream);
            }

            Category category = new()
            {
                Name = categoryViewModel.Name,
                Image = fileName,
                IsDeleted = false,
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var category = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return NotFound();

            return View(category);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return NotFound();

            CategoryViewModel categoryViewModel = new()
            {
                Id = category.Id,
                Name = category.Name,
            };

            return View(categoryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, CategoryViewModel categoryViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var category = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return NotFound();

            if (await _context.Categories.AnyAsync(c => c.Name == categoryViewModel.Name && c.Name != category.Name))
            {
                ModelState.AddModelError("Name", "Name already exist");
                return View();
            }

            if (categoryViewModel.Image != null)
            {
                if (!categoryViewModel.Image.CheckFileSize(300))
                {
                    ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                    return View();
                }
                if (!categoryViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
                {
                    ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", "course", category.Image);
                string fileName = $"{Guid.NewGuid()}-{categoryViewModel.Image.FileName}";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
                using (FileStream stream = new(path, FileMode.Create))
                {
                    await categoryViewModel.Image.CopyToAsync(stream);
                }
                category.Image = fileName;
            }

            category.Name = categoryViewModel.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [ActionName(nameof(Delete))]
        public async Task<IActionResult> DeletePost(int id)
        {
            var category = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", category.Image);
            category.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
