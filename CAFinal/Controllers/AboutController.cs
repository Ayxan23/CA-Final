namespace CAFinal.Controllers
{
    public class AboutController : Controller
    {
        private readonly AppDbContext _context;

        public AboutController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var abouts = await _context.Abouts.OrderByDescending(a => a.ModifiedAt).ToListAsync();

            return View(abouts);
        }
    }
}
