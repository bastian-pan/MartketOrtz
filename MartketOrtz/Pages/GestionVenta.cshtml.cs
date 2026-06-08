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

        // --- ACCIÓN: AGREGAR AL CARRITO ---
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

        // --- ACCIÓN: ELIMINAR UN ÍTEM DEL CARRITO ---
        public IActionResult OnPostEliminarItem()
        {
            if (IndexEditar.HasValue)
            {
                var carrito = LeerCarrito();
                if (IndexEditar.Value >= 0 && IndexEditar.Value < carrito.Count)
                {
                    carrito.RemoveAt(IndexEditar.Value);
                    GuardarCarrito(carrito);
                }
            }
            return RedirectToPage();
        }

        // --- ACCIÓN: VACIAR TODO EL CARRITO (CANCELAR) ---
        public IActionResult OnPostCancelar()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        // --- ACCIÓN: EDITAR ÍTEM DEL CARRITO ---
        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (IndexEditar.HasValue)
            {
                var carrito = LeerCarrito();
                if (IndexEditar.Value >= 0 && IndexEditar.Value < carrito.Count)
                {
                    var item = carrito[IndexEditar.Value];

                    // Devolvemos los valores al formulario superior
                    IdProductoSeleccionado = item.IdProducto;
                    Cantidad = item.Cantidad;

                    // Lo removemos temporalmente de la lista para que al "Agregar" no se duplique
                    carrito.RemoveAt(IndexEditar.Value);
                    GuardarCarrito(carrito);

                    // Recargamos colecciones antes de renderizar la página actual
                    Productos = await _databaseHelper.GetProductosConCategoria();
                    Ventas = await _databaseHelper.GetVentas();
                    Carrito = carrito;
                    return Page();
                }
            }
            return RedirectToPage();
        }

        // --- ACCIÓN: REGISTRAR VENTA EN SQL Y DESCONTAR STOCK ---
        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            var carrito = LeerCarrito();
            if (carrito.Count == 0) return RedirectToPage();

            decimal total = carrito.Sum(x => x.SubTotal);
            decimal iva = carrito.Sum(x => x.IVA);

            // 1. Guarda el registro de la venta general
            await _databaseHelper.InsertVenta(DateTime.Now, total, iva);

            // 2. Descuenta las unidades correspondientes de cada producto vendido
            foreach (var item in carrito)
            {
                await _databaseHelper.RestarStockProducto(item.IdProducto, item.Cantidad);
            }

            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        // --- ACCIÓN: ELIMINAR UNA VENTA DEL HISTORIAL ---
        public async Task<IActionResult> OnPostEliminarVentaAsync()
        {
            if (IdVentaSeleccionada.HasValue)
            {
                await _databaseHelper.DeleteVenta(IdVentaSeleccionada.Value);
            }
            return RedirectToPage();
        }

        // --- ACCIÓN: ACTUALIZAR EL MONTO DE UNA VENTA REALIZADA ---
        public async Task<IActionResult> OnPostActualizarVentaAsync()
        {
            if (IdVentaSeleccionada.HasValue && NuevoTotal.HasValue)
            {
                // Recalculamos el IVA en base al nuevo valor ingresado
                decimal nuevoIva = Math.Round(NuevoTotal.Value - (NuevoTotal.Value / 1.19m), 2);

                await _databaseHelper.UpdateVenta(IdVentaSeleccionada.Value, NuevoTotal.Value, nuevoIva);
            }
            return RedirectToPage();
        }
    }
}