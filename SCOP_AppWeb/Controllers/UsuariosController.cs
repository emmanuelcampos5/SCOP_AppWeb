using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SCOP_AppWeb.Models;
using System.Security.Claims;
using System.Text;

namespace SCOP_AppWeb.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private static string EmailRestablecer = "";

        public UsuariosController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }     

        //------------------------------------------LOGIN---------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Usuarios usuario)
        {
            var temp = validarUsuario(usuario);

            if (temp != null)
            {
                bool restablecer = false;

                restablecer = verificarRestablecer(temp);

                if (restablecer)
                {
                    return RedirectToAction("Restablecer", "Usuarios", new { tempCorreo = temp.correoUsuario });
                }
                else
                {
                    var usuarioClaims = new List<Claim>() { new Claim(ClaimTypes.Name, temp.correoUsuario) };

                    var grandmaIdentity = new ClaimsIdentity(usuarioClaims, "User Identity");

                    var usuarioPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });

                    HttpContext.SignInAsync(usuarioPrincipal);

                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["Mensaje"] = "Email o password incorrecto";
            return View(usuario);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }

        //
        private Usuarios validarUsuario(Usuarios temp)
        {
            Usuarios autorizado = null;

            var listaUsuarios = _context.Usuarios.ToList();

            var user = listaUsuarios.FirstOrDefault(u => u.correoUsuario == temp.correoUsuario);

            if (user != null)
            {
                if (user.password.Equals(temp.password))
                {
                    autorizado = user;
                }
            }
            return autorizado;
        }

        //-------------------------------------------INDEX----------------------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.Usuarios.ToList());
        }

        //---------------------------------------CREAR USUARIO------------------------------------------------
        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarUsuario(Usuarios usuario, string listaRoles)
        {
            if (usuario != null)
            {
                var temp = _context.Usuarios.FirstOrDefault(t => t.correoUsuario == usuario.correoUsuario);
                
                if(temp == null)
                {
                    var rol = _context.Roles.FirstOrDefault(r => r.nombreRol == listaRoles);

                    if(rol != null)
                    {
                        usuario.idRol = rol.idRol;
                        usuario.estadoActivo = true;
                        usuario.password = this.GenerarClave();
                        usuario.restablecer = false;

                        try
                        {
                            _context.Usuarios.Add(usuario);
                            _context.SaveChanges();

                            if (this.EnviarEmailRegistro(usuario))
                            {
                                TempData["MensajeCreado"] = "Usuario creado correctamente, Su contraseña fue enviada por email";
                            }
                            else
                            {
                                TempData["MensajeCreado"] = "Usuario creado pero no se envio el email, comuniquese con el administrador;";
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["MensajeError"] = "No se logro crear la cuenta.." + ex.Message;
                        }
                        return View();
                    }
                    else
                    {
                        TempData["MensajeError"] = "Seleccione un rol valido";
                        return View(usuario);
                    }
                }
                else
                {
                    TempData["MensajeError"] = "Ya existe un usuario con este correo";
                    return View(usuario);
                }                           
            }
            else
            {
                return View();
            }
        }

        private string GenerarClave()
        {
            Random random = new Random();
            string clave = string.Empty;
            clave = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(clave, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool EnviarEmailRegistro(Usuarios temp)
        {
            try
            {
                bool enviado = false;
                EmailRegistro email = new EmailRegistro();
                email.Enviar(temp);
                enviado = true;

                return enviado;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //----------------------------------------RESTABLECER-------------------------------------------------
        [HttpGet]
        public IActionResult Restablecer(string? tempCorreo)
        {           
            var usuario = _context.Usuarios.First(usuario => usuario.correoUsuario.Equals(tempCorreo));

            SeguridadRestablecer restablecer = new SeguridadRestablecer();

            restablecer.correoUsuario = usuario.correoUsuario;
            restablecer.password = usuario.password;

            EmailRestablecer = usuario.correoUsuario;

            return View(restablecer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restablecer(SeguridadRestablecer pRestablecer)
        {
            if (pRestablecer != null)
            {             
                var usuario = _context.Usuarios.First(c => c.correoUsuario == EmailRestablecer);

                if (usuario.password.Equals(pRestablecer.password))
                {
                    if (pRestablecer.nuevoPassword.Equals(pRestablecer.confirmar))
                    {
                        usuario.nombreUsuario = pRestablecer.nombreUsuario;
                        usuario.telefonoUsuario = pRestablecer.telefonoUsuario;
                        usuario.password = pRestablecer.confirmar;
                        usuario.restablecer = true;

                        _context.Usuarios.Update(usuario);
                        _context.SaveChanges();

                        return RedirectToAction("Login", "Usuarios");
                    }
                    else
                    {
                        TempData["MensajeError"] = "Las contraseñas no coinciden";
                        return View(pRestablecer);
                    }
                }
                else
                {
                    TempData["MensajeError"] = "La contraseña es incorrecta";
                    return View(pRestablecer);
                }
            }
            else
            {
                TempData["MensajeError"] = "Datos incorrectos";
                return View(pRestablecer);
            }

        }

        private bool verificarRestablecer(Usuarios temp)
        {
            bool verificado = false;

            var user = _context.Usuarios.FirstOrDefault(u => u.correoUsuario == temp.correoUsuario);

            if (user != null)
            {
                if (user.restablecer == false)
                {
                    verificado = true;
                }
            }
            return verificado;
        }
    }
}
