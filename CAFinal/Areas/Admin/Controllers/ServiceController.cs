namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public ServiceController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _count = _context.Services.AsEnumerable().Count();
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.OrderByDescending(s => s.ModifiedAt).ToListAsync();

            ViewBag.Count = _count;

            return View(services);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            if (_count == 4)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(ServiceViewModel serviceViewModel)
        {
            if (_count == 4)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            Service service = new()
            {
                Title = serviceViewModel.Title,
                Description = serviceViewModel.Description,
                Icon = serviceViewModel.Icon,
                IsDeleted = false
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service is null)
                return NotFound();

            return View(service);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service is null)
                return NotFound();

            ServiceViewModel serviceViewModel = new()
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                Icon = service.Icon,
            };

            return View(serviceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, ServiceViewModel serviceViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service is null)
                return NotFound();

            service.Title = serviceViewModel.Title;
            service.Description = serviceViewModel.Description;
            service.Icon = serviceViewModel.Icon;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service is null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service is null)
                return NotFound();

            service.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
