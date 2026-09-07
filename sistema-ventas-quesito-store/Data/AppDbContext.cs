using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        //****************tablas************************************
        public DbSet<Rol>            Roles            { get; set; }
        public DbSet<Usuario>        Usuarios         { get; set; }
        public DbSet<Categoria>      Categorias       { get; set; }
        public DbSet<Talla>          Tallas           { get; set; }
        public DbSet<CategoriaTalla> CategoriaTallas  { get; set; }
        public DbSet<Producto>       Productos        { get; set; }
        public DbSet<TipoEntrega>    TiposEntrega     { get; set; }
        public DbSet<Pedido>         Pedidos          { get; set; }
        public DbSet<DetallePedido>  DetallesPedido   { get; set; }
        public DbSet<Entrega>        Entregas         { get; set; }
        public DbSet<EstadoEntrega>  EstadosEntrega   { get; set; }   
        public DbSet<Carrito>        Carritos         { get; set; }
        public DbSet<CarritoDetalle> CarritoDetalles  { get; set; }
        public DbSet<Pago>           Pagos            { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //*************************Relaciones************************

            // Usuario → Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol);

            // CategoriaTalla (N:N Categoria <-> Talla) con clave compuesta
            modelBuilder.Entity<CategoriaTalla>()
                .HasKey(ct => new { ct.IdCategoria, ct.IdTalla });

            modelBuilder.Entity<CategoriaTalla>()
                .HasOne(ct => ct.Categoria)
                .WithMany(c => c.CategoriaTallas)
                .HasForeignKey(ct => ct.IdCategoria)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoriaTalla>()
                .HasOne(ct => ct.Talla)
                .WithMany(t => t.CategoriaTallas)
                .HasForeignKey(ct => ct.IdTalla)
                .OnDelete(DeleteBehavior.Cascade);

            // Evita productos "huérfanos" de categoría: si la categoría tiene productos, no se puede borrar
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido → Cliente (Usuario)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido → TipoEntrega
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.TipoEntrega)
                .WithMany(t => t.Pedidos)
                .HasForeignKey(p => p.IdTipoEntrega);

            // DetallePedido → Pedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdPedido);

            // DetallePedido → Producto
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesPedido)
                .HasForeignKey(d => d.IdProducto);

            // Entrega → Pedido (1 a 1)
            modelBuilder.Entity<Entrega>()
                .HasOne(e => e.Pedido)
                .WithOne(p => p.Entrega)
                .HasForeignKey<Entrega>(e => e.IdPedido);

            // Pago → Pedido (1 a 1)
            modelBuilder.Entity<Pago>()
                .HasOne(pg => pg.Pedido)
                .WithOne(p => p.Pago)
                .HasForeignKey<Pago>(pg => pg.IdPedido);

            // Entrega → Repartidor (puede ser null si es recojo en tienda)
            modelBuilder.Entity<Entrega>()
                .HasOne(e => e.Repartidor)
                .WithMany(u => u.Entregas)
                .HasForeignKey(e => e.IdRepartidor)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // EstadoEntrega → Entrega
            modelBuilder.Entity<EstadoEntrega>()
                .HasOne(s => s.Entrega)
                .WithMany(e => e.EstadosEntrega)
                .HasForeignKey(s => s.IdEntrega);

            // Carrito → Usuario (1 a 1)
            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Carrito)
                .HasForeignKey<Carrito>(c => c.IdUsuario);

            // CarritoDetalle → Carrito
            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(cd => cd.Carrito)
                .WithMany(c => c.Detalles)
                .HasForeignKey(cd => cd.IdCarrito);

            // CarritoDetalle → Producto
            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(cd => cd.Producto)
                .WithMany(p => p.CarritoDetalles)
                .HasForeignKey(cd => cd.IdProducto);

            //************Seed Data

            // Roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NombreRol = "Administrador" },
                new Rol { IdRol = 2, NombreRol = "Empaquetador" },
                new Rol { IdRol = 3, NombreRol = "Repartidor" },
                new Rol { IdRol = 4, NombreRol = "Cliente" }
            );

            // Usuarios iniciales
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    NombreCompleto = "Administrador",
                    DNI = "00000000",
                    Celular = "999999999",
                    Direccion = "Cajamarca",
                    Correo = "admin@quesitostore.com",
                    Contrasena = "admin123",
                    IdRol = 1
                },
                new Usuario
                {
                    IdUsuario = 2,
                    NombreCompleto = "Empaquetador",
                    DNI = "00000001",
                    Celular = "999999998",
                    Direccion = "Cajamarca",
                    Correo = "empaquetador@quesitostore.com",
                    Contrasena = "empaquetador123",
                    IdRol = 2
                },
                new Usuario
                {
                    IdUsuario = 3,
                    NombreCompleto = "Repartidor",
                    DNI = "00000002",
                    Celular = "999999997",
                    Direccion = "Cajamarca",
                    Correo = "repartidor@quesitostore.com",
                    Contrasena = "repartidor123",
                    IdRol = 3
                },
                new Usuario
                {
                    IdUsuario = 4,
                    NombreCompleto = "Cliente",
                    DNI = "00000003",
                    Celular = "999999996",
                    Direccion = "Cajamarca",
                    Correo = "cliente@quesitostore.com",
                    Contrasena = "cliente123",
                    IdRol = 4
                }
            );

            // Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { IdCategoria = 1, NombreCategoria = "Gorras",     Descripcion = "Gorras urbanas y deportivas" },
                new Categoria { IdCategoria = 2, NombreCategoria = "Zapatillas", Descripcion = "Zapatillas casuales y deportivas" },
                new Categoria { IdCategoria = 3, NombreCategoria = "Polos",      Descripcion = "Polos de algodón y lycra" },
                new Categoria { IdCategoria = 4, NombreCategoria = "Poleras",    Descripcion = "Poleras con y sin capucha" },
                new Categoria { IdCategoria = 5, NombreCategoria = "Pantalones", Descripcion = "Jeans, joggers y cargo pants" }
            );

            // Tallas maestras (catálogo único de nombres de talla, con orden de presentación)
            modelBuilder.Entity<Talla>().HasData(
                // Gorras (1-6)
                new Talla { IdTalla = 1,  Nombre = "Única / Ajustable", Orden = 1 },
                new Talla { IdTalla = 2,  Nombre = "S — 54–56 cm",      Orden = 2 },
                new Talla { IdTalla = 3,  Nombre = "M — 56–58 cm",      Orden = 3 },
                new Talla { IdTalla = 4,  Nombre = "L — 58–60 cm",      Orden = 4 },
                new Talla { IdTalla = 5,  Nombre = "XL — 60–62 cm",     Orden = 5 },
                new Talla { IdTalla = 6,  Nombre = "XXL — 62–64 cm",    Orden = 6 },
                // Zapatillas EU (7-16)
                new Talla { IdTalla = 7,  Nombre = "36", Orden = 7 },
                new Talla { IdTalla = 8,  Nombre = "37", Orden = 8 },
                new Talla { IdTalla = 9,  Nombre = "38", Orden = 9 },
                new Talla { IdTalla = 10, Nombre = "39", Orden = 10 },
                new Talla { IdTalla = 11, Nombre = "40", Orden = 11 },
                new Talla { IdTalla = 12, Nombre = "41", Orden = 12 },
                new Talla { IdTalla = 13, Nombre = "42", Orden = 13 },
                new Talla { IdTalla = 14, Nombre = "43", Orden = 14 },
                new Talla { IdTalla = 15, Nombre = "44", Orden = 15 },
                new Talla { IdTalla = 16, Nombre = "45", Orden = 16 },
                // Letras (Polos / Poleras) (17-22)
                new Talla { IdTalla = 17, Nombre = "XS",  Orden = 17 },
                new Talla { IdTalla = 18, Nombre = "S",   Orden = 18 },
                new Talla { IdTalla = 19, Nombre = "M",   Orden = 19 },
                new Talla { IdTalla = 20, Nombre = "L",   Orden = 20 },
                new Talla { IdTalla = 21, Nombre = "XL",  Orden = 21 },
                new Talla { IdTalla = 22, Nombre = "XXL", Orden = 22 },
                // Pantalones (numérico distinto, 23-32)
                new Talla { IdTalla = 23, Nombre = "28", Orden = 23 },
                new Talla { IdTalla = 24, Nombre = "30", Orden = 24 },
                new Talla { IdTalla = 25, Nombre = "32", Orden = 25 },
                new Talla { IdTalla = 26, Nombre = "34", Orden = 26 },
                new Talla { IdTalla = 27, Nombre = "36", Orden = 27 },
                new Talla { IdTalla = 28, Nombre = "38", Orden = 28 },
                new Talla { IdTalla = 29, Nombre = "40", Orden = 29 },
                new Talla { IdTalla = 30, Nombre = "42", Orden = 30 },
                new Talla { IdTalla = 31, Nombre = "44", Orden = 31 },
                new Talla { IdTalla = 32, Nombre = "46", Orden = 32 }
            );

            // Vínculo Categoría <-> Talla (qué tallas ofrece cada categoría existente)
            modelBuilder.Entity<CategoriaTalla>().HasData(
                // Gorras (IdCategoria=1) -> Tallas 1-6
                new CategoriaTalla { IdCategoria = 1, IdTalla = 1 },
                new CategoriaTalla { IdCategoria = 1, IdTalla = 2 },
                new CategoriaTalla { IdCategoria = 1, IdTalla = 3 },
                new CategoriaTalla { IdCategoria = 1, IdTalla = 4 },
                new CategoriaTalla { IdCategoria = 1, IdTalla = 5 },
                new CategoriaTalla { IdCategoria = 1, IdTalla = 6 },
                // Zapatillas (IdCategoria=2) -> Tallas 7-16 (EU 36-45)
                new CategoriaTalla { IdCategoria = 2, IdTalla = 7 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 8 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 9 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 10 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 11 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 12 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 13 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 14 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 15 },
                new CategoriaTalla { IdCategoria = 2, IdTalla = 16 },
                // Polos (IdCategoria=3) -> Tallas 17-22 (XS-XXL)
                new CategoriaTalla { IdCategoria = 3, IdTalla = 17 },
                new CategoriaTalla { IdCategoria = 3, IdTalla = 18 },
                new CategoriaTalla { IdCategoria = 3, IdTalla = 19 },
                new CategoriaTalla { IdCategoria = 3, IdTalla = 20 },
                new CategoriaTalla { IdCategoria = 3, IdTalla = 21 },
                new CategoriaTalla { IdCategoria = 3, IdTalla = 22 },
                // Poleras (IdCategoria=4) -> Tallas 17-22 (XS-XXL)
                new CategoriaTalla { IdCategoria = 4, IdTalla = 17 },
                new CategoriaTalla { IdCategoria = 4, IdTalla = 18 },
                new CategoriaTalla { IdCategoria = 4, IdTalla = 19 },
                new CategoriaTalla { IdCategoria = 4, IdTalla = 20 },
                new CategoriaTalla { IdCategoria = 4, IdTalla = 21 },
                new CategoriaTalla { IdCategoria = 4, IdTalla = 22 },
                // Pantalones (IdCategoria=5) -> Tallas 23-32 (28-46)
                new CategoriaTalla { IdCategoria = 5, IdTalla = 23 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 24 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 25 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 26 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 27 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 28 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 29 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 30 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 31 },
                new CategoriaTalla { IdCategoria = 5, IdTalla = 32 }
            );

            // Tipos de entrega
            modelBuilder.Entity<TipoEntrega>().HasData(
                new TipoEntrega { IdTipoEntrega = 1, Nombre = "Recojo en tienda",    Descripcion = "El cliente recoge en la tienda de Cajamarca" },
                new TipoEntrega { IdTipoEntrega = 2, Nombre = "Envío a domicilio",   Descripcion = "Entrega en dirección del cliente dentro de Cajamarca" },
                new TipoEntrega { IdTipoEntrega = 3, Nombre = "Envío a otra ciudad", Descripcion = "Envío nacional mediante empresa de transporte" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
