using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
              return _context.Requisiciones != null ? 
                          View(await _context.Requisiciones.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.Requisiciones'  is null.");
        }


        // GET: Requisiciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Requisiciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdenProduccion,IdUsuario,TipoRequisicion,CostoRequisicion")] Requisiciones requisiciones)
        {
            if (ModelState.IsValid)
            {
                // Asignar la fecha de creación automáticamente
                requisiciones.FechaCreacion = DateTime.Now;
                requisiciones.EstadoActivo = true;

                _context.Add(requisiciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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
        public async Task<IActionResult> Edit(int id, [Bind("IdRequisicion,IdOrdenProduccion,IdUsuario,FechaCreacion,TipoRequisicion,CostoRequisicion,EstadoActivo")] Requisiciones requisiciones)
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
                // Si la orden de producción está en estado "Espera", cambiar el estado de la requisición a "Inactivo",o "false"
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
    }
}
