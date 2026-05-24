namespace MartketOrtz.Models
{
    public class DetalleVenta
    {
        public int IdProduto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal SubTotal => Cantidad * PrecioUnitario;

        public decimal IVA => Math.Round(SubTotal - (SubTotal / 1.19m), 2);

        public decimal Neto => Math.Round(SubTotal / 1.19m, 2);
    }
}
