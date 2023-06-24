namespace CAFinal.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HeaderViewComponent(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            var userName = HttpContext?.User?.Identity?.Name;
            ViewBag.UserName = userName;

            ViewBag.IsLog = User.Identity.IsAuthenticated;

            ViewBag.AdminAccess = User.IsInRole("Member");

            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                bool userIsActive = !user.IsActive && user != null;
                ViewBag.UserIsActive = userIsActive;
            }
            else
            {
                ViewBag.UserIsActive = false;
            }
            
            return View(settings);

        }
    }
}

