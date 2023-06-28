using CAFinal.Areas.Admin.ViewModels;

namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.OrderByDescending(s => s.Id).ToListAsync();

            return View(settings);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(Setting setting)
        {
            if (!ModelState.IsValid)
                return View();

            if (await _context.Settings.AnyAsync(c => c.Key == setting.Key))
            {
                ModelState.AddModelError("Key", "Key already exist");
                return View();
            }

            await _context.Settings.AddAsync(setting);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (setting is null)
                return NotFound();

            return View(setting);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (setting is null)
                return NotFound();

            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, Setting setting)
        {
            if (!ModelState.IsValid)
                return View();

            if (await _context.Settings.AnyAsync(c => c.Key == setting.Key))
            {
                ModelState.AddModelError("Key", "Key already exist");
                return View();
            }

            var foundSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (foundSetting is null)
                return NotFound();

            foundSetting.Value = setting.Value;
            foundSetting.Key = setting.Key;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (setting is null)
                return NotFound();

            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var foundSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (foundSetting is null)
                return NotFound();

            _context.Settings.Remove(foundSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
