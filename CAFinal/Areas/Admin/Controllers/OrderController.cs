using CAFinal.Models;
using Microsoft.AspNetCore.Hosting;

namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var baskets = await _context.Baskets.OrderBy(p => p.Id).Where(p => p.IsPay == true).Include(p => p.Product).Include(p => p.AppUser).IgnoreQueryFilters().ToListAsync();
            return View(baskets);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Detail(int id)
        {
            var basket = await _context.Baskets.IgnoreQueryFilters().Include(p => p.Product).Include(p => p.AppUser).FirstOrDefaultAsync(c => c.Id == id);
            if (basket is null)
                return NotFound();

            return View(basket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var basket = await _context.Baskets.IgnoreQueryFilters().Include(p => p.Product).Include(p => p.AppUser).FirstOrDefaultAsync(c => c.Id == id);
            if (basket is null)
                return NotFound();

            return View(basket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var basket = await _context.Baskets.IgnoreQueryFilters().Include(p => p.Product).Include(p => p.AppUser).FirstOrDefaultAsync(c => c.Id == id);
            if (basket is null)
                return NotFound();

            _context.Baskets.Remove(basket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
