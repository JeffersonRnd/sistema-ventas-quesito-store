using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Data;

namespace sistema_ventas_quesito_store.Controllers
{
    public class WelcomeController : Controller
    {
        private readonly AppDbContext _db;
        public WelcomeController(AppDbContext db) => _db = db;

        private IActionResult? VerificarSesion(string rolRequerido)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (string.IsNullOrEmpty(rol) || rol != rolRequerido)
                return RedirectToAction("Index", "Login");
            return null;
        }

        public async Task<IActionResult> Administrador()
        {
            var check = VerificarSesion("Administrador");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            // Badge: pedidos esperando aprobación del admin
            ViewBag.PedidosPorAprobar = await _db.Pedidos.CountAsync(p => p.EstadoPedido == "Pendiente");
            return View();
        }

        public async Task<IActionResult> Empaquetador()
        {
            var check = VerificarSesion("Empaquetador");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            // Badge: pedidos listos para empacar
            ViewBag.PedidosPorEmpacar = await _db.Pedidos.CountAsync(p =>
                p.EstadoPedido == "Aprobado" || p.EstadoPedido == "Empacando");
            return View();
        }

        public async Task<IActionResult> Repartidor()
        {
            var check = VerificarSesion("Repartidor");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            // Badge: entregas disponibles/pendientes para el repartidor
            ViewBag.EntregasPendientes = await _db.Entregas
                .Include(e => e.Pedido).ThenInclude(p => p!.TipoEntrega)
                .CountAsync(e => (e.Pedido!.EstadoPedido == "Empacado y listo" || e.Pedido.EstadoPedido == "En despacho")
                              && e.Pedido.TipoEntrega!.Nombre != "Recojo en tienda");
            return View();
        }

        public async Task<IActionResult> Cliente()
        {
            var check = VerificarSesion("Cliente");
            if (check != null) return check;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            // Catálogo directo en el panel del cliente (misma consulta que Pedido/Catalogo)
            var productos = await _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0).ToListAsync();
            return View(productos);
        }
    }
}
