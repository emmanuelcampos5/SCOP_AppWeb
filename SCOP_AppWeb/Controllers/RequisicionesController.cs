using System;
using System.Collections.Generic;
using System.IO.Pipelines;
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
    public class RequisicionesController : Controller
    {
        private readonly AppDbContext _context;

        public RequisicionesController(AppDbContext context)
        {
            _context = context;
        }
        // GET: Devoluciones        
        public async Task<IActionResult> Index(bool mostrarActivos = true, bool ordenarPorIdOrdenProduccion = false)
        {
            IEnumerable<Requisiciones> requisiciones = _context.Requisiciones;

            // Filtra por activos o cancelados
            requisiciones = requisiciones.Where(r => r.EstadoActivo == mostrarActivos);

            // Si es true se muestran las devoluciones activas, si es false las que han sido canceladas
            ViewBag.MostrarActivos = mostrarActivos;

            // Ordena la lista solo si se hace clic en el botón para ordenar
            if (ordenarPorIdOrdenProduccion)
            {
                requisiciones = requisiciones.OrderBy(d => d.IdOrdenProduccion);
                ViewBag.OrdenarPor = "Producción";
            }
            else
            {
                ViewBag.OrdenarPor = "Requisición";
            }

            return View(requisiciones);
        }

        //Permite mostrar las requisiciones que están activas o las que han sido canceladas.
        public IActionResult FiltrarRequisiciones(bool mostrarActivos)
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
            IEnumerable<Requisiciones> resultados = new List<Requisiciones>();
            string mensajeError = null;
            OrdenProduccion ordenProduccion = null;
            Usuarios usuario = null;

            //Selecciona si la búsqueda es por órdenes de producción o requisiciones
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

                            //Si la orden existe busca el usuario y las requisiciones asociadas
                            if (ordenProduccion != null)
                            {
                                usuario = await _context.Usuarios
                                    .FirstOrDefaultAsync(u => u.idUsuario == ordenProduccion.IdUsuario);

                                resultados = _context.Requisiciones
                                    .Where(r => r.IdOrdenProduccion == ordenProduccionId);
                            }

                            if (!resultados.Any())
                            {
                                mensajeError = "No se encuentra requisiciones asociadas con el ID " + terminoBusqueda + " en la base de datos.";
                            }

                            if (ordenProduccion == null)
                            {
                                mensajeError = "No se encuentra una orden de producción con el ID " + terminoBusqueda + " en la base de datos.";
                            }
                            else
                            {
                                //Se agrega la información de los costos parciales (sin impuestos) de la orden de producción
                                ViewBag.CostoTotal = await CalcularCostoRequisicionesPorOrdenProduccion(ordenProduccionId);

                                // Se agrega la información del usuario y la descripción de la orden para la vista
                                ViewBag.NombreUsuarioOrdenProduccion = usuario.nombreUsuario;
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





        // GET: Requisiciones/Create
        public IActionResult Create()
        {
            Requisiciones requisicion = new Requisiciones();

            if (User.Identity.IsAuthenticated)
            {
                Usuarios usuarioConectado = ObtenerUsuarioConectado();
                ViewBag.Usuarios = usuarioConectado.nombreUsuario;
            }

            return View(requisicion);
        }


        // POST: Requisiciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdenProduccion,IdUsuario,TipoRequisicion,CostoRequisicion")] Requisiciones requisiciones)
        {
            OrdenProduccion ordenProduccion = _context.OrdenProduccion.FirstOrDefault(o => o.IdOrdenProduccion == requisiciones.IdOrdenProduccion);
            if (ordenProduccion == null)
            {
                TempData["Mensaje"] = "No se encuentra una órden de producción con el ID " + requisiciones.IdOrdenProduccion;
                return RedirectToAction(nameof(Create));
            }

            if (ModelState.IsValid)
            {
                Usuarios user = null;

                if (User.Identity.IsAuthenticated)
                {
                    // Obtener el nombre del usuario logeado
                    var correoUsuario = User.Identity.Name;
                    // Buscar el ID del usuario en base al correo de usuario
                    user = await _context.Usuarios.SingleOrDefaultAsync(u => u.correoUsuario == correoUsuario);
                }
                else {
                    user = await _context.Usuarios.FirstOrDefaultAsync(u => u.idUsuario == requisiciones.IdUsuario);
                }                   
                

                if (user != null)
                {
                    // Asignar la fecha de creación y el estado automáticamente
                    requisiciones.FechaCreacion = DateTime.Now;
                    requisiciones.EstadoActivo = true;

                    // Asignar el ID del usuario logeado a la requisición
                    requisiciones.IdUsuario = user.idUsuario;

                    _context.Add(requisiciones);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Manejar el caso donde el usuario no se encuentra en la base de datos
                    ModelState.AddModelError(string.Empty, "El usuario no se encuentra en la base de datos.");
                }
            }

            // Si pasa algo, volver a cargar la lista de órdenes de producción y usuarios
            ViewBag.OrdenesProduccion = _context.OrdenProduccion.ToList();
            ViewBag.Usuarios = _context.Usuarios.ToList();

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

                        //Auditoria de edición
                        await Auditoria(requisiciones, "editar");
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

            if (User.Identity.IsAuthenticated)
            {
                // Verificar si la orden de producción está en estado "Espera"
                if (ordenProduccion != null && ordenProduccion.EstadoProduccion == "Espera")
                {
                    // Si la orden de producción está en estado "Espera", cambiar el estado de la requisición a "false"
                    requisiciones.EstadoActivo = false; // 
                    _context.Update(requisiciones);
                    await _context.SaveChangesAsync();
                    //Auditoria de eliminación
                    await Auditoria(requisiciones, "eliminar");
                }
                else
                {
                    // La orden de producción no está en estado "Espera", no se permite la cancelación                
                    TempData["Mensaje"] = "No se puede cancelar la requisición porque la orden de producción no está en estado 'Espera'.";

                    return View(requisiciones);
                }
            }
            else {
                TempData["Mensaje"] = "Inicie sesión para realizar esta acción";
                return RedirectToAction(nameof(Delete));
            }

            return RedirectToAction(nameof(Index));
        }



        private bool RequisicionesExists(int id)
        {
          return (_context.Requisiciones?.Any(e => e.IdRequisicion == id)).GetValueOrDefault();
        }




        public Usuarios ObtenerUsuarioConectado() {
            Usuarios user = _context.Usuarios.FirstOrDefault(u => u.correoUsuario == User.FindFirst(ClaimTypes.Name).Value);
            return user;
        }

        
        public async Task<float> CalcularCostoRequisicionesPorOrdenProduccion(int idOrdenProduccion)
        {
            // Buscar todas las requisiciones asociadas al ID de orden de producción
            var requisiciones = await _context.Requisiciones
                .Where(r => r.IdOrdenProduccion == idOrdenProduccion && r.EstadoActivo == true)
                .ToListAsync();

            // Sumar los costos de las requisiciones encontradas
            float costoTotal = (float)requisiciones.Sum(r => r.CostoRequisicion);

            return costoTotal;
        }

        public async Task Auditoria(Requisiciones requisicion, string tipoAccion)
        {
            // Obtener el nombre del usuario logeado
            var correoUsuario = User.Identity.Name;

            // Buscar el ID del usuario en base al nombre de usuario
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.correoUsuario == correoUsuario);

            if (user != null)
            {
                RegistroAuditoria registroAuditoria = new RegistroAuditoria
                {
                    TablaModificada = "Requisiciones",
                    IdUsuarioModificacion = user.idUsuario,
                    FechaModificacion = DateTime.Now
                };

                switch (tipoAccion)
                {
                    case "eliminar":
                        registroAuditoria.Descripcion = "Se eliminó la requisición con el ID " + requisicion.IdRequisicion;
                        break;

                    case "editar":
                        registroAuditoria.Descripcion = "Se editó la requisición con el ID " + requisicion.IdRequisicion;
                        break;

                    default:
                        throw new ArgumentException("Acción no identificada para la auditoría.");
                }

                _context.RegistroAuditoria.Add(registroAuditoria);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Si el usuario no se encuentra                
                throw new InvalidOperationException("No se pudo encontrar el usuario para la auditoría.");
            }
        }



    }
}
