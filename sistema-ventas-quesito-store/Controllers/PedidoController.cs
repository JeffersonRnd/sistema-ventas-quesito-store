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
        public async Task<IActionResult> AgregarCarrito(int idProducto, int cantidad, string? talla)
        {
            if (GetRol() != "Cliente") return Json(new { ok = false, mensaje = "Sesión inválida." });
            var producto = await _db.Productos.FindAsync(idProducto);
            if (producto == null) return Json(new { ok = false, mensaje = "Producto no encontrado." });

            // Si el producto maneja tallas, la talla es obligatoria y su stock propio manda
            var tallas = TallaHelper.Parse(producto.Talla);
            int stockDisponible = producto.Stock;
            talla = string.IsNullOrWhiteSpace(talla) ? null : talla.Trim();

            if (tallas.Count > 0)
            {
                if (talla == null)
                    return Json(new { ok = false, mensaje = "Selecciona una talla antes de agregar al carrito." });

                var tallaInfo = tallas.FirstOrDefault(t => t.Nombre == talla);
                if (tallaInfo.Nombre == null)
                    return Json(new { ok = false, mensaje = "La talla seleccionada no está disponible para este producto." });

                stockDisponible = tallaInfo.Stock;
            }

            var idUsuario = GetUserId();
            var carrito = await _db.Carritos.FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
            if (carrito == null)
            {
                carrito = new Carrito { IdUsuario = idUsuario };
                _db.Carritos.Add(carrito);
                await _db.SaveChangesAsync();
            }
            var detalle = await _db.CarritoDetalles
                .FirstOrDefaultAsync(d => d.IdCarrito == carrito.IdCarrito && d.IdProducto == idProducto && d.TallaSeleccionada == talla);
            int cantidadActual = detalle?.Cantidad ?? 0;

            if (cantidad < 1) return Json(new { ok = false, mensaje = "La cantidad debe ser al menos 1." });
            if (cantidadActual + cantidad > stockDisponible)
            {
                int disponible = stockDisponible - cantidadActual;
                return Json(new
                {
                    ok = false,
                    mensaje = disponible <= 0
                    ? "Ya tienes en el carrito todo el stock disponible de este producto/talla."
                    : $"Solo puedes agregar {disponible} unidad(es) más. Stock disponible: {stockDisponible}."
                });
            }

            if (detalle == null)
                _db.CarritoDetalles.Add(new CarritoDetalle { IdCarrito = carrito.IdCarrito, IdProducto = idProducto, Cantidad = cantidad, TallaSeleccionada = talla });
            else
                detalle.Cantidad += cantidad;
            await _db.SaveChangesAsync();

            int restante = stockDisponible - (cantidadActual + cantidad);
            return Json(new { ok = true, mensaje = "Se agregó correctamente al carrito.", restante });
        }

        // ── CLIENTE: actualizar cantidad de un producto en el carrito ──
        [HttpPost]
        public async Task<IActionResult> ActualizarCantidadCarrito(int idDetalle, int cantidad)
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var idUsuario = GetUserId();
            var detalle = await _db.CarritoDetalles.Include(d => d.Carrito).Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.IdCarritoDetalle == idDetalle && d.Carrito!.IdUsuario == idUsuario);
            if (detalle != null)
            {
                int stockDisponible = detalle.Producto!.Stock;
                if (!string.IsNullOrEmpty(detalle.TallaSeleccionada))
                {
                    var tallaInfo = TallaHelper.Parse(detalle.Producto.Talla).FirstOrDefault(t => t.Nombre == detalle.TallaSeleccionada);
                    stockDisponible = tallaInfo.Nombre != null ? tallaInfo.Stock : 0;
                }

                if (cantidad <= 0)
                {
                    _db.CarritoDetalles.Remove(detalle);
                }
                else if (cantidad > stockDisponible)
                {
                    TempData["ErrorPedido"] = $"Solo hay {stockDisponible} unidad(es) en stock de {detalle.Producto.Nombre}" +
                        (detalle.TallaSeleccionada != null ? $" (talla {detalle.TallaSeleccionada})." : ".");
                }
                else
                {
                    detalle.Cantidad = cantidad;
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Carrito));
        }

        // ── CLIENTE: quitar un producto del carrito ──────────────
        [HttpPost]
        public async Task<IActionResult> EliminarDelCarrito(int idDetalle)
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            var idUsuario = GetUserId();
            var detalle = await _db.CarritoDetalles.Include(d => d.Carrito)
                .FirstOrDefaultAsync(d => d.IdCarritoDetalle == idDetalle && d.Carrito!.IdUsuario == idUsuario);
            if (detalle != null)
            {
                _db.CarritoDetalles.Remove(detalle);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Carrito));
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

        // ── CLIENTE: confirmar pedido (el pago con tarjeta se simula aquí mismo) ──
        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(int idTipoEntrega, string? direccionDestino,
            string? titularTarjeta, string? numeroTarjeta, string? vencimientoTarjeta, string? cvvTarjeta)
        {
            if (GetRol() != "Cliente") return RedirectToAction("Index", "Login");
            if (idTipoEntrega <= 0)
            {
                TempData["ErrorPedido"] = "Por favor seleccione el tipo de entrega.";
                return RedirectToAction(nameof(Carrito));
            }
            if (idTipoEntrega > 1 && string.IsNullOrWhiteSpace(direccionDestino))
            {
                TempData["ErrorPedido"] = "Por favor ingrese la dirección de entrega.";
                return RedirectToAction(nameof(Carrito));
            }

            // ── Validación de la tarjeta (simulada) ──
            // Solo se valida el formato; el número completo y el CVV nunca se guardan.
            var numeroLimpio = (numeroTarjeta ?? "").Replace(" ", "").Trim();
            if (string.IsNullOrWhiteSpace(titularTarjeta) ||
                numeroLimpio.Length < 13 || numeroLimpio.Length > 19 || !numeroLimpio.All(char.IsDigit) ||
                string.IsNullOrWhiteSpace(vencimientoTarjeta) ||
                string.IsNullOrWhiteSpace(cvvTarjeta) || cvvTarjeta.Trim().Length < 3 || cvvTarjeta.Trim().Length > 4 || !cvvTarjeta.Trim().All(char.IsDigit))
            {
                TempData["ErrorPedido"] = "Revisa los datos de tu tarjeta: hay campos incompletos o inválidos.";
                return RedirectToAction(nameof(Carrito));
            }

            var idUsuario = GetUserId();
            var carrito = await _db.Carritos
                .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
            if (carrito == null || !carrito.Detalles.Any()) return RedirectToAction(nameof(Carrito));

            var total = carrito.Detalles.Sum(d => d.Producto!.Precio * d.Cantidad);

            var pedido = new Pedido
            {
                IdCliente = idUsuario,
                IdTipoEntrega = idTipoEntrega,
                Total = total,
                EstadoPago = "Pagado" // el pago simulado ya se validó arriba
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
                    TallaSeleccionada = d.TallaSeleccionada,
                    PrecioUnitario = d.Producto!.Precio
                });
                d.Producto.Stock -= d.Cantidad;

                // Descuenta también el stock de la talla específica
                if (!string.IsNullOrEmpty(d.TallaSeleccionada))
                {
                    var tallas = TallaHelper.Parse(d.Producto.Talla);
                    for (int i = 0; i < tallas.Count; i++)
                    {
                        if (tallas[i].Nombre == d.TallaSeleccionada)
                        {
                            tallas[i] = (tallas[i].Nombre, Math.Max(0, tallas[i].Stock - d.Cantidad));
                            break;
                        }
                    }
                    d.Producto.Talla = TallaHelper.Serializar(tallas);
                }
            }

            // Crear registro de entrega
            var entrega = new Entrega { IdPedido = pedido.IdPedido, DireccionDestino = direccionDestino };
            _db.Entregas.Add(entrega);

            // Registrar el pago simulado (nunca se guarda el número completo ni el CVV)
            var marca = numeroLimpio.StartsWith("4") ? "Visa"
                : (numeroLimpio.StartsWith("5") ? "Mastercard" : "Tarjeta");
            _db.Pagos.Add(new Pago
            {
                IdPedido = pedido.IdPedido,
                MetodoPago = "Tarjeta",
                TitularTarjeta = titularTarjeta!.Trim(),
                TarjetaMarca = marca,
                TarjetaUltimos4 = numeroLimpio[^4..],
                Monto = total,
                EstadoPago = "Aprobado"
            });

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
                .Include(p => p.Pago)
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
                .Include(p => p.Pago)
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
        public async Task<IActionResult> Cancelar(int id)
        {
            if (GetRol() != "Administrador") return RedirectToAction("Index", "Login");
            var p = await _db.Pedidos.FindAsync(id);
            if (p != null) { p.EstadoPedido = "Cancelado"; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}