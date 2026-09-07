using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    // Tabla puente: qué tallas están habilitadas para cada categoría.
    // Esto reemplaza el diccionario de tallas que antes vivía hardcodeado en JavaScript.
    public class CategoriaTalla
    {
        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }

        public int IdTalla { get; set; }
        [ForeignKey("IdTalla")]
        public Talla? Talla { get; set; }
    }
}
