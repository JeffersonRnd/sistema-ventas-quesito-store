using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_ventas_quesito_store.Models
{
    // Pago simulado que se genera automáticamente al confirmar el pedido.
    // Por seguridad (aunque sea una simulación) NUNCA se guarda el número
    // completo de tarjeta ni el CVV: solo la marca y los últimos 4 dígitos.
    public class Pago
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPago { get; set; }

        [Required, StringLength(20)]
        public string MetodoPago { get; set; } = "Tarjeta";
        // Tarjeta (por ahora es el único método simulado)

        [Required, StringLength(100)]
        public string TitularTarjeta { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string TarjetaMarca { get; set; } = string.Empty;
        // Visa | Mastercard | Tarjeta

        [Required, StringLength(4)]
        public string TarjetaUltimos4 { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required, StringLength(20)]
        public string EstadoPago { get; set; } = "Aprobado";
        // Aprobado | Rechazado (simulado; en este sistema siempre se aprueba si la validación pasa)

        // FK Pedido (relación 1 a 1)
        public int IdPedido { get; set; }
        [ForeignKey("IdPedido")]
        public Pedido? Pedido { get; set; }
    }
}
