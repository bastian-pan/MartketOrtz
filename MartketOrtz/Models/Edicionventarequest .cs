namespace MartketOrtz.Models
{
    public class ItemEditadoVenta
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }


    public class EdicionVentaRequest
    {
        public int IdVenta { get; set; }
        public List<ItemEditadoVenta> Items { get; set; } = new();
    }
}
