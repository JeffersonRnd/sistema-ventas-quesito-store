using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Controllers
{
    // Gestión de personal (Repartidores y Empaquetadores) por parte del Administrador.
    // No incluye a Administrador ni Cliente: esos roles no se gestionan desde aquí.
    public class PersonalController : Controller
    {
        private readonly AppDbContext _db;
        public PersonalController(AppDbContext db) => _db = db;

        private IActionResult? VerificarAdmin()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
                return RedirectToAction("Index", "Login");
            return null;
        }

        private async Task<SelectList> RolesPersonal(int? seleccionado = null)
        {
            var roles = await _db.Roles
                .Where(r => r.NombreRol == "Repartidor" || r.NombreRol == "Empaquetador")
                .ToListAsync();
            return new SelectList(roles, "IdRol", "NombreRol", seleccionado);
        }

        public async Task<IActionResult> Index()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var personal = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol!.NombreRol == "Repartidor" || u.Rol.NombreRol == "Empaquetador")
                .OrderBy(u => u.Rol!.NombreRol).ThenBy(u => u.NombreCompleto)
                .ToListAsync();
            return View(personal);
        }

        public async Task<IActionResult> Crear()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            ViewBag.Roles = await RolesPersonal();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuario u)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("Rol");
            ModelState.Remove("Pedidos");
            ModelState.Remove("Entregas");
            ModelState.Remove("Carrito");

            if (await _db.Usuarios.AnyAsync(x => x.Correo == u.Correo))
                ModelState.AddModelError("Correo", "Ya existe una cuenta con ese correo.");

            var rolValido = await _db.Roles.AnyAsync(r => r.IdRol == u.IdRol && (r.NombreRol == "Repartidor" || r.NombreRol == "Empaquetador"));
            if (!rolValido)
                ModelState.AddModelError("IdRol", "Selecciona un rol de personal válido.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await RolesPersonal(u.IdRol);
                return View(u);
            }

            _db.Usuarios.Add(u);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            ViewBag.Roles = await RolesPersonal(u.IdRol);
            return View(u);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Usuario u)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("Rol");
            ModelState.Remove("Pedidos");
            ModelState.Remove("Entregas");
            ModelState.Remove("Carrito");
            ModelState.Remove("Contrasena");

            var rolValido = await _db.Roles.AnyAsync(r => r.IdRol == u.IdRol && (r.NombreRol == "Repartidor" || r.NombreRol == "Empaquetador"));
            if (!rolValido)
                ModelState.AddModelError("IdRol", "Selecciona un rol de personal válido.");

            if (await _db.Usuarios.AnyAsync(x => x.Correo == u.Correo && x.IdUsuario != u.IdUsuario))
                ModelState.AddModelError("Correo", "Ya existe otra cuenta con ese correo.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await RolesPersonal(u.IdRol);
                return View(u);
            }

            var existente = await _db.Usuarios.FindAsync(u.IdUsuario);
            if (existente == null) return NotFound();

            existente.NombreCompleto = u.NombreCompleto;
            existente.DNI = u.DNI;
            existente.Celular = u.Celular;
            existente.Direccion = u.Direccion;
            existente.Correo = u.Correo;
            existente.IdRol = u.IdRol;
            if (!string.IsNullOrWhiteSpace(u.Contrasena))
                existente.Contrasena = u.Contrasena; // solo se cambia si se escribió una nueva

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
