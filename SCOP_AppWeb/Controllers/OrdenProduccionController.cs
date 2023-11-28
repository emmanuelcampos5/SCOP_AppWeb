
using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        public string ObtenerEstadoProduccionOrden(int id)
        {
            string estado;

            var orden = _context.OrdenProduccion.AsNoTracking().FirstOrDefault(o => o.IdOrdenProduccion == id);
            estado = orden.EstadoProduccion;

            return estado;
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarOrdenProduccion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarOrdenProduccion([Bind("IdOrdenProduccion, IdUsuario, FechaRecepcion, EstadoProduccion, CantidadProductos, Descripcion, EstadoActivo")] OrdenProduccion orden)
        {            
            try
            {
                if (orden != null)
                {
                    if (orden.CantidadProductos == 0 || orden.Descripcion == "")
                    {
                        TempData["MensajeError"] = "Asegurese de llenar todos los campos";
                        return View(orden);
                    }
                    else
                    {
                        orden.EstadoProduccion = "Espera";
                        orden.FechaRecepcion = DateTime.Now;
                        orden.IdUsuario = ObtenerUsuarioConectado().idUsuario;
                        orden.EstadoActivo = true;

                        _context.Add(orden);

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                } 
            }catch(Exception ex)
            {
                TempData["MensajeError"] = "Error al agregar la orden de producción" + ex;
            }          
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> Delete(int id)
        {

            var temp = _context.OrdenProduccion.Find(id);

            try
            {
                if (temp.EstadoProduccion == "Espera")
                {   
                    temp.EstadoActivo = false;
                    _context.OrdenProduccion.Update(temp);
                    await _context.SaveChangesAsync();

                    RegistroAuditoria auditoria = new RegistroAuditoria();

                    auditoria.TablaModificada = "OrdenProduccion";
                    auditoria.FechaModificacion = DateTime.Now;
                    auditoria.IdUsuarioModificacion = ObtenerUsuarioConectado().idUsuario;
                    auditoria.Descripcion = "Se eliminó la orden de producción con el ID " + temp.IdOrdenProduccion;

                    _context.RegistroAuditoria.Add(auditoria);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensajeError"] = "No se pueden eliminar oredenes en estado de producción o finalizado";
                    return View(temp);
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar la orden" + ex.Message;
            }
                   
            return RedirectToAction("Index");
        }   
        

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var temp = await _context.OrdenProduccion.FindAsync(id);

            if(temp == null)
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
        public async Task<IActionResult> Edit(int id, [Bind("IdOrdenProduccion, IdUsuario, FechaRecepcion, EstadoProduccion, CantidadProductos, Descripcion, EstadoActivo")] OrdenProduccion orden)
        {
            if(id != orden.IdOrdenProduccion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {                    
                    string estado = ObtenerEstadoProduccionOrden(id);

                    if (estado == "Espera")
                    {
                        _context.OrdenProduccion.Update(orden);
                        await _context.SaveChangesAsync();

                        RegistroAuditoria auditoria = new RegistroAuditoria();

                        auditoria.TablaModificada = "OrdenProduccion";
                        auditoria.FechaModificacion = DateTime.Now;
                        auditoria.IdUsuarioModificacion = ObtenerUsuarioConectado().idUsuario;
                        auditoria.Descripcion = "Se editó la orden de producción con el ID " + orden.IdOrdenProduccion;

                        _context.RegistroAuditoria.Update(auditoria);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["MensajeError"] = "No se pueden editar oredenes en estado de producción o finalizado";
                        return View(orden);
                    }
                }
                catch (Exception ex)
                {
                    TempData["MensajeError"] = "Error al editar la orden de produccion" + ex.Message;
                }
                return RedirectToAction(nameof(Index));
            }            
            return View(orden);                        
        }

        [HttpGet]
        public IActionResult Finalizar(int? id)
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
        public async Task<IActionResult>Finalizar(int id)
        {
            var temp = _context.OrdenProduccion.Find(id);

            try
            {
                if (temp.EstadoProduccion == "Producción")
                {
                    temp.EstadoProduccion = "Finalizado";
                    _context.OrdenProduccion.Update(temp);
                    await _context.SaveChangesAsync();

                    RegistroAuditoria auditoria = new RegistroAuditoria();

                    auditoria.TablaModificada = "OrdenProduccion";
                    auditoria.FechaModificacion = DateTime.Now;
                    auditoria.IdUsuarioModificacion = ObtenerUsuarioConectado().idUsuario;
                    auditoria.Descripcion = "Se finalizó la orden de producción con el ID " + temp.IdOrdenProduccion;

                    _context.RegistroAuditoria.Add(auditoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensajeError"] = "Únicamente se puede finalizar órdenes que esten en estado de producción";
                    return View(temp);
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al finalizar la orden" + ex.Message;
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Produccion(int? id)
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
        public async Task<IActionResult> Produccion(int id)
        {
            var temp = _context.OrdenProduccion.Find(id);

            try
            {
                if (temp.EstadoProduccion == "Espera")
                {
                    temp.EstadoProduccion = "Producción";
                    _context.OrdenProduccion.Update(temp);
                    await _context.SaveChangesAsync();

                    RegistroAuditoria auditoria = new RegistroAuditoria();

                    auditoria.TablaModificada = "OrdenProduccion";
                    auditoria.FechaModificacion = DateTime.Now;
                    auditoria.IdUsuarioModificacion = ObtenerUsuarioConectado().idUsuario;
                    auditoria.Descripcion = "Se pasó a estado de producción la orden con el ID " + temp.IdOrdenProduccion;

                    _context.RegistroAuditoria.Add(auditoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["MensajeError"] = "Únicamente se puede pasar a producción las órdenes que esten en estado de espera";
                    return View(temp);
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al pasar la orden a producción" + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
