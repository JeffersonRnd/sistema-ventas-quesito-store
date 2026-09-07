using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;

namespace sistema_ventas_quesito_store.Controllers
{
    public class EmpaquetadorController : Controller
    {
        private readonly AppDbContext _db;
        public EmpaquetadorController(AppDbContext db) => _db = db;

        private bool EsEmpaquetador() => HttpContext.Session.GetString("UsuarioRol") == "Empaquetador";

        public async Task<IActionResult> Index()
        {
            if (!EsEmpaquetador()) return RedirectToAction("Index", "Login");
            var pedidos = await _db.Pedidos
                .Include(p => p.Cliente).Include(p => p.TipoEntrega)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Where(p => p.EstadoPedido == "Aprobado" || p.EstadoPedido == "Empacando" || p.EstadoPedido == "Empacado y listo")
                .OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        public async Task<IActionResult> Historial()
        {
            if (!EsEmpaquetador()) return RedirectToAction("Index", "Login");
            var pedidos = await _db.Pedidos
                .Include(p => p.Cliente).Include(p => p.TipoEntrega)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Where(p => p.EstadoPedido == "En despacho" || p.EstadoPedido == "Finalizado" || p.EstadoPedido == "Recogido")
                .OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarEmpacando(int id)
        {
            if (!EsEmpaquetador()) return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null && p.EstadoPedido == "Aprobado") { p.EstadoPedido = "Empacando"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarcarListo(int id)
        {
            if (!EsEmpaquetador()) return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null && p.EstadoPedido == "Empacando") { p.EstadoPedido = "Empacado y listo"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarcarRecogido(int id)
        {
            if (!EsEmpaquetador()) return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.Include(x => x.TipoEntrega).FirstOrDefaultAsync(x => x.IdPedido == id);
            if (p != null && p.EstadoPedido == "Empacado y listo" && p.TipoEntrega!.Nombre == "Recojo en tienda")
            {
                p.EstadoPedido = "Recogido";
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}