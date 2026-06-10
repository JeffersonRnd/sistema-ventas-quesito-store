using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class TipoEntrega
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTipoEntrega { get; set; }

        [Required, StringLength(80)]
        public string Nombre { get; set; } = string.Empty;
        // "Recojo en tienda" | "Envío a domicilio" | "Envío a otra ciudad"

        [StringLength(200)]
        public string? Descripcion { get; set; }

        // Navegación
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
