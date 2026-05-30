namespace MartketOrtz.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        
        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int IdCategoria { get; set; }

    }
}
