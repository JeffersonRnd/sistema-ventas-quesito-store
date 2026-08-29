using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;
using System.Text.Json;

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
            ViewBag.Categorias = new SelectList(await _db.Categorias.Where(c => c.Activo).ToListAsync(), "IdCategoria", "NombreCategoria");
            ViewBag.TallasPorCategoriaJson = await ObtenerTallasPorCategoriaJson();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Producto p, IFormFile? imagen)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Categoria");
            ModelState.Remove("DetallesPedido");
            ModelState.Remove("CarritoDetalles");
            ModelState.Remove("Stock"); // el stock se calcula en servidor a partir de las tallas, no se confía en el valor enviado por el cliente

            if (imagen == null || imagen.Length == 0)
                ModelState.AddModelError("", "Debes subir una imagen de referencia del producto.");

            var errorTallas = await ValidarTallasDeCategoria(p.IdCategoria, p.Talla);
            if (errorTallas != null) ModelState.AddModelError("Talla", errorTallas);

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.Where(c => c.Activo).ToListAsync(), "IdCategoria", "NombreCategoria", p.IdCategoria);
                ViewBag.TallasPorCategoriaJson = await ObtenerTallasPorCategoriaJson();
                return View(p);
            }

            // El stock general SIEMPRE es la suma de las tallas: nunca se acepta un valor manual desincronizado.
            p.Stock = TallaHelper.Parse(p.Talla).Sum(t => t.Stock);

            p.ImagenUrl = await GuardarImagen(imagen!);
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
            ViewBag.TallasPorCategoriaJson = await ObtenerTallasPorCategoriaJson();
            return View(p);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Producto p, IFormFile? imagen)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Categoria");
            ModelState.Remove("DetallesPedido");
            ModelState.Remove("CarritoDetalles");
            ModelState.Remove("Stock");

            var errorTallas = await ValidarTallasDeCategoria(p.IdCategoria, p.Talla);
            if (errorTallas != null) ModelState.AddModelError("Talla", errorTallas);

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await _db.Categorias.ToListAsync(), "IdCategoria", "NombreCategoria", p.IdCategoria);
                ViewBag.TallasPorCategoriaJson = await ObtenerTallasPorCategoriaJson();
                return View(p);
            }

            var existente = await _db.Productos.FindAsync(p.IdProducto);
            if (existente == null) return NotFound();

            existente.Nombre = p.Nombre;
            existente.Descripcion = p.Descripcion;
            existente.Talla = p.Talla;
            existente.Color = p.Color;
            existente.Precio = p.Precio;
            existente.Stock = TallaHelper.Parse(p.Talla).Sum(t => t.Stock); // recalculado siempre en servidor
            existente.IdCategoria = p.IdCategoria;
            existente.Activo = p.Activo;

            if (imagen != null && imagen.Length > 0)
                existente.ImagenUrl = await GuardarImagen(imagen);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Construye { "1": ["Única","S — 54–56 cm", ...], "2": ["36","37",...], ... } por IdCategoria
        // para que la vista pinte los checkboxes de talla dinámicamente, sin nada hardcodeado en JS.
        private async Task<string> ObtenerTallasPorCategoriaJson()
        {
            var datos = await _db.CategoriaTallas
                .Include(ct => ct.Talla)
                .ToListAsync();

            var mapa = datos
                .GroupBy(ct => ct.IdCategoria)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.OrderBy(ct => ct.Talla!.Orden).Select(ct => ct.Talla!.Nombre).ToList());

            return JsonSerializer.Serialize(mapa);
        }

        // Verifica que las tallas enviadas realmente pertenezcan a la categoría elegida
        // (evita que, vía manipulación del form, se guarden tallas de otra categoría).
        private async Task<string?> ValidarTallasDeCategoria(int idCategoria, string? tallaSerializada)
        {
            var enviadas = TallaHelper.Parse(tallaSerializada);
            if (enviadas.Count == 0) return "Selecciona al menos una talla disponible.";

            var validas = await _db.CategoriaTallas
                .Where(ct => ct.IdCategoria == idCategoria)
                .Include(ct => ct.Talla)
                .Select(ct => ct.Talla!.Nombre)
                .ToListAsync();

            var invalidas = enviadas.Select(t => t.Nombre).Except(validas).ToList();
            if (invalidas.Count > 0)
                return $"Las siguientes tallas no pertenecen a la categoría seleccionada: {string.Join(", ", invalidas)}";

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var p = await _db.Productos.FindAsync(id);
            if (p != null)
            {
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
            var check = VerificarAdmin(); if (check != null) return check;
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