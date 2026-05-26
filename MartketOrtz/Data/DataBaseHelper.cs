using System.Data.SqlClient;

namespace MartketOrtz.Data
{
    public class DataBaseHelper
    {

        private readonly string _connectionString;

        public DataBaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task InsertCategoria(string nombre, string descripcion)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "INSERT INTO Personas (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }



        public async Task InsertProducto(string nombre, string categoria, decimal precio, int stock)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "INSERT INTO Productos (Nombre, Categoria, Precio, Stock) VALUES (@Nombre, @Categoria, @Precio, @Stock)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Categoria", categoria);
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Stock", stock);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        }
}
