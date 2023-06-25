using CAFinal.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAFinal.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index(int? prodId, string? delete, string? count)
        {
            var basket = await _context.Baskets.FirstOrDefaultAsync(a => a.Id == prodId);
            if (delete == "del")
            {
                if (basket is null)
                    return NotFound();

                _context.Baskets.Remove(basket);
                await _context.SaveChangesAsync();
            }
            if (count == "+" && basket != null)
            {
                basket.Count++;
                await _context.SaveChangesAsync();
            }
            else if (count == "-" && basket != null && basket.Count > 1)
            {
                basket.Count--;
                await _context.SaveChangesAsync();
            }

            var products = await _context.Baskets.OrderByDescending(p => p.ModifiedAt).Include(p => p.Product).ToListAsync();

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                products = await _context.Baskets.OrderByDescending(p => p.ModifiedAt).Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();

                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind != null)
                {
                    countFind.CountNum = 1;
                    await _context.SaveChangesAsync();
                }
            }

            return View(products);
        }

        public async Task<IActionResult> Buy()
        {
            return View();
        }
    }
}
