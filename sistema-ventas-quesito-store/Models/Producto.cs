using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [RegularExpression(@"^(?=.*[A-Za-zÁÉÍÓÚÑáéíóúñ].*[A-Za-zÁÉÍÓÚÑáéíóúñ]).+$",
            ErrorMessage = "El nombre debe contener palabras completas (no solo espacios o símbolos)")]
        public string Nombre { get; set; } = string.Empty;


        [StringLength(300)]
        [RegularExpression(@"(?s)^$|^(?=.*[A-Za-zÁÉÍÓÚÑáéíóúñ].*[A-Za-zÁÉÍÓÚÑáéíóúñ]).+$",
        ErrorMessage = "La descripción debe contener palabras completas (no solo espacios o símbolos)")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Selecciona al menos una talla")]
        [StringLength(150)]
        public string? Talla { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser un valor positivo")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [StringLength(300)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }

        public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
        public ICollection<CarritoDetalle> CarritoDetalles { get; set; } = new List<CarritoDetalle>();
    }
}