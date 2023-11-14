using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SCOP_AppWeb.Models;
using System.Security.Claims;

namespace SCOP_AppWeb.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //------------------------------------------LOGIN---------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Usuarios usuario)
        {
            var temp = validarUsuario(usuario);

            if (temp != null)
            {
                //bool restablecer = false;

                //restablecer = await verificarRestablecer(temp);

                //if (restablecer)
                //{
                //    return RedirectToAction("Restablecer", "Cliente", new { Email = temp.Email });
                //}
                //else
                //{
                    var usuarioClaims = new List<Claim>() { new Claim(ClaimTypes.Name, temp.correoUsuario) };

                    var grandmaIdentity = new ClaimsIdentity(usuarioClaims, "User Identity");

                    var usuarioPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });

                    HttpContext.SignInAsync(usuarioPrincipal);

                    return RedirectToAction("Index", "Home");
                //}
            }
            TempData["Mensaje"] = "Email o password incorrecto";
            return View(usuario);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }

        //valida si el usuario se encuentra registrado en la base de datos
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
    }
}
