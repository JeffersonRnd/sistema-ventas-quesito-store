namespace sistema_ventas_quesito_store.Models
{
    // Producto.Talla guarda pares "Nombre:Stock" separados por coma, ej: "S:10,M:0,L:5"
    // Esto permite que el administrador gestione la disponibilidad de cada talla
    // sin requerir tablas adicionales.
    public static class TallaHelper
    {
        public static List<(string Nombre, int Stock)> Parse(string? talla)
        {
            var lista = new List<(string, int)>();
            if (string.IsNullOrWhiteSpace(talla)) return lista;

            foreach (var parte in talla.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var seg = parte.Split(':');
                var nombre = seg[0].Trim();
                if (nombre == "") continue;
                int stock = (seg.Length > 1 && int.TryParse(seg[1].Trim(), out var s)) ? s : 0;
                lista.Add((nombre, stock));
            }
            return lista;
        }

        public static string Serializar(IEnumerable<(string Nombre, int Stock)> tallas)
            => string.Join(",", tallas.Select(t => $"{t.Nombre}:{t.Stock}"));

        public static bool TieneTallas(string? talla) => Parse(talla).Count > 0;
    }
}
