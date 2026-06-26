using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Controllers
{
    public class RepartidorController : Controller
    {
        private readonly AppDbContext _db;
        public RepartidorController(AppDbContext db) => _db = db;
        private bool EsRepartidor() => HttpContext.Session.GetString("UsuarioRol") == "Repartidor";

        public async Task<IActionResult> Index()
        {
            if (!EsRepartidor()) return RedirectToAction("Index", "Login");
            // Solo pedidos con envío (domicilio o ciudad), no recojo en tienda
            var entregas = await _db.Entregas
                .Include(e => e.Pedido).ThenInclude(p => p!.Cliente)
                .Include(e => e.Pedido).ThenInclude(p => p!.TipoEntrega)
                .Include(e => e.EstadosEntrega)
                .Where(e => (e.Pedido!.EstadoPedido == "Empacado y listo" || e.Pedido.EstadoPedido == "En despacho")
                         && e.Pedido.TipoEntrega!.Nombre != "Recojo en tienda")
                .ToListAsync();
            return View(entregas);
        }

        public async Task<IActionResult> Historial()
        {
            if (!EsRepartidor()) return RedirectToAction("Index", "Login");
            var entregas = await _db.Entregas
                .Include(e => e.Pedido).ThenInclude(p => p!.Cliente)
                .Include(e => e.Pedido).ThenInclude(p => p!.TipoEntrega)
                .Include(e => e.EstadosEntrega)
                .Where(e => e.Pedido!.EstadoPedido == "Finalizado"
                         && e.Pedido.TipoEntrega!.Nombre != "Recojo en tienda")
                .ToListAsync();
            return View(entregas);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int idEntrega, string nuevoEstado, string? observacion)
        {
            if (!EsRepartidor()) return RedirectToAction("Index", "Login");
            var entrega = await _db.Entregas
                .Include(e => e.EstadosEntrega)
                .Include(e => e.Pedido)
                .FirstOrDefaultAsync(e => e.IdEntrega == idEntrega);
            if (entrega == null) return NotFound();

            // No permitir registrar un estado ya registrado
            var yaExiste = entrega.EstadosEntrega.Any(s => s.Estado == nuevoEstado);
            if (!yaExiste)
            {
                _db.EstadosEntrega.Add(new EstadoEntrega
                {
                    IdEntrega = idEntrega,
                    Estado = nuevoEstado,
                    Observacion = observacion,
                    FechaHora = DateTime.Now
                });
                if (nuevoEstado == "Recogido en tienda")
                    entrega.Pedido!.EstadoPedido = "En despacho";
                else if (nuevoEstado == "Entregado" || nuevoEstado == "Finalizado")
                    entrega.Pedido!.EstadoPedido = "Finalizado";
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
