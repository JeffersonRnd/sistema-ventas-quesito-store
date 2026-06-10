using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCategoria { get; set; }

        [Required, StringLength(80)]
        public string NombreCategoria { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
