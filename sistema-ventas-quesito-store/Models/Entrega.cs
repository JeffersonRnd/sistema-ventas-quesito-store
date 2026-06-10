using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Entrega
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEntrega { get; set; }

        [StringLength(200)]
        public string? DireccionDestino { get; set; }

        [StringLength(100)]
        public string? CiudadDestino { get; set; }

        [StringLength(100)]
        public string? EmpresaEnvio { get; set; }
        // Solo para envíos a otra ciudad: Olva, Shalom, etc.

        [StringLength(100)]
        public string? MedioTransporte { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoAdicional { get; set; }

        [StringLength(300)]
        public string? Observaciones { get; set; }

        // FK Pedido (1 a 1)
        public int IdPedido { get; set; }
        [ForeignKey("IdPedido")]
        public Pedido? Pedido { get; set; }

        // FK Repartidor — null si es recojo en tienda
        public int? IdRepartidor { get; set; }
        [ForeignKey("IdRepartidor")]
        public Usuario? Repartidor { get; set; }

        // Navegación historial de estados
        public ICollection<EstadoEntrega> EstadosEntrega { get; set; } = new List<EstadoEntrega>();
    }
}
