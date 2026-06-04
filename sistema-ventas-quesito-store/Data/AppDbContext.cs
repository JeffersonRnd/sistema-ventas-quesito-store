using Microsoft.EntityFrameworkCore;
using sistema_ventas_quesito_store.Models;

namespace sistema_ventas_quesito_store.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol);

            // Roles del sistema Quesito Store
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NombreRol = "Administrador" },
                new Rol { IdRol = 2, NombreRol = "Empleado" },
                new Rol { IdRol = 3, NombreRol = "Repartidor" },
                new Rol { IdRol = 4, NombreRol = "Cliente" }
            );

            // Usuario administrador inicial
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario      = 1,
                    NombreCompleto = "Administrador",
                    DNI            = "00000000",
                    Celular        = "999999999",
                    Direccion      = "Cajamarca",
                    Correo         = "admin@quesitostore.com",
                    Contrasena     = "Admin123",
                    IdRol          = 1
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
