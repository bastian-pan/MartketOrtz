using MartketOrtz.Data;
using MartketOrtz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Transbank.Common;
using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;

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

        // para multiples id
        [BindProperty] public List<int> IdsVentasSeleccionadas { get; set; } = new();
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

        // Agregar al carrito
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

        // Eliminar
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

        // Cancelar
        public IActionResult OnPostCancelar()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        // Editar 
        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (IndexEditar.HasValue)
            {
                var carrito = LeerCarrito();
                if (IndexEditar.Value >= 0 && IndexEditar.Value < carrito.Count)
                {
                    var item = carrito[IndexEditar.Value];

                    IdProductoSeleccionado = item.IdProducto;
                    Cantidad = item.Cantidad;

                    carrito.RemoveAt(IndexEditar.Value);
                    GuardarCarrito(carrito);

                    Productos = await _databaseHelper.GetProductosConCategoria();
                    Ventas = await _databaseHelper.GetVentas();
                    Carrito = carrito;
                    return Page();
                }
            }
            return RedirectToPage();
        }

        // Registrar venta y restar stock
        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            var carrito = LeerCarrito();
            if (carrito.Count == 0) return RedirectToPage();

            decimal total = carrito.Sum(x => x.SubTotal);
            decimal iva = carrito.Sum(x => x.IVA);

            await _databaseHelper.InsertVenta(DateTime.Now, total, iva);

            foreach (var item in carrito)
            {
                await _databaseHelper.RestarStockProducto(item.IdProducto, item.Cantidad);
            }

            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        // Eliminar una o varias ventas seleccionadas
        public async Task<IActionResult> OnPostEliminarVentaAsync()
        {
            if (IdsVentasSeleccionadas != null && IdsVentasSeleccionadas.Count > 0)
            {
                // bucle para eliminar cada venta seleccionada
                foreach (var id in IdsVentasSeleccionadas)
                {
                    await _databaseHelper.DeleteVenta(id);
                }
            }
            return RedirectToPage();
        }

        // Actualizar valor venta
        public async Task<IActionResult> OnPostActualizarVentaAsync()
        {
            // aca validamos que solo haya una venta seleccionada y que el nuevo total tenga valor, para evitar errores al actualizar
            if (IdsVentasSeleccionadas != null && IdsVentasSeleccionadas.Count == 1 && NuevoTotal.HasValue)
            {
                int idVenta = IdsVentasSeleccionadas[0];
                decimal nuevoIva = Math.Round(NuevoTotal.Value - (NuevoTotal.Value / 1.19m), 0);

                await _databaseHelper.UpdateVenta(idVenta, NuevoTotal.Value, nuevoIva);
            }
            return RedirectToPage();
        }



        //API TRANSBANK

        
        private Transaction GetWebpayTransaction()
        {
            var options = new Transbank.Common.Options(
                Transbank.Common.IntegrationCommerceCodes.WEBPAY_PLUS,
                Transbank.Common.IntegrationApiKeys.WEBPAY,
                WebpayIntegrationType.Test
            );
            return new Transaction(options);
        }

        
        public IActionResult OnPostPagar()
        {
            var carrito = LeerCarrito();
            if (carrito.Count == 0) return RedirectToPage();

            decimal total = carrito.Sum(x => x.SubTotal);

            var transaction = GetWebpayTransaction();
            string buyOrder = "orden-" + DateTime.Now.Ticks;
            string sessionId = HttpContext.Session.Id;
            string returnUrl = $"{Request.Scheme}://{Request.Host}/GestionVenta?handler=Retorno";

            var response = transaction.Create(buyOrder, sessionId, (long)total, returnUrl);

            return Redirect($"{response.Url}?token_ws={response.Token}");
        }


        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> OnGetRetornoAsync(string token_ws)
        {
            var transaction = GetWebpayTransaction();
            var result = transaction.Commit(token_ws);

            TempData["Debug"] = $"ResponseCode: {result.ResponseCode}, Status: {result.Status}";

            var carrito = LeerCarrito();

            if (result.ResponseCode == 0 && carrito.Count > 0)
            {
                decimal total = carrito.Sum(x => x.SubTotal);
                decimal iva = carrito.Sum(x => x.IVA);

                await _databaseHelper.InsertVenta(DateTime.Now, total, iva);

                foreach (var item in carrito)
                {
                    await _databaseHelper.RestarStockProducto(item.IdProducto, item.Cantidad);
                }

                HttpContext.Session.Remove(SessionKey);
                TempData["MensajePago"] = "Pago aprobado. Venta registrada.";
            }
            else
            {
                TempData["MensajePago"] = "Pago rechazado. La venta no se registró.";
            }

            Productos = await _databaseHelper.GetProductosConCategoria();
            Ventas = await _databaseHelper.GetVentas();
            Carrito = LeerCarrito();

            return RedirectToPage();
        }

    }
}