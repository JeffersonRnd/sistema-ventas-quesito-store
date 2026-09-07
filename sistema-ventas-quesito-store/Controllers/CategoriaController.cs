using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _db;

        public CategoriaController(AppDbContext db)
        {
            _db = db;
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
            var categorias = await _db.Categorias
                .Include(c => c.CategoriaTallas).ThenInclude(ct => ct.Talla)
                .Include(c => c.Productos)
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();
            return View(categorias);
        }

        public async Task<IActionResult> Crear()
        {
            var check = VerificarAdmin(); if (check != null) return check;
            ViewBag.TallasExistentes = await _db.Tallas.OrderBy(t => t.Orden).ToListAsync();
            return View(new Categoria());
        }

        // tallasSeleccionadas: IDs de tallas ya existentes en el catálogo que se marcaron
        // tallasNuevas: texto libre separado por comas con tallas nuevas a crear (ej: "S, M, L")
        [HttpPost]
        public async Task<IActionResult> Crear(Categoria c, int[]? tallasSeleccionadas, string? tallasNuevas)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("Productos");
            ModelState.Remove("CategoriaTallas");

            if (!ModelState.IsValid)
            {
                ViewBag.TallasExistentes = await _db.Tallas.OrderBy(t => t.Orden).ToListAsync();
                return View(c);
            }

            _db.Categorias.Add(c);
            await _db.SaveChangesAsync(); // necesitamos el IdCategoria generado

            await VincularTallas(c.IdCategoria, tallasSeleccionadas, tallasNuevas);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var c = await _db.Categorias
                .Include(x => x.CategoriaTallas)
                .FirstOrDefaultAsync(x => x.IdCategoria == id);
            if (c == null) return NotFound();

            ViewBag.TallasExistentes = await _db.Tallas.OrderBy(t => t.Orden).ToListAsync();
            ViewBag.TallasSeleccionadasIds = c.CategoriaTallas.Select(ct => ct.IdTalla).ToHashSet();
            return View(c);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria c, int[]? tallasSeleccionadas, string? tallasNuevas)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            ModelState.Remove("Productos");
            ModelState.Remove("CategoriaTallas");

            if (!ModelState.IsValid)
            {
                ViewBag.TallasExistentes = await _db.Tallas.OrderBy(t => t.Orden).ToListAsync();
                ViewBag.TallasSeleccionadasIds = (tallasSeleccionadas ?? Array.Empty<int>()).ToHashSet();
                return View(c);
            }

            var existente = await _db.Categorias
                .Include(x => x.CategoriaTallas)
                .FirstOrDefaultAsync(x => x.IdCategoria == c.IdCategoria);
            if (existente == null) return NotFound();

            existente.NombreCategoria = c.NombreCategoria;
            existente.Descripcion = c.Descripcion;
            existente.Activo = c.Activo;

            // Reemplaza el vínculo de tallas por el nuevo set seleccionado/creado
            _db.CategoriaTallas.RemoveRange(existente.CategoriaTallas);
            await VincularTallas(existente.IdCategoria, tallasSeleccionadas, tallasNuevas);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;

            var tieneProductos = await _db.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar: hay productos asociados a esta categoría. Desactívala o reasigna esos productos primero.";
                return RedirectToAction(nameof(Index));
            }

            var c = await _db.Categorias.Include(x => x.CategoriaTallas).FirstOrDefaultAsync(x => x.IdCategoria == id);
            if (c != null)
            {
                _db.CategoriaTallas.RemoveRange(c.CategoriaTallas);
                _db.Categorias.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            var check = VerificarAdmin(); if (check != null) return check;
            var c = await _db.Categorias.FindAsync(id);
            if (c != null) { c.Activo = !c.Activo; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // Crea (si hace falta) las tallas nuevas escritas a mano y vincula todo a la categoría.
        private async Task VincularTallas(int idCategoria, int[]? tallasSeleccionadas, string? tallasNuevas)
        {
            var idsTallas = new HashSet<int>(tallasSeleccionadas ?? Array.Empty<int>());

            if (!string.IsNullOrWhiteSpace(tallasNuevas))
            {
                var nombres = tallasNuevas.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Distinct(StringComparer.OrdinalIgnoreCase);
                var maxOrden = (await _db.Tallas.MaxAsync(t => (int?)t.Orden)) ?? 0;

                foreach (var nombre in nombres)
                {
                    var talla = await _db.Tallas.FirstOrDefaultAsync(t => t.Nombre == nombre);
                    if (talla == null)
                    {
                        talla = new Talla { Nombre = nombre, Orden = ++maxOrden };
                        _db.Tallas.Add(talla);
                        await _db.SaveChangesAsync(); // para obtener IdTalla
                    }
                    idsTallas.Add(talla.IdTalla);
                }
            }

            foreach (var idTalla in idsTallas)
                _db.CategoriaTallas.Add(new CategoriaTalla { IdCategoria = idCategoria, IdTalla = idTalla });
        }
    }
}
