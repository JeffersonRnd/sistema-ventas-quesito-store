using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;
using System.Diagnostics;

namespace sistema_ventas_quesito_store.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        // Vitrina pública: cualquiera puede ver el catálogo sin iniciar sesión.
        public async Task<IActionResult> Index()
        {
            var productos = await _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0)
                .OrderByDescending(p => p.IdProducto)
                .ToListAsync();
            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
