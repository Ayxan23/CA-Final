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

            var baskets = await _context.Baskets.OrderByDescending(p => p.Id).Include(p => p.Product).ToListAsync();

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                baskets = await _context.Baskets.OrderByDescending(p => p.Id).Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();

                var countFind = await _context.Counts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (countFind != null)
                {
                    countFind.CountNum = 1;
                    await _context.SaveChangesAsync();
                }
            }
            if (baskets.Count == 0)
            {
                return RedirectToAction("Empty");
            }

            TempData["TotalPrice"] = $"{(double)baskets.Sum(i => i.Product.Price * i.Count)}";

            return View(baskets);
        }

        [Authorize]
        public async Task<IActionResult> Buy()
        {
            var userName = HttpContext?.User?.Identity?.Name;

            var user = await _userManager.FindByNameAsync(userName);
            var baskets = await _context.Baskets.OrderByDescending(p => p.ModifiedAt).Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();

            if (baskets.Count == 0)
            {
                return RedirectToAction("Empty");
            }

            TempData["TotalPrice"] = $"{(double)baskets.Sum(i => i.Product.Price * i.Count)}";

            ViewBag.User = user;

            AddressViewModel addressViewModel = new()
            {
                PhoneNumber = user.PhoneNumber,
            };

            if (user.Address != null)
            {
                addressViewModel.Address = AdressCut.CutStr(user.Address, "Address:", ",");
                addressViewModel.City = AdressCut.CutStr(user.Address, "City:", ",");
                addressViewModel.Postcode = AdressCut.CutStr(user.Address, "Postcode:", ",");
                addressViewModel.Home = AdressCut.CutStr(user.Address, "Home:", ",");
            }

            return View(addressViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Buy(AddressViewModel addressViewModel)
        {
            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var baskets = await _context.Baskets.Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();
            TempData["TotalPrice"] = $"{(double)baskets.Sum(i => i.Product.Price * i.Count)}";
            ViewBag.User = user;

            if (!ModelState.IsValid)
                return View();

            user.PhoneNumber = addressViewModel.PhoneNumber;
            user.Address = $"Address:{addressViewModel.Address}, City:{addressViewModel.City}, Postcode:{addressViewModel.Postcode}, Home:{addressViewModel.Home},";
            await _context.SaveChangesAsync();

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string? url = Url.Action("CreditCard", "Basket", new { userId = user.Id, token }, HttpContext.Request.Scheme);
            return Redirect(url);
        }

        [Authorize]
        public async Task<IActionResult> CreditCard(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (string.IsNullOrWhiteSpace(resetPasswordViewModel.UserId) || string.IsNullOrWhiteSpace(resetPasswordViewModel.Token))
                return BadRequest();

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                var baskets = await _context.Baskets.Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();
                TempData["TotalPrice"] = $"{(double)baskets.Sum(i => i.Product.Price * i.Count)}";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreditCard(CartViewModel cartViewModel, string? userId, string? token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            if (cartViewModel.Year < DateTime.Now.Year)
            {
                ModelState.AddModelError("Year", "Year is Invalid");
                return View();
            }
            if (cartViewModel.Cvv < 100)
            {
                ModelState.AddModelError("Cvv", "Cvv is Invalid");
                return View();
            }
            if (cartViewModel.Month >= 12 && cartViewModel.Month < 0)
            {
                ModelState.AddModelError("Month", "Month is Invalid");
                return View();
            }
            if (cartViewModel.CardNumber < 1000000000000000)
            {
                ModelState.AddModelError("CardNumber", "CardNumber is Invalid");
                return View();
            }

            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                var baskets = await _context.Baskets.Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();
                TempData["TotalPrice"] = $"{(double)baskets.Sum(i => i.Product.Price * i.Count)}";

                foreach (var basket in baskets)
                {
                    basket.IsPay = true;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Shop");
        }

        [Authorize]
        public async Task<IActionResult> Empty()
        {
            var userName = HttpContext?.User?.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                var baskets = await _context.Baskets.OrderByDescending(p => p.Id).Where(p => p.AppUserId == user.Id).Include(p => p.Product).ToListAsync();

                if (baskets.Count > 0)
                {
                    return RedirectToAction("Index");
                }
            }
            return View();
        }
    }
}