using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MartketOrtz.Data;

namespace MartketOrtz.Pages
{
    public class LoginModel : PageModel
    {
        private readonly DataBaseHelper _databaseHelper;

        public LoginModel(DataBaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        [BindProperty]
        public string Usuario { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;


        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {

        }


        public async Task<IActionResult> OnPostAsync()
        {

            bool accesoConcedido = await _databaseHelper.ValidarLogin(Usuario, Password);

            if (accesoConcedido)
            {

                HttpContext.Session.SetString("Usuario Logueado", Usuario);


                return RedirectToPage("/Index");
            }
            else
            {

                ErrorMessage = "Usuario o contraseña incorrectos.";
                return Page();
            }
        }
    }
}