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

        public LoginController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == model.Correo
                                       && u.Contrasena == model.Contrasena);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol!.NombreRol);

            return usuario.Rol!.NombreRol switch
            {
                "Administrador" => RedirectToAction("Administrador", "Welcome"),
                "Empleado"      => RedirectToAction("Empleado",      "Welcome"),
                "Repartidor"    => RedirectToAction("Repartidor",    "Welcome"),
                "Cliente"       => RedirectToAction("Cliente",       "Welcome"),
                _               => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            var vm = new RegistroViewModel
            {
                Roles = await _db.Roles.ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            model.Roles = await _db.Roles.ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            bool existe = await _db.Usuarios.AnyAsync(u => u.Correo == model.Correo);
            if (existe)
            {
                ModelState.AddModelError("Correo", "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            var nuevo = new Usuario
            {
                NombreCompleto = model.NombreCompleto,
                DNI            = model.DNI,
                Celular        = model.Celular,
                Direccion      = model.Direccion,
                Correo         = model.Correo,
                Contrasena     = model.Contrasena,
                IdRol          = model.IdRol
            };

            _db.Usuarios.Add(nuevo);
            await _db.SaveChangesAsync();

            TempData["Mensaje"] = "Cuenta creada. Inicia sesión.";
            return RedirectToAction("Index");
        }

        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
