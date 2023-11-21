using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;
using System.Security.Claims;

namespace SCOP_AppWeb.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly AppDbContext _context;

        public DevolucionesController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.Devoluciones.ToList());
        }


        [HttpGet]
        public IActionResult RegistrarDevoluciones()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarDevoluciones(Devoluciones devolucion)
        {
            if (devolucion != null)
            {
                if (ValidarIdOrdenProduccion(devolucion.IdOrdenProduccion))
                {
                    devolucion.FechaCreacion = DateTime.Now;
                    devolucion.IdUsuario = ObtenerUsuarioConectado().idUsuario;
                    devolucion.EstadoActivo = true;

                    try
                    {
                        _context.Devoluciones.Add(devolucion);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = "No se registró la devolucion";
                    }
                    return RedirectToAction("Index" , "Devoluciones");
                }
                else
                {
                    TempData["Mensaje"] = "La orden de produccion no existe";
                    return View(devolucion);                    
                }
            }
            else
            {
                return View();
            }
        }

        public Usuarios ObtenerUsuarioConectado()
        {
            Usuarios user = _context.Usuarios.FirstOrDefault(u => u.correoUsuario == User.FindFirst(ClaimTypes.Name).Value);

            return user;
        }

        public bool ValidarIdOrdenProduccion(int idOrdenProduccion)
        {
            foreach (var temp in _context.OrdenProduccion.ToList())
            {
                if (temp.IdOrdenProduccion == idOrdenProduccion)
                {
                    return true;
                }               
            }
            return false;
        }
    }
}
