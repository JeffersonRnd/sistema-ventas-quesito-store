using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required, StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string DNI { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Celular { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required, StringLength(100)]
        public string Correo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Contrasena { get; set; } = string.Empty;

        public int IdRol { get; set; }
        [ForeignKey("IdRol")]
        public Rol? Rol { get; set; }

        // Navegación
        public ICollection<Pedido>  Pedidos  { get; set; } = new List<Pedido>();
        public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
        public Carrito? Carrito { get; set; }
    }
}
