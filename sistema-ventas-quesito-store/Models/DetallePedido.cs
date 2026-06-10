using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class DetallePedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetallePedido { get; set; }

        [Required]
        public int Cantidad { get; set; }

        // Precio al momento de la compra (no cambia si el producto sube de precio)
        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        // FK Pedido
        public int IdPedido { get; set; }
        [ForeignKey("IdPedido")]
        public Pedido? Pedido { get; set; }

        // FK Producto
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }
    }
}
