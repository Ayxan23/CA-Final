namespace CAFinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public ContactController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _count = _context.Contacts.AsEnumerable().Count();
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts.OrderByDescending(c => c.ModifiedAt).ToListAsync();

            ViewBag.Count = _count;

            return View(contacts);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            if (_count == 3)
                return BadRequest();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(ContactViewModel contactViewModel)
        {
            if (_count == 3)
                return BadRequest();

            if (!ModelState.IsValid)
                return View();

            Contact contact = new()
            {
                Title = contactViewModel.Title,
                Description = contactViewModel.Description,
                Icon = contactViewModel.Icon,
                IsDeleted = false
            };

            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return NotFound();

            return View(contact);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return NotFound();

            ContactViewModel contactViewModel = new()
            {
                Id = contact.Id,
                Title = contact.Title,
                Description = contact.Description,
                Icon = contact.Icon,
            };

            return View(contactViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, ContactViewModel contactViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return NotFound();

            contact.Title = contactViewModel.Title;
            contact.Description = contactViewModel.Description;
            contact.Icon = contactViewModel.Icon;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return NotFound();

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return NotFound();

            contact.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
