using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Carrito
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCarrito { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // FK Cliente (1 a 1)
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        // Navegación
        public ICollection<CarritoDetalle> Detalles { get; set; } = new List<CarritoDetalle>();
    }
}
