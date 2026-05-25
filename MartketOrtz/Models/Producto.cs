namespace MartketOrtz.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        
        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int IdCategoria { get; set; }

        private static readonly List<Producto> _productosMock = new()
        {
            new() { IdProducto = 1, Nombre = "Coca-Cola", Precio = 1500m, Stock = 50, IdCategoria = 1 },
            new() { IdProducto = 2, Nombre = "Pan", Precio = 2500m, Stock = 30, IdCategoria = 2 },
            new() { IdProducto = 3, Nombre = "Leche", Precio = 1200m, Stock = 20, IdCategoria = 1 },
            new() { IdProducto = 4, Nombre = "Queso Gouda", Precio = 3500m, Stock = 15, IdCategoria = 3 },
            new() { IdProducto = 5, Nombre = "Cereal Corn Flakes", Precio = 3000m, Stock = 25, IdCategoria = 4 }
        };

        public static List<Producto> ObtenerTodos()
        {
            return _productosMock;
        }

        public static void DescontarStock(int idProducto, int cantidadVendida)
        {
            var producto = _productosMock.FirstOrDefault(p => p.IdProducto == idProducto);
            if (producto != null)
            {
                producto.Stock -= cantidadVendida;
            }
        }
    }
}
