namespace MartketOrtz.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public decimal IVA { get; set; }

        public decimal Neto => Math.Round(Total / 1.19m, 2);

        private static int _nextId = 1;

        private static readonly List<Venta> _ventas = new()
        {
            new()
            {
                IdVenta = _nextId++,
                Fecha = DateTime.Now.AddDays(-2),
                Total = 11900m,
                IVA = Math.Round(11900m - (11900m / 1.19m), 2)
            },
        };


        public static List<Venta> ObtenerVentas() => _ventas.OrderByDescending(v => v.Fecha).ToList();

        public static void Agregar(Venta v)
        {
            v.IdVenta = _nextId++;
            _ventas.Add(v);
        }

        public static bool Eliminar(int id)
        {
            var v = _ventas.FirstOrDefault(x => x.IdVenta == id);
            if (v == null) return false;
            _ventas.Remove(v);
            return true;

        }

        public static bool Actualizar(int id, decimal nuevoTotal)
        {
            var v = _ventas.FirstOrDefault(x => x.IdVenta == id);
            if (v == null) return false;
            v.Total = nuevoTotal;
            v.IVA = Math.Round(nuevoTotal - (nuevoTotal / 1.19m), 2);
            return true;
        }

    }
}
