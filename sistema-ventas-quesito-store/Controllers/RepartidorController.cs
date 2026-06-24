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

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Repartidor")
                return RedirectToAction("Index", "Login");
            var entregas = await _db.Entregas
                .Include(e => e.Pedido).ThenInclude(p => p!.Cliente)
                .Include(e => e.Pedido).ThenInclude(p => p!.TipoEntrega)
                .Include(e => e.EstadosEntrega)
                .Where(e => e.Pedido!.EstadoPedido == "Empacado y listo" || e.Pedido.EstadoPedido == "En despacho")
                .ToListAsync();
            return View(entregas);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int idEntrega, string nuevoEstado, string? observacion)
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Repartidor")
                return RedirectToAction("Index", "Login");

            var entrega = await _db.Entregas.Include(e => e.Pedido).FirstOrDefaultAsync(e => e.IdEntrega == idEntrega);
            if (entrega == null) return NotFound();

            _db.EstadosEntrega.Add(new EstadoEntrega
            {
                IdEntrega = idEntrega,
                Estado = nuevoEstado,
                Observacion = observacion,
                FechaHora = DateTime.Now
            });

            // Actualizar estado del pedido según avance
            if (nuevoEstado == "Recogido en tienda")
                entrega.Pedido!.EstadoPedido = "En despacho";
            else if (nuevoEstado == "Entregado" || nuevoEstado == "Finalizado")
                entrega.Pedido!.EstadoPedido = "Finalizado";

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}