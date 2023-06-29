using CAFinal.Areas.Admin.ViewModels;
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

            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Detail(int id, string count)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return NotFound();

            ProductCategoryViewModel productCategoryViewModel = new()
            {
                Product = product,
                Categories = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).ToListAsync(),
            };

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null && count == null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind != null)
                {
                    ViewBag.ProdCount = countFind.CountNum;

                }
                else
                {
                    ViewBag.ProdCount = 1;
                }
            }
            if (userName != null && count == "+")
            {
                var user = await _userManager.FindByNameAsync(userName);
                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind is null)
                {
                    Count countItem = new()
                    {
                        UserId = user.Id,
                        CountNum = 2,
                    };
                    ViewBag.ProdCount = countItem.CountNum;
                    await _context.Counts.AddAsync(countItem);
                }
                else
                {
                    countFind.CountNum++;
                    ViewBag.ProdCount = countFind.CountNum;
                }
                await _context.SaveChangesAsync();
            }
            if (userName != null && count == "-")
            {
                var user = await _userManager.FindByNameAsync(userName);
                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind is null)
                {
                    Count countItem = new()
                    {
                        UserId = user.Id,
                        CountNum = 2,
                    };
                    ViewBag.ProdCount = countItem.CountNum;
                    await _context.Counts.AddAsync(countItem);
                }
                else
                {
                    if (countFind.CountNum > 1)
                    {
                        countFind.CountNum--;
                    }
                    ViewBag.ProdCount = countFind.CountNum;
                }
                await _context.SaveChangesAsync();
            }

            return View(productCategoryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Detail))]
        [Authorize]
        public async Task<IActionResult> DetailPost(int id, int prodCount)
        {
            var product = await _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return NotFound();

            ProductCategoryViewModel productCategoryViewModel = new()
            {
                Product = product,
                Categories = await _context.Categories.Include(c => c.ProductCategories).ThenInclude(pc => pc.Product).ToListAsync(),
            };

            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var basketFind = await _context.Baskets.FirstOrDefaultAsync(c => c.ProductId == product.Id && c.AppUserId == user.Id);
            if (basketFind != null && userName != null)
            {
                basketFind.Count += prodCount;
                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind != null)
                {
                    countFind.CountNum = 1;
                    ViewBag.ProdCount = countFind.CountNum;
                }
            }
            else
            {
                Basket basket = new()
                {
                    ProductId = product.Id,
                    IsPay = false,
                    Count = prodCount,
                };
                if (userName != null)
                {
                    basket.AppUserId = user.Id;

                    var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                    if (countFind != null)
                    {
                        countFind.CountNum = 1;
                        ViewBag.ProdCount = countFind.CountNum;
                    }
                }

                await _context.Baskets.AddAsync(basket);
            }
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
