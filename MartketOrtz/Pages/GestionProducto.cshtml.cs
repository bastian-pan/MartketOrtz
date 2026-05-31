using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MartketOrtz.Data;
using MartketOrtz.Models;

namespace MartketOrtz.Pages
{
    public class GestionProductoModel : PageModel
    {
        private readonly DataBaseHelper _databaseHelper;

        public GestionProductoModel(DataBaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public List<(Producto producto, string nombreCategoria)> Productos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();

        [BindProperty]
        public Producto NuevoProducto { get; set; } = new Producto();

        public async Task OnGet()
        {
            Productos = await _databaseHelper.GetProductosConCategoria();
            Categorias = await _databaseHelper.GetCategorias();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _databaseHelper.InsertProducto(
                NuevoProducto.Nombre,
                NuevoProducto.IdCategoria,
                NuevoProducto.Precio,
                NuevoProducto.Stock
            );
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _databaseHelper.DeleteProducto(id);
            return RedirectToPage();
        }
    }
}