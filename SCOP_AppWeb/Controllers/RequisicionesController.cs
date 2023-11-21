using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SCOP_AppWeb.Models;

namespace SCOP_AppWeb.Controllers
{
    public class RequisicionesController : Controller
    {
        private readonly AppDbContext _context;

        public RequisicionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Requisiciones
        public async Task<IActionResult> Index(bool mostrarInactivos = false)
        {
            IEnumerable<Requisiciones> requisiciones = _context.Requisiciones;

            if (!mostrarInactivos)
            {
                requisiciones = requisiciones.Where(r => r.EstadoActivo);
            }

            ViewBag.MostrarInactivos = mostrarInactivos;

            return View(requisiciones);
        }

        //Permite mostrar las requisiciones que están canceladas o inactivas.
        public IActionResult BuscarInactivos(bool mostrarInactivos)
        {
            return RedirectToAction("Index", new { mostrarInactivos });
        }

        //GET: Requisiciones/Buscar
        public IActionResult Buscar()
        {
            return View("Buscar");
        }
        [HttpPost]
        public async Task<IActionResult> BuscarFiltradoAsync(string terminoBusqueda, string buscarPor)
        {
            IEnumerable<Requisiciones> resultados = new List<Requisiciones>();
            string mensajeError = null;
            OrdenProduccion ordenProduccion = null;
            Usuarios usuario = null;

            try
            {
                switch (buscarPor)
                {
                    case "Requisicion":
                        if (int.TryParse(terminoBusqueda, out int requisicionId))
                        {
                            resultados = _context.Requisiciones.Where(r => r.IdRequisicion == requisicionId);
                        }
                        if (!resultados.Any())
                        {
                            mensajeError = "No se encuentra una requisión con el ID " + terminoBusqueda + " en la base de datos.";
                        }
                        if(terminoBusqueda.IsNullOrEmpty())
                        {
                            mensajeError = "Ingrese un número de requisición válido.";
                        }
                        break;

                    case "OrdenProduccion":
                        if (int.TryParse(terminoBusqueda, out int ordenProduccionId))
                        {
                            ordenProduccion = await _context.OrdenProduccion
                                .FirstOrDefaultAsync(op => op.IdOrdenProduccion == ordenProduccionId);
                            usuario = await _context.Usuarios
                                .FirstOrDefaultAsync(u => u.idUsuario == ordenProduccion.IdUsuario);

                            resultados = _context.Requisiciones
                                .Where(r => r.IdOrdenProduccion == ordenProduccionId);

                            if (!resultados.Any())
                            {
                                mensajeError = "No se encuentra requisiciones asociadas con el ID " + terminoBusqueda + " en la base de datos.";
                            }

                            if (ordenProduccion == null)
                            {
                                mensajeError = "No se encuentra una orden de producción con el ID " + terminoBusqueda + " en la base de datos.";
                            }

                            ViewBag.CostoTotal = await CalcularCostoRequisicionesPorOrdenProduccion(ordenProduccionId);

                            // Se agrega la información del usuario y la descripción de la orden para la vista
                            ViewBag.NombreUsuarioOrdenProduccion = usuario.nombreUsuario;
                            ViewBag.DescripcionOrdenProduccion = ordenProduccion.Descripcion;
                        }
                        else
                        {
                            mensajeError = "Ingrese un ID de Orden de Producción válido.";
                        }
                        break;


                    default:
                        mensajeError = "Seleccione un criterio de búsqueda válido.";
                        break;
                }
            }
            catch (Exception ex)
            {
                mensajeError = "Ocurrió un error durante la búsqueda.";
                // Loguear el error.
                // 
            }

            if (!resultados.Any())
            {
                TempData["Mensaje"] = mensajeError;
                return RedirectToAction(nameof(Buscar));
            }

            return View("Buscar", resultados);
        }





        // GET: Requisiciones/Create
        public IActionResult Create()
        {
            ViewBag.Usuarios = _context.Usuarios.ToList();
            ViewBag.OrdenesProduccion = _context.OrdenProduccion.ToList();
            return View();
        }

