namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public SliderController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _count = _context.Sliders.AsEnumerable().Count();
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.OrderByDescending(s => s.ModifiedAt).ToListAsync();
            ViewBag.Count = _count;

            return View(sliders);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(SliderViewModel sliderViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            if (sliderViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "Image bosh ola bilmez");
                return View();
            }
            if (!sliderViewModel.Image.CheckFileSize(300))
            {
                ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                return View();
            }
            if (!sliderViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
            {
                ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{sliderViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
            using (FileStream stream = new(path, FileMode.Create))
            {
                await sliderViewModel.Image.CopyToAsync(stream);
            }

            Slider slider = new()
            {
                Title = sliderViewModel.Title,
                Offer = sliderViewModel.Offer,
                Description = sliderViewModel.Description,
                Image = fileName,
                IsDeleted = false
            };

            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null)
                return NotFound();

            return View(slider);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null)
                return NotFound();

            SliderViewModel sliderViewModel = new()
            {
                Id = slider.Id,
                Title = slider.Title,
                Offer = slider.Offer,
                Description = slider.Description,
            };

            return View(sliderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, SliderViewModel sliderViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null)
                return NotFound();

            if (sliderViewModel.Image != null)
            {
                if (!sliderViewModel.Image.CheckFileSize(300))
                {
                    ModelState.AddModelError("Image", "Faylin hecmi 300kb-dan kicik olmalidir.");
                    return View();
                }
                if (!sliderViewModel.Image.CheckFileType(ContentTypes.image.ToString()))
                {
                    ModelState.AddModelError("Image", "Faylin tipi image olmalidir.");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", slider.Image);
                string fileName = $"{Guid.NewGuid()}-{sliderViewModel.Image.FileName}";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img", fileName);
                using (FileStream stream = new(path, FileMode.Create))
                {
                    await sliderViewModel.Image.CopyToAsync(stream);
                }
                slider.Image = fileName;
            }

            slider.Title = sliderViewModel.Title;
            slider.Description = sliderViewModel.Description;
            slider.Offer = sliderViewModel.Offer;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (_count == 1)
                return BadRequest();

            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null)
                return NotFound();

            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (_count == 1)
                return BadRequest();

            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "img", slider.Image);
            slider.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
