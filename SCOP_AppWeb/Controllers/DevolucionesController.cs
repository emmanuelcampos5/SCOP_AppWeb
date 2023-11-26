using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SCOP_AppWeb.Models;

namespace SCOP_AppWeb.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly AppDbContext _context;

        public DevolucionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Devoluciones        
        public async Task<IActionResult> Index(bool mostrarActivos = true, bool ordenarPorIdOrdenProduccion = false)
        {
            IEnumerable<Devoluciones> devoluciones = _context.Devoluciones;

            // Filtra por activos o cancelados
            devoluciones = devoluciones.Where(r => r.EstadoActivo == mostrarActivos);

            // Si es true se muestran las devoluciones activas, si es false las que han sido canceladas
            ViewBag.MostrarActivos = mostrarActivos;

            // Ordena la lista solo si se hace clic en el botón para ordenar
            if (ordenarPorIdOrdenProduccion)
            {
                devoluciones = devoluciones.OrderBy(d => d.IdOrdenProduccion);
                ViewBag.OrdenarPor = "Producción";
            }else
            {
                ViewBag.OrdenarPor = "Devolución";
            }

            return View(devoluciones);
        }


        //Permite mostrar las devoluciones que están activas o las que han sido canceladas.
        public IActionResult FiltrarDevoluciones(bool mostrarActivos)
        {
            return RedirectToAction("Index", new { mostrarActivos });
        }

        //GET: Requisiciones/Buscar
        public IActionResult Buscar()
        {
            return View("Buscar");
        }


        //Realiza la búsqueda de las requisiciones u órdenes de producción según su ID
        [HttpPost]
        public async Task<IActionResult> BuscarFiltradoAsync(string terminoBusqueda, string buscarPor)
        {
            //Inicializa las variables
            IEnumerable<Devoluciones> resultados = new List<Devoluciones>();
            string mensajeError = null;
            OrdenProduccion ordenProduccion = null;
            

            //Selecciona si la búsqueda es por órdenes de producción o requisiciones
            try
            {
                switch (buscarPor)
                {
                    case "Devolucion":
                        if (int.TryParse(terminoBusqueda, out int requisicionId))
                        {
                            resultados = _context.Devoluciones.Where(d => d.IdDevolucion== requisicionId);
                        }
                        if (!resultados.Any())
                        {
                            mensajeError = "No se encuentra una devolución con el ID " + terminoBusqueda + " en la base de datos.";
                        }
                        if (terminoBusqueda.IsNullOrEmpty())
                        {
                            mensajeError = "Ingrese un número de devolución válido.";
                        }
                        break;

                    case "OrdenProduccion":
                        if (int.TryParse(terminoBusqueda, out int ordenProduccionId))
                        {
                            ordenProduccion = await _context.OrdenProduccion
                                .FirstOrDefaultAsync(op => op.IdOrdenProduccion == ordenProduccionId);

                            //Si la orden existe busca el usuario y las requisiciones asociadas
                            if (ordenProduccion != null)
                            {

                                resultados = _context.Devoluciones
                                    .Where(d => d.IdOrdenProduccion == ordenProduccionId);
                            }

                            if (!resultados.Any())
                            {
                                mensajeError = "No se encuentra devoluciones asociadas con el ID " + terminoBusqueda + " en la base de datos.";
                            }

                            if (ordenProduccion == null)
                            {
                                mensajeError = "No se encuentra una orden de producción con el ID " + terminoBusqueda + " en la base de datos.";
                            }
                            else
                            {
                                // Se agrega la descripción de la orden para la vista                                
                                ViewBag.DescripcionOrdenProduccion = ordenProduccion.Descripcion;
                            }
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
            }

            if (!resultados.Any())
            {
                TempData["Mensaje"] = mensajeError;
                return RedirectToAction(nameof(Buscar));
            }

            return View("Buscar", resultados);
        }






        // GET: Devoluciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Devoluciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDevolucion,IdOrdenProduccion,IdUsuario,FechaCreacion,ImporteDevolucion,EstadoActivo")] Devoluciones devoluciones)
        {

            Usuarios usuarios = ObtenerUsuarioConectado();

            //Busca la ordende producción con el ID que se ingresa
            OrdenProduccion ordenProduccion = _context.OrdenProduccion.FirstOrDefault(o => o.IdOrdenProduccion == devoluciones.IdOrdenProduccion);
            if (ordenProduccion == null)
            {
                TempData["Mensaje"] = "No se encuentra una órden de producción con el ID " + devoluciones.IdOrdenProduccion;
                return RedirectToAction(nameof(Create));
            }

            //Verifica que el modelo está bien
            if (ModelState.IsValid)
            {
                //Agrega los datos default de las devoluciones
                devoluciones.IdUsuario = usuarios.idUsuario;
                devoluciones.FechaCreacion = DateTime.Now;
                devoluciones.EstadoActivo = true;
                //Guarda las devoluciones en la base de datos
                _context.Add(devoluciones);
                await _context.SaveChangesAsync();

                //Actualiza el estado de la orden de producción en la base de datos
                ordenProduccion.EstadoActivo = false;
                _context.Update(devoluciones);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(devoluciones);
        }


        // GET: Devoluciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Devoluciones == null)
            {
                return NotFound();
            }

            var devoluciones = await _context.Devoluciones.FindAsync(id);
            if (devoluciones == null)
            {
                return NotFound();
            }
            return View(devoluciones);
        }

        // POST: Devoluciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDevolucion,IdOrdenProduccion,IdUsuario,FechaCreacion,ImporteDevolucion,EstadoActivo")] Devoluciones devoluciones)
        {
            if (id != devoluciones.IdDevolucion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devoluciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DevolucionesExists(devoluciones.IdDevolucion))
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
            return View(devoluciones);
        }

        // GET: Devoluciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Devoluciones == null)
            {
                return NotFound();
            }

            var devoluciones = await _context.Devoluciones
                .FirstOrDefaultAsync(m => m.IdDevolucion == id);
            if (devoluciones == null)
            {
                return NotFound();
            }

            return View(devoluciones);
        }

        // POST: Devoluciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Devoluciones == null)
            {
                return Problem("Entity set 'AppDbContext.Devoluciones'  is null.");
            }
            var devoluciones = await _context.Devoluciones.FindAsync(id);
            if (devoluciones != null)
            {
                devoluciones.EstadoActivo = false;
                _context.Devoluciones.Update(devoluciones);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionesExists(int id)
        {
          return (_context.Devoluciones?.Any(e => e.IdDevolucion == id)).GetValueOrDefault();
        }


        public Usuarios ObtenerUsuarioConectado()
        {
            Usuarios user = _context.Usuarios.FirstOrDefault(u => u.correoUsuario == User.FindFirst(ClaimTypes.Name).Value);

            return user;
        }
    }
}
