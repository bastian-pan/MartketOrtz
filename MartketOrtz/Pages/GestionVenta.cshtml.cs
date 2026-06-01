using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MartketOrtz.Models;
using System.Text.Json;
using MartketOrtz.Data;

namespace MartketOrtz.Pages
{
    public class GestionVentaModel : PageModel
    {
        private readonly DataBaseHelper _databaseHelper;
        private const string SessionKey = "Carrito";

        public GestionVentaModel(DataBaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public List<(Producto producto, string nombreCategoria)> Productos { get; set; } = new();
        public List<DetalleVenta> Carrito { get; set; } = new();
        public List<Venta> Ventas { get; set; } = new();

        [BindProperty] public int IdProductoSeleccionado { get; set; }
        [BindProperty] public int Cantidad { get; set; } = 1;
        [BindProperty] public int? IndexEditar { get; set; }
        [BindProperty] public int? IdVentaSeleccionada { get; set; }
        [BindProperty] public decimal? NuevoTotal { get; set; }

        private List<DetalleVenta> LeerCarrito()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            return string.IsNullOrEmpty(json) ? new List<DetalleVenta>() : JsonSerializer.Deserialize<List<DetalleVenta>>(json)!;
        }

        private void GuardarCarrito(List<DetalleVenta> carrito)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(carrito));
        }

        public async Task OnGetAsync()
        {
            Productos = await _databaseHelper.GetProductosConCategoria();
            Ventas = await _databaseHelper.GetVentas();
            Carrito = LeerCarrito();
        }

        public async Task<JsonResult> OnGetProductoInfoAsync(int id)
        {
            var productosDb = await _databaseHelper.GetProductosConCategoria();
            var producto = productosDb.FirstOrDefault(p => p.producto.IdProducto == id).producto;

            if (producto != null)
            {
                return new JsonResult(new { precio = producto.Precio, stock = producto.Stock });
            }
            return new JsonResult(null);
        }

        //  INSERT 1: AGREGAR AL CARRITO 
        public async Task<IActionResult> OnPostAgregarAsync()
        {
            if (IdProductoSeleccionado <= 0 || Cantidad <= 0) return RedirectToPage();

            var productosDb = await _databaseHelper.GetProductosConCategoria();
            var producto = productosDb.FirstOrDefault(p => p.producto.IdProducto == IdProductoSeleccionado).producto;

            if (producto == null) return RedirectToPage();

            var carrito = LeerCarrito();
            var existente = carrito.FirstOrDefault(c => c.IdProducto == IdProductoSeleccionado);

            if (existente != null)
            {
                existente.Cantidad += Cantidad;
            }
            else
            {
                carrito.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    Cantidad = Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            GuardarCarrito(carrito);
            return RedirectToPage();
        }

        // INSERT 2: REGISTRAR VENTA FINAL EN SQL 
        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            var carrito = LeerCarrito();
            if (carrito.Count == 0) return RedirectToPage();

            decimal total = carrito.Sum(x => x.SubTotal);
            decimal iva = carrito.Sum(x => x.IVA);

            await _databaseHelper.InsertVenta(DateTime.Now, total, iva);

            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        public IActionResult OnPostEliminarItem()
        {
            return RedirectToPage();
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage();
        }

        public IActionResult OnPostEditar()
        {
            return RedirectToPage();
        }

        public IActionResult OnPostEliminarVenta()
        {
            return RedirectToPage();
        }

        public IActionResult OnPostActualizarVenta()
        {
            return RedirectToPage();
        }
    }
}