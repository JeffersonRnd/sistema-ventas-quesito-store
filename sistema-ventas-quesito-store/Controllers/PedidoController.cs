using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Controllers
{
    public class PedidoController : Controller
    {
        private readonly AppDbContext _db;
        public PedidoController(AppDbContext db) => _db = db;

        private string? GetRol() => HttpContext.Session.GetString("UsuarioRol");
        private int GetUserId() => int.Parse(HttpContext.Session.GetString("UsuarioId") ?? "0");

        // ── CLIENTE: ver catálogo ────────────────────────────────
        public async Task<IActionResult> Catalogo()
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var productos = await _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0).ToListAsync();
            return View(productos);
        }

        // ── CLIENTE: agregar al carrito ──────────────────────────
        [HttpPost]
        public async Task<IActionResult> AgregarCarrito(int idProducto, int cantidad)
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var idUsuario = GetUserId();
            var carrito = await _db.Carritos.FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
            if (carrito == null)
            {
                carrito = new Carrito { IdUsuario = idUsuario };
                _db.Carritos.Add(carrito);
                await _db.SaveChangesAsync();
            }
            var detalle = await _db.CarritoDetalles
                .FirstOrDefaultAsync(d => d.IdCarrito == carrito.IdCarrito && d.IdProducto == idProducto);
            if (detalle == null)
                _db.CarritoDetalles.Add(new CarritoDetalle { IdCarrito = carrito.IdCarrito, IdProducto = idProducto, Cantidad = cantidad });
            else
                detalle.Cantidad += cantidad;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Catalogo));
        }

        // ── CLIENTE: ver carrito ─────────────────────────────────
        public async Task<IActionResult> Carrito()
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var idUsuario = GetUserId();
            var carrito = await _db.Carritos
                .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
            ViewBag.TiposEntrega = new SelectList(await _db.TiposEntrega.ToListAsync(), "IdTipoEntrega", "Nombre");
            return View(carrito);
        }

        // ── CLIENTE: confirmar pedido ────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(int idTipoEntrega, string? direccionDestino)
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var idUsuario = GetUserId();
            var carrito = await _db.Carritos
                .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
            if (carrito == null || !carrito.Detalles.Any()) return RedirectToAction(nameof(Carrito));

            var pedido = new Pedido
            {
                IdCliente = idUsuario,
                IdTipoEntrega = idTipoEntrega,
                Total = carrito.Detalles.Sum(d => d.Producto!.Precio * d.Cantidad)
            };
            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            foreach (var d in carrito.Detalles)
            {
                _db.DetallesPedido.Add(new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.Producto!.Precio
                });
                d.Producto.Stock -= d.Cantidad;
            }

            // Crear registro de entrega
            var entrega = new Entrega { IdPedido = pedido.IdPedido, DireccionDestino = direccionDestino };
            _db.Entregas.Add(entrega);

            // Limpiar carrito
            _db.CarritoDetalles.RemoveRange(carrito.Detalles);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(MisPedidos));
        }

        // ── CLIENTE: mis pedidos / seguimiento ──────────────────
        public async Task<IActionResult> MisPedidos()
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var pedidos = await _db.Pedidos
                .Include(p => p.TipoEntrega)
                .Include(p => p.Entrega).ThenInclude(e => e!.EstadosEntrega)
                .Where(p => p.IdCliente == GetUserId())
                .OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        // ── ADMIN: todos los pedidos ─────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (GetRol() != "Administrador") return RedirectToAction("Index", "Login");
            var pedidos = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.TipoEntrega)
                .OrderByDescending(p => p.FechaPedido).ToListAsync();
            return View(pedidos);
        }

        // ── ADMIN: aprobar pedido ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Aprobar(int id)
        {
            if (GetRol() != "Administrador") return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPedido = "Aprobado"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarcarPagado(int id)
        {
            if (GetRol() != "Administrador") return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPago = "Pagado"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (GetRol() != "Administrador") return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPedido = "Cancelado"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}