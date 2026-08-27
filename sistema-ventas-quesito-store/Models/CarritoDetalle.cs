using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class CarritoDetalle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCarritoDetalle { get; set; }

        [Required]
        public int Cantidad { get; set; }

        // Talla elegida por el cliente (si el producto maneja tallas)
        [StringLength(30)]
        public string? TallaSeleccionada { get; set; }

        // FK Carrito
        public int IdCarrito { get; set; }
        [ForeignKey("IdCarrito")]
        public Carrito? Carrito { get; set; }

        // FK Producto
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }
    }
}
