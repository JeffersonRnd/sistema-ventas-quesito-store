using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class EstadoEntrega
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEstadoEntrega { get; set; }

        [Required, StringLength(100)]
        public string Estado { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Observacion { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        public int IdEntrega { get; set; }
        [ForeignKey("IdEntrega")]
        public Entrega? Entrega { get; set; }
    }
}