namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Moderator")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _appDbContext;

        public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var userName = HttpContext?.User?.Identity?.Name;

            var users = await _userManager.Users.Where(u => u.UserName != userName).ToListAsync();

            List<AllUserViewModel> allUsers = new();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                allUsers.Add(new AllUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    IsActive = user.IsActive,
                    Role = userRoles.FirstOrDefault()
                });
            }

            return View(allUsers);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault();

            if (userRole == RoleType.Admin.ToString())
                return BadRequest();

            UserViewModel userViewModel = new()
            {
                Role = userRole
            };

            ViewBag.Roles = _roleManager.Roles.ToList();

            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string id, UserViewModel userViewModel)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault();

            if (userRole == RoleType.Admin.ToString())
                return BadRequest();

            await _userManager.RemoveFromRoleAsync(user, userRole);

            await _userManager.AddToRoleAsync(user, userViewModel.Role);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeActivity(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault();

            if (userRole == RoleType.Admin.ToString())
                return BadRequest();

            UserActiveViewModel userActiveViewModel = new()
            {
                IsActive = user.IsActive
            };

            return View(userActiveViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeActivity(string id, UserActiveViewModel userActiveViewModel)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault();

            if (userRole == RoleType.Admin.ToString())
                return BadRequest();

            user.IsActive = userActiveViewModel.IsActive;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
