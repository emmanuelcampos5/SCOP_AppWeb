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
            //return View(_context.OrdenProduccion.ToList());
            return View(ObtenerOrdenesProduccionActivas());
        }

        public List<OrdenProduccion> ObtenerOrdenesProduccionActivas()
        {
            return _context.OrdenProduccion
                .Where(d => d.EstadoActivo == true)
                .ToList();
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
        public IActionResult RegistrarOrdenProduccion([Bind("IdOrdenProduccion, IdUsuario, FechaRecepcion, EstadoProduccion, CantidadProductos, Descripcion, EstadoActivo")] OrdenProduccion orden)
        {
            if (orden != null)
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

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            var temp = _context.OrdenProduccion.Find(id);

            if (temp == null)
            {
                return NotFound();
            }
            else
            {
                return View(temp);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {

            var temp = _context.OrdenProduccion.Find(id);

            try
            {
                if (temp.EstadoProduccion == "Espera")
                {   
                    temp.EstadoActivo = false;
                    _context.OrdenProduccion.Update(temp);
                    _context.SaveChanges();

                    RegistroAuditoria auditoria = new RegistroAuditoria();

                    auditoria.TablaModificada = "OrdenProduccion";
                    auditoria.FechaModificacion = DateTime.Now;
                    auditoria.IdUsuarioModificacion = ObtenerUsuarioConectado().idUsuario;
                    auditoria.Descripcion = "Se eliminó la orden de producción con el ID " + temp.IdOrdenProduccion;

                    _context.RegistroAuditoria.Add(auditoria);
                    _context.SaveChanges();
                }
                else
                {
                    TempData["MensajeError"] = "No se pueden eliminar oredenes en estado de producción o finalizado";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar la orden";
            }
                   
            return RedirectToAction("Index");

        }   
    }
}
