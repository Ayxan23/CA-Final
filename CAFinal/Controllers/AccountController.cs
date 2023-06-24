namespace CAFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            AppUser newUser = new()
            {
                Fullname = registerViewModel.Fullname,
                Email = registerViewModel.Email,
                UserName = registerViewModel.Username,
                PhoneNumber = registerViewModel.PhoneNumber,
                IsActive = true,
                EmailConfirmed = false,
            };

            var identityResult = await _userManager.CreateAsync(newUser, registerViewModel.Password);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

            await _userManager.AddToRoleAsync(newUser, RoleType.Member.ToString());

            string token = await _userManager.GeneratePasswordResetTokenAsync(newUser);
            string? url = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, token }, HttpContext.Request.Scheme);
            EmailHelper emailHelper = new();
            MailRequestViewModel mailRequestViewModel = new()
            {
                ToEmail = newUser.Email,
                Subject = "Confirm Email",
                Body = $"<a href='{url}'>Confirm Email</a>"
            };
            await emailHelper.SendEmailAsync(mailRequestViewModel);

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound();

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            var user = await _userManager.FindByNameAsync(loginViewModel.Username);
            if (user is null)
            {
                ModelState.AddModelError("", "Username or password invalid");
                return View();
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Your email is not confirmed");
                return View();
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is blocked");
                return View();
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, true);
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account is blocked temporary");
                return View();
            }
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Username or password invalid");
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (var roleType in Enum.GetValues(typeof(RoleType)))
            {
                if (!await _roleManager.RoleExistsAsync(roleType.ToString()))
                    await _roleManager.CreateAsync(new IdentityRole { Name = roleType.ToString() });
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.Email);
            if (user is null)
            {
                ModelState.AddModelError("Email", $"User not found by email: {forgotPasswordViewModel.Email}");
                return View();
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is blocked");
                return View();
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string? url = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, HttpContext.Request.Scheme);

            EmailHelper emailHelper = new();

            string body = await GetEmailTemplateAsync(url);

            MailRequestViewModel mailRequestViewModel = new()
            {
                ToEmail = user.Email,
                Subject = "Reset your password",
                Body = body
            };

            await emailHelper.SendEmailAsync(mailRequestViewModel);

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(resetPasswordViewModel.UserId) || string.IsNullOrWhiteSpace(resetPasswordViewModel.Token))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(resetPasswordViewModel.UserId);
            if (user is null)
                return NotFound();
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is blocked");
                return View();
            }

            ViewBag.UserName = user.UserName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ChangePasswordViewModel changePasswordViewModel, string? userId, string? token)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound();
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is blocked");
                return View();
            }

            var identityResult = await _userManager.ResetPasswordAsync(user, token, changePasswordViewModel.Password);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

            return RedirectToAction(nameof(Login));
        }

        private async Task<string> GetEmailTemplateAsync(string url)
        {
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "templates", "template.html");

            using StreamReader streamReader = new(path);
            string result = await streamReader.ReadToEndAsync();

            result = result.Replace("[reset_password_url]", url);
            streamReader.Close();
            return result;
        }
    }
}
