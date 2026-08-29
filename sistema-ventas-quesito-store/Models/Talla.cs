using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    // Catálogo maestro de tallas. Una talla puede pertenecer a varias categorías
    // (ej: "S" aplica tanto a Polos como a Poleras) mediante CategoriaTalla.
    public class Talla
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTalla { get; set; }

        [Required, StringLength(30)]
        public string Nombre { get; set; } = string.Empty;

        // Define el orden de presentación (ej: XS antes que S antes que M)
        public int Orden { get; set; } = 0;

        public ICollection<CategoriaTalla> CategoriaTallas { get; set; } = new List<CategoriaTalla>();
    }
}
