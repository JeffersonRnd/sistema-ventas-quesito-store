using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Controllers
{
    public class ProductoController : Controller
    {
        private readonly AppDbContext _db;
        public ProductoController(AppDbContext db) => _db = db;

        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador") return RedirectToAction("Index", "Login");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var productos = await _db.Productos.Include(p => p.Categoria).ToListAsync();
            return View(productos);
        }

        public async Task<IActionResult> Crear()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Producto p)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
                return View(p);
            }
            _db.Productos.Add(p);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var p = await _db.Productos.FindAsync(id);
            if (p == null) return NotFound();
            ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria", p.IdCategoria);
            return View(p);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Producto p)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
                return View(p);
            }
            _db.Productos.Update(p);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p != null) { p.Activo = !p.Activo; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}