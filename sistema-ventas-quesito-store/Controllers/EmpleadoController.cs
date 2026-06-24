using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;

namespace sistema_ventas_quesito_store.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly AppDbContext _db;
        public EmpleadoController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Empleado")
                return RedirectToAction("Index", "Login");
            var pedidos = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.TipoEntrega)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Where(p => p.EstadoPedido == "Aprobado" || p.EstadoPedido == "Empacando")
                .OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarEmpacando(int id)
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Empleado")
                return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPedido = "Empacando"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarcarListo(int id)
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Empleado")
                return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPedido = "Empacado y listo"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}