using CAFinal.Models;
using Microsoft.AspNetCore.Identity;

namespace CAFinal.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ShopController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var products = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).OrderByDescending(p => p.ModifiedAt).Take(6).ToListAsync();
            ViewBag.ProductsCount = _context.Products.Count();

            if (categoryId != null)
            {
                products = await _context.Products.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId)).Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).OrderByDescending(p => p.ModifiedAt).ToListAsync();
                ViewBag.ProductsCount = 0;
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = await _context.Products.Where(p => p.Name.Contains(search)).OrderByDescending(p => p.ModifiedAt).ToListAsync();
                ViewBag.ProductsCount = 0;
            }

            ViewBag.ShopCategories = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).ToListAsync();
            ViewBag.All = _context.Products.ToList().Count;

            return View(products);
        }

        public async Task<IActionResult> Detail(int id, int count)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return NotFound();

            ProductCategoryViewModel productCategoryViewModel = new()
            {
                Product = product,
                Categories = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).ToListAsync(),
            };
            if (count == 0)
            {
                count = 1;
            }
        
            ViewBag.ProdCount = count;

            return View(productCategoryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Detail))]
        [Authorize]
        public async Task<IActionResult> DetailPost(int id, int count)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return NotFound();

            ProductCategoryViewModel productCategoryViewModel = new()
            {
                Product = product,
                Categories = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).ToListAsync(),
            };

            Basket basket = new()
            {
                ProductId = product.Id,
                IsPay = false,
                Count = count,
            };

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                string userId = user.Id;
                basket.AppUserId = userId;
            }

            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();

            return View(productCategoryViewModel);
        }

        public async Task<IActionResult> LoadMore(int skip)
        {
            var products = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).OrderByDescending(p => p.ModifiedAt).Skip(skip).Take(6).ToListAsync();

            return PartialView("_ProductPartial", products);
        }
    }
}
