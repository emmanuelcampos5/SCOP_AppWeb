using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;

namespace SCOP_AppWeb.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly AppDbContext _context;

        public DevolucionesController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public IActionResult Index()
        {
            return View(_context.Devoluciones.ToList());
        }
    }
}
