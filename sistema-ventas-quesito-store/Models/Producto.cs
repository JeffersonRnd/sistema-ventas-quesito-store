using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [StringLength(20)]
        public string? Talla { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        [StringLength(300)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        // FK
        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }

        // Navegación
        public ICollection<DetallePedido>  DetallesPedido  { get; set; } = new List<DetallePedido>();
        public ICollection<CarritoDetalle> CarritoDetalles { get; set; } = new List<CarritoDetalle>();
    }
}
