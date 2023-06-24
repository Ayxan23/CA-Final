namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AboutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public AboutController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _count = _context.Abouts.AsEnumerable().Count();
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var abouts = await _context.Abouts.ToListAsync();

            ViewBag.Count = _count;

            return View(abouts);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            if (_count == 1)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(AboutViewModel aboutViewModel)
        {
            if (_count == 1)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            if (aboutViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "Image bosh ola bilmez");
                return View();
            }
            if (!aboutViewModel.Image.CheckFileSize(300))
            {
                ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                return View();
            }
            if (!aboutViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
            {
                ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{aboutViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
            using (FileStream stream = new(path, FileMode.Create))
            {
                await aboutViewModel.Image.CopyToAsync(stream);
            }

            About about = new()
            {
                Title = aboutViewModel.Title,
                Description = aboutViewModel.Description,
                Image = fileName,
                IsDeleted = false
            };

            await _context.Abouts.AddAsync(about);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var about = await _context.Abouts.FirstOrDefaultAsync(a => a.Id == id);
            if (about is null)
                return NotFound();

            return View(about);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var about = await _context.Abouts.FirstOrDefaultAsync(a => a.Id == id);
            if (about is null)
                return NotFound();

            AboutViewModel aboutViewModel = new()
            {
                Id = about.Id,
                Title = about.Title,
                Description = about.Description,
            };

            return View(aboutViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, AboutViewModel aboutViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var about = await _context.Abouts.FirstOrDefaultAsync(a => a.Id == id);
            if (about is null)
                return NotFound();

            if (aboutViewModel.Image != null)
            {
                if (!aboutViewModel.Image.CheckFileSize(300))
                {
                    ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                    return View();
                }
                if (!aboutViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
                {
                    ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", about.Image);
                string fileName = $"{Guid.NewGuid()}-{aboutViewModel.Image.FileName}";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
                using (FileStream stream = new(path, FileMode.Create))
                {
                    await aboutViewModel.Image.CopyToAsync(stream);
                }
                about.Image = fileName;
            }

            about.Title = aboutViewModel.Title;
            about.Description = aboutViewModel.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var about = await _context.Abouts.FirstOrDefaultAsync(a => a.Id == id);
            if (about is null)
                return NotFound();

            return View(about);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var about = await _context.Abouts.FirstOrDefaultAsync(a => a.Id == id);
            if (about is null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", about.Image);
            about.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
