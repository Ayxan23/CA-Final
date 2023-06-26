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
            var category = await _context.Categories.Include(p => p.ProductCategories).ThenInclude(pc => pc.Product).OrderByDescending(c => c.ModifiedAt).Take(4).ToListAsync();

            HomeViewModel homeViewModel = new()
            {
                Sliders = sliders,
                Services = services,
                Products = products,
                Categories = category,
            };

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind != null)
                {
                    countFind.CountNum = 1;
                    await _context.SaveChangesAsync();
                }
            }

            return View(homeViewModel);
        }
    }
}
