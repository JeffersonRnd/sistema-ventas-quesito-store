using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Pedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedido { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required, StringLength(30)]
        public string EstadoPedido { get; set; } = "Pendiente";
        // Pendiente | Aprobado | Empacando | Empacado y listo | En despacho | Finalizado | Cancelado

        [Required, StringLength(20)]
        public string EstadoPago { get; set; } = "Pendiente";
        // Pendiente | Pagado

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [StringLength(300)]
        public string? Observaciones { get; set; }

        // FK Cliente
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Usuario? Cliente { get; set; }

        // FK TipoEntrega
        public int IdTipoEntrega { get; set; }
        [ForeignKey("IdTipoEntrega")]
        public TipoEntrega? TipoEntrega { get; set; }

        // Navegación
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public Entrega? Entrega { get; set; }
        public Pago? Pago { get; set; }
    }
}
