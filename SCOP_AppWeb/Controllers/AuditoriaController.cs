using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;

namespace SCOP_AppWeb.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.RegistroAuditoria.ToList());
        }
    }
}
