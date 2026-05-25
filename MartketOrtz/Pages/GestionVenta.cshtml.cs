using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MartketOrtz.Models;
using System.Text.Json;

namespace MartketOrtz.Pages
{
    public class GestionVentaModel : PageModel
    {
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public List<DetalleVenta> Carrito { get; set; } = new List<DetalleVenta>();

        public List<Venta> Ventas { get; set; } = new List<Venta>();

        [BindProperty] public int IdProductoSeleccionado { get; set; }

        [BindProperty] public int Cantidad { get; set; } = 1;

        [BindProperty] public int? IndexEditar { get; set; }

        [BindProperty] public int? IdVentaSeleccionada { get; set; }

        [BindProperty] public decimal? NuevoTotal { get; set; }


        private const string SessionKey = "Carrito";

        private List<DetalleVenta> leerCarrito()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            return string.IsNullOrEmpty(json) ? new List<DetalleVenta>() : JsonSerializer.Deserialize<List<DetalleVenta>>(json)!;
        }

        private void GuardarCarrito(List<DetalleVenta> carrito)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(carrito));
        }

        private void CargarDatos()
        {
            Productos = Producto.ObtenerTodos();
            Ventas = Venta.ObtenerVentas();
            Carrito = leerCarrito();
        }

        public void OnGet()
        {
            CargarDatos();

        }
        public IActionResult OnPostAgregar()
        {
            CargarDatos();
            var producto = Productos.FirstOrDefault(p => p.IdProducto == IdProductoSeleccionado);
            if (producto == null) return RedirectToPage();

            var carrito = leerCarrito();
            var existente = carrito.FirstOrDefault(c => c.IdProduto == IdProductoSeleccionado);

            if (existente != null)
            {
                existente.Cantidad += Cantidad;
            }
            else
            {
                carrito.Add(new DetalleVenta
                {
                    IdProduto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    Cantidad = Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            GuardarCarrito(carrito);
            return RedirectToPage();



        }

        public IActionResult OnPostRegistrar()
        {
            var carrito = leerCarrito();
            if (carrito.Count == 0) return RedirectToPage();

            decimal total = carrito.Sum(x => x.SubTotal);
            decimal iva = Math.Round(total - total / 1.19m, 2);

            // Guardar Venta
            Venta.Agregar(new Venta
            {
                Fecha = DateTime.Now,
                Total = total,
                IVA = iva
            });

            // Descontar Stock
            foreach (var item in carrito)
            {
                Producto.DescontarStock(item.IdProduto, item.Cantidad);
            }

            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        public IActionResult OnPostCancelar()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToPage();
        }

        public IActionResult OnPostEliminarItem()
        {
            if (IndexEditar.HasValue)
            {
                var carrito = leerCarrito();
                if (IndexEditar.Value >= 0 && IndexEditar.Value < carrito.Count)
                {
                    carrito.RemoveAt(IndexEditar.Value);
                    GuardarCarrito(carrito);
                }
            }
            return RedirectToPage();
        }

        // Dejo preparados los métodos vacíos o básicos para que no te tire error el HTML
        public IActionResult OnPostEditar()
        {
            // Lógica de edición pendiente
            return RedirectToPage();
        }

        public IActionResult OnPostEliminarVenta()
        {
            if (IdVentaSeleccionada.HasValue)
            {
                Venta.Eliminar(IdVentaSeleccionada.Value);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostActualizarVenta()
        {
            if (IdVentaSeleccionada.HasValue && NuevoTotal.HasValue)
            {
                Venta.Actualizar(IdVentaSeleccionada.Value, NuevoTotal.Value);
            }
            return RedirectToPage();
        }
    }
}
