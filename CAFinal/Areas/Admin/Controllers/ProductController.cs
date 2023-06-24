namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.OrderByDescending(p => p.ModifiedAt).ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.AsEnumerable();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            ViewBag.Categories = _context.Categories.AsEnumerable();

            if (!ModelState.IsValid)
                return View();

            if (productViewModel.CategoryIds == null)
            {
                ModelState.AddModelError("CategoryIds", "The Categories field is required");
                return View();
            }

            foreach (var categoryId in productViewModel.CategoryIds)
            {
                if (!_context.Categories.Any(c => c.Id == categoryId))
                    return BadRequest();
            }

            if (await _context.Products.AnyAsync(c => c.Name == productViewModel.Name))
            {
                ModelState.AddModelError("Name", "Name already exist");
                return View();
            }

            if (productViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "Image bosh ola bilmez");
                return View();
            }
            if (!productViewModel.Image.CheckFileSize(300))
            {
                ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                return View();
            }
            if (!productViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
            {
                ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{productViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
            using (FileStream stream = new(path, FileMode.Create))
            {
                await productViewModel.Image.CopyToAsync(stream);
            }

            Product product = new()
            {
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Image = fileName,
                Price = productViewModel.Price,
                IsDeleted = false
            };

            List<ProductCategory> productCategories = new();
            foreach (var categoryId in productViewModel.CategoryIds)
            {
                ProductCategory productCategory = new()
                {
                    ProductId = productViewModel.Id,
                    CategoryId = categoryId
                };
                productCategories.Add(productCategory);
            }
            product.ProductCategories = productCategories;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return NotFound();

            ViewBag.Categories = _context.Categories.AsEnumerable();

            ProductViewModel productViewModel = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
            };

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, ProductViewModel productViewModel)
        {
            ViewBag.Categories = _context.Categories.AsEnumerable();

            if (!ModelState.IsValid)
                return View();

            if (productViewModel.CategoryIds != null)
            {
                foreach (var categoryId in productViewModel.CategoryIds)
                {
                    if (!_context.Categories.Any(c => c.Id == categoryId))
                        return BadRequest();
                }
            }

            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return NotFound();

            if (await _context.Products.AnyAsync(p => p.Name == productViewModel.Name && p.Name != product.Name))
            {
                ModelState.AddModelError("Name", "Name already exist");
                return View();
            }

            if (productViewModel.Image != null)
            {
                if (!productViewModel.Image.CheckFileSize(300))
                {
                    ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                    return View();
                }
                if (!productViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
                {
                    ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", product.Image);
                string fileName = $"{Guid.NewGuid()}-{productViewModel.Image.FileName}";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
                using (FileStream stream = new(path, FileMode.Create))
                {
                    await productViewModel.Image.CopyToAsync(stream);
                }
                product.Image = fileName;
            }

            product.Name = productViewModel.Name;
            product.Description = productViewModel.Description;
            product.Price = productViewModel.Price;

            if (productViewModel.CategoryIds != null)
            {
                List<ProductCategory> productCategories = new();
                foreach (var categoryId in productViewModel.CategoryIds)
                {
                    ProductCategory productCategory = new()
                    {
                        ProductId = productViewModel.Id,
                        CategoryId = categoryId
                    };
                    productCategories.Add(productCategory);
                }
                product.ProductCategories = productCategories;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", product.Image);
            product.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
