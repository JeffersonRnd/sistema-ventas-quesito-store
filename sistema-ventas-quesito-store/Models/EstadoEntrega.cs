using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class EstadoEntrega
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEstadoEntrega { get; set; }

        [Required, StringLength(80)]
        public string Estado { get; set; } = string.Empty;
        // Domicilio:     "Recogido en tienda" | "Saliendo de tienda" | "En camino" |
        //                "En la dirección indicada" | "Entregado"
        // Otra ciudad:   "Recogido en tienda" | "Saliendo de tienda" |
        //                "En camino a empresa de envío" | "Entregado a empresa de envío" |
        //                "Salió de Cajamarca" | "En tránsito" | "Llegó a ciudad destino" |
        //                "En espera de entrega" | "Entregado" | "Finalizado"

        [StringLength(300)]
        public string? Observacion { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        // FK Entrega
        public int IdEntrega { get; set; }
        [ForeignKey("IdEntrega")]
        public Entrega? Entrega { get; set; }
    }
}