        //POST: Requisiciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdenProduccion,IdUsuario,TipoRequisicion,CostoRequisicion")] Requisiciones requisiciones)
        {
            if (ModelState.IsValid)
            {
                // Asignar la fecha de creación y el estado automáticamente 
                requisiciones.FechaCreacion = DateTime.Now;
                requisiciones.EstadoActivo = true;

                _context.Add(requisiciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si pasa algo, volver a cargar la lista de usuarios y órdenes de producción
            ViewBag.Usuarios = _context.Usuarios.ToList();
            ViewBag.OrdenesProduccion = _context.OrdenProduccion.ToList();

            return View(requisiciones);
        }



        // GET: Requisiciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Requisiciones == null)
            {
                return NotFound();
            }

            var requisiciones = await _context.Requisiciones.FindAsync(id);
            if (requisiciones == null)
            {
                return NotFound();
            }
            return View(requisiciones);
        }

        // POST: Requisiciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRequisicion,IdOrdenProduccion,IdUsuario,FechaCreacion,TipoRequisicion,CostoRequisicion")] Requisiciones requisiciones)
        {
            if (id != requisiciones.IdRequisicion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener la orden de producción asociada a la requisición
                    var ordenProduccion = await _context.OrdenProduccion.FindAsync(requisiciones.IdOrdenProduccion);

                    // Verificar si la orden de producción está en estado "Espera"
                    if (ordenProduccion != null && ordenProduccion.EstadoProduccion == "Espera")
                    {
                        //Hay un bug que hace que se cambie a false cuando se edita
                        requisiciones.EstadoActivo = true;
                        // La orden de producción está en estado "Espera", se puede modificar la requisición
                        _context.Update(requisiciones);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // La orden de producción no está en estado "Espera", no se permite la modificación
                        TempData["Mensaje"] = "No se puede modificar la requisición porque la orden de producción no está en estado 'Espera'.";
                        return View(requisiciones);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequisicionesExists(requisiciones.IdRequisicion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(requisiciones);
        }


        // GET: Requisiciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Requisiciones == null)
            {
                return NotFound();
            }

            var requisiciones = await _context.Requisiciones
                .FirstOrDefaultAsync(m => m.IdRequisicion == id);
            if (requisiciones == null)
            {
                return NotFound();
            }

            return View(requisiciones);
        }

        // POST: Requisiciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var requisiciones = await _context.Requisiciones.FindAsync(id);

            if (requisiciones == null)
            {
                return NotFound();
            }

            // Obtener la orden de producción asociada a la requisición
            var ordenProduccion = await _context.OrdenProduccion.FindAsync(requisiciones.IdOrdenProduccion);

            // Verificar si la orden de producción está en estado "Espera"
            if (ordenProduccion != null && ordenProduccion.EstadoProduccion == "Espera")
            {
                // Si la orden de producción está en estado "Espera", cambiar el estado de la requisición a "false"
                requisiciones.EstadoActivo = false; // 
                _context.Update(requisiciones);
                await _context.SaveChangesAsync();
            }
            else
            {
                // La orden de producción no está en estado "Espera", no se permite la cancelación                
                TempData["Mensaje"] = "No se puede cancelar la requisición porque la orden de producción no está en estado 'Espera'.";

                return View(requisiciones);
            }

            return RedirectToAction(nameof(Index));
        }



        private bool RequisicionesExists(int id)
        {
          return (_context.Requisiciones?.Any(e => e.IdRequisicion == id)).GetValueOrDefault();
        }






        public async Task<float> CalcularCostoRequisicionesPorOrdenProduccion(int idOrdenProduccion)
        {
            // Buscar todas las requisiciones asociadas al ID de orden de producción
            var requisiciones = await _context.Requisiciones
                .Where(r => r.IdOrdenProduccion == idOrdenProduccion)
                .ToListAsync();

            // Sumar los costos de las requisiciones encontradas
            float costoTotal = (float)requisiciones.Sum(r => r.CostoRequisicion);

            return costoTotal;
        }

    }
}
