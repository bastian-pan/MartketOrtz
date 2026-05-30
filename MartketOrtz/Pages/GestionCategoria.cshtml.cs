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
            await _databaseHelper.InsertCategoria(NuevaCategoria.Nombre, NuevaCategoria.Descripcion);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _databaseHelper.DeleteCategoria(id);
            return RedirectToPage();
        }
    }
}