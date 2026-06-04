using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MartketOrtz.Data;
using MartketOrtz.Models;

namespace MartketOrtz.Pages
{
    public class GestionCategoriaModel : PageModel
    {
        private readonly DataBaseHelper _databaseHelper;

        public GestionCategoriaModel(DataBaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public List<Categoria> Categorias { get; set; } = new List<Categoria>();

        [BindProperty]
        public Categoria NuevaCategoria { get; set; } = new Categoria();

        public async Task OnGet()
        {
            Categorias = await _databaseHelper.GetCategorias();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(NuevaCategoria.Nombre) || string.IsNullOrEmpty(NuevaCategoria.Descripcion))
                return RedirectToPage();

            await _databaseHelper.InsertCategoria(NuevaCategoria.Nombre, NuevaCategoria.Descripcion);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, string nombre, string descripcion)
        {
            await _databaseHelper.UpdateCategoria(id, nombre, descripcion);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (await _databaseHelper.CategoriaHasProductos(id))
            {
                TempData["Error"] = "No se puede eliminar esta categoría porque tiene productos asociados. Elimina los productos primero.";
                return RedirectToPage();
            }

            await _databaseHelper.DeleteCategoria(id);
            return RedirectToPage();
        }
    }
}