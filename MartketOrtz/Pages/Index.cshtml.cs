using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartketOrtz.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var usuario = HttpContext.Session.GetString("UsuarioLogueado");

            if (string.IsNullOrEmpty(usuario))
            {
                // Si está vacio (no ha iniciado sesion), lo mandamos al Login
                return RedirectToPage("/Login");
            }

            // sí hay sesión, lo dejamos entrar a la pagina de Inicio sin problemas
            return Page();
        }
    }
}
