using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SCOP_AppWeb.Controllers
{
    public class OrdenProduccionController : Controller
    {
        
        private readonly AppDbContext _context;

        public OrdenProduccionController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public IActionResult Index()
        {           
            return View(_context.OrdenProduccion.ToList());
        }


        //---------------------------------------------------------------------------------------------------------
        public List<Devoluciones> ObtenerDevolucionesPorOrdenProduccion(int idOrdenProduccion)
        {
            return _context.Devoluciones
                .Where(d => d.IdOrdenProduccion == idOrdenProduccion)
                .ToList();
        }

        public List<Requisiciones> ObtenerRequisicionesPorOrdenProduccion(int idOrdenProduccion)
        {
            return _context.Requisiciones
                .Where(r => r.IdOrdenProduccion == idOrdenProduccion)
                .ToList();
        }

        //Devuelve el costo total de una orden de produccion en especifico que entra como parametro
        public double CalcularCostoTotalPorOrdenProduccion(int idOrdenProduccion)
        {
            List<Requisiciones> requisiciones = ObtenerRequisicionesPorOrdenProduccion(idOrdenProduccion);

            List<Devoluciones> devoluciones = ObtenerDevolucionesPorOrdenProduccion(idOrdenProduccion);

            double costoTotal = requisiciones.Sum(r => r.CostoRequisicion);

            costoTotal -= devoluciones.Sum(d => d.ImporteDevolucion);

            return costoTotal;
        }


        public Usuarios ObtenerUsuarioConectado()
        {
            Usuarios user = _context.Usuarios.FirstOrDefault(u => u.correoUsuario == User.FindFirst(ClaimTypes.Name).Value);

            return user;
        }


        [HttpGet]
        public IActionResult RegistrarOrdenProduccion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarOrdenProduccion(OrdenProduccion orden)
        {
            if(orden != null)
            {

                orden.IdUsuario = ObtenerUsuarioConectado().idUsuario;
                orden.EstadoActivo = true;

                
                _context.Add(orden);

                _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(orden);
            }
        }
        
    }
}
