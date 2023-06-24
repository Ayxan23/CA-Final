using Microsoft.AspNetCore.Mvc;

namespace CAFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.OrderByDescending(s => s.ModifiedAt).ToListAsync();
            var services = await _context.Services.OrderByDescending(s => s.ModifiedAt).Take(4).ToListAsync();
            var products = await _context.Products.OrderByDescending(p => p.ModifiedAt).Take(6).ToListAsync();
            var category = await _context.Categories.OrderByDescending(p => p.ModifiedAt).Take(4).ToListAsync();


            return View(sliders);
        }
    }
}
