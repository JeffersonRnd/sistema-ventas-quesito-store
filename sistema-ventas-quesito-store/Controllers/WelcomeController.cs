using Microsoft.AspNetCore.Mvc;

namespace sistema_ventas_quesito_store.Controllers
{
    public class WelcomeController : Controller
    {
        private IActionResult? VerificarSesion(string rolRequerido)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (string.IsNullOrEmpty(rol) || rol != rolRequerido)
                return RedirectToAction("Index", "Login");
            return null;
        }

        public IActionResult Administrador()
        {
            var check = VerificarSesion("Administrador");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }

        public IActionResult Empleado()
        {
            var check = VerificarSesion("Empleado");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }

        public IActionResult Repartidor()
        {
            var check = VerificarSesion("Repartidor");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }

        public IActionResult Cliente()
        {
            var check = VerificarSesion("Cliente");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }
    }
}
