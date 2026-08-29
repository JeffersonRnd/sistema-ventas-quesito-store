using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;
using sistema_ventas_quesito_store.ViewModels;

namespace sistema_ventas_quesito_store.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _db;
        public LoginController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Correo) || string.IsNullOrWhiteSpace(model.Contrasena))
            {
                ModelState.AddModelError("", "Ingresa tu correo y contraseña.");
                return View(model);
            }

            // Buscar solo por correo (insensible a mayúsculas está bien para el correo)
            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == model.Correo.ToLower().Trim());

            // Comparar contraseña en C# — case sensitive
            if (usuario == null || usuario.Contrasena != model.Contrasena.Trim())
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol!.NombreRol);

            return usuario.Rol!.NombreRol switch
            {
                "Administrador" => RedirectToAction("Administrador", "Welcome"),
                "Empaquetador" => RedirectToAction("Empaquetador", "Welcome"),
                "Repartidor" => RedirectToAction("Repartidor", "Welcome"),
                "Cliente" => RedirectToAction("Cliente", "Welcome"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            var vm = new RegistroViewModel { Roles = await _db.Roles.ToListAsync() };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            model.Roles = await _db.Roles.ToListAsync();
            if (!ModelState.IsValid) return View(model);

            if (await _db.Usuarios.AnyAsync(u => u.Correo == model.Correo))
            {
                ModelState.AddModelError("Correo", "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            _db.Usuarios.Add(new Usuario
            {
                NombreCompleto = model.NombreCompleto,
                DNI = model.DNI,
                Celular = model.Celular,
                Direccion = model.Direccion,
                Correo = model.Correo,
                Contrasena = model.Contrasena,
                IdRol = model.IdRol
            });
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Cuenta creada. Inicia sesión.";
            return RedirectToAction("Index");
        }

        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Index", "Home");
        }
    }
}