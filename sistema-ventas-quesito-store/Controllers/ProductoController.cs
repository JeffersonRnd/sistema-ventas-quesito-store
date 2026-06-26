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
        private readonly IWebHostEnvironment _env;

        public ProductoController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private IActionResult? VerificarAdmin()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Administrador")
                return RedirectToAction("Index", "Login");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            return View(await _db.Productos.Include(p => p.Categoria).ToListAsync());
        }

        public async Task<IActionResult> Crear()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Producto p, IFormFile? imagen)
        {
            // Quitar ImagenUrl del ModelState para no bloquear
            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Categoria");
            ModelState.Remove("DetallesPedido");
            ModelState.Remove("CarritoDetalles");

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
                return View(p);
            }

            if (imagen != null && imagen.Length > 0)
                p.ImagenUrl = await GuardarImagen(imagen);

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
        public async Task<IActionResult> Editar(Producto p, IFormFile? imagen)
        {
            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Categoria");
            ModelState.Remove("DetallesPedido");
            ModelState.Remove("CarritoDetalles");

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria");
                return View(p);
            }

            var existente = await _db.Productos.FindAsync(p.IdProducto);
            if (existente == null) return NotFound();

            existente.Nombre = p.Nombre;
            existente.Descripcion = p.Descripcion;
            existente.Talla = p.Talla;
            existente.Color = p.Color;
            existente.Precio = p.Precio;
            existente.Stock = p.Stock;
            existente.IdCategoria = p.IdCategoria;
            existente.Activo = p.Activo;

            if (imagen != null && imagen.Length > 0)
                existente.ImagenUrl = await GuardarImagen(imagen);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var p = await _db.Productos.FindAsync(id);
            if (p != null)
            {
                // Eliminar imagen del disco si existe
                if (!string.IsNullOrEmpty(p.ImagenUrl))
                {
                    var ruta = Path.Combine(_env.WebRootPath, p.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
                }
                _db.Productos.Remove(p);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p != null) { p.Activo = !p.Activo; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GuardarImagen(IFormFile imagen)
        {
            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "productos");
            Directory.CreateDirectory(carpeta);
            var nombre = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var ruta = Path.Combine(carpeta, nombre);
            using var stream = new FileStream(ruta, FileMode.Create);
            await imagen.CopyToAsync(stream);
            return $"/uploads/productos/{nombre}";
        }
    }
}