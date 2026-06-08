using MartketOrtz.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting; // Agregado para leer la ruta del sistema

namespace MartketOrtz.Data
{
    public class DataBaseHelper
    {
        private readonly string _connectionString = string.Empty;

        // Modificamos el constructor para inyectar IWebHostEnvironment
        public DataBaseHelper(IConfiguration configuration, IWebHostEnvironment env)
        {
            string rawConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            // Reemplazamos |DataDirectory| por la ruta real de la carpeta del proyecto en la PC actual
            _connectionString = rawConnectionString.Replace("|DataDirectory|", env.ContentRootPath);
        }

        //CATEGORIA

        public async Task InsertCategoria(string nombre, string descripcion)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "INSERT INTO Categoria (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nombre", nombre);
            cmd.Parameters.AddWithValue("@Descripcion", descripcion);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Categoria>> GetCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "SELECT IdCategoria, Nombre, Descripcion FROM Categoria";
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                categorias.Add(new Categoria
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.GetString(2)
                });
            }
            return categorias;
        }

        //verificar que no hayan productos activos con esa categoria
        public async Task<bool> CategoriaHasProductos(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "SELECT COUNT(*) FROM Producto WHERE IdCategoria = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }


        public async Task DeleteCategoria(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "DELETE FROM Categoria WHERE IdCategoria = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateCategoria(int id, string nombre, string descripcion)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "UPDATE Categoria SET Nombre = @Nombre, Descripcion = @Descripcion WHERE IdCategoria = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nombre", nombre);
            cmd.Parameters.AddWithValue("@Descripcion", descripcion);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Categoria?> GetCategoriaById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "SELECT IdCategoria, Nombre, Descripcion FROM Categoria WHERE IdCategoria = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new Categoria
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.GetString(2)
                };
            }
            return null;
        }

        //PRODUCTO

        public async Task InsertProducto(string nombre, int idCategoria, decimal precio, int stock)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "INSERT INTO Producto (Nombre, IdCategoria, Precio, Stock) VALUES (@Nombre, @IdCategoria, @Precio, @Stock)";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nombre", nombre);
            cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
            cmd.Parameters.AddWithValue("@Precio", precio);
            cmd.Parameters.AddWithValue("@Stock", stock);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<(Producto producto, string nombreCategoria)>> GetProductosConCategoria()
        {
            var resultado = new List<(Producto, string)>();
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = @"SELECT p.IdProducto, p.Nombre, p.Precio, p.Stock, p.IdCategoria, c.Nombre 
                     FROM Producto p 
                     INNER JOIN Categoria c ON p.IdCategoria = c.IdCategoria";
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                resultado.Add((new Producto
                {
                    IdProducto = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Precio = reader.GetDecimal(2),
                    Stock = reader.GetInt32(3),
                    IdCategoria = reader.GetInt32(4)
                }, reader.GetString(5)));
            }
            return resultado;
        }

        public async Task DeleteProducto(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "DELETE FROM Producto WHERE IdProducto = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateProducto(int id, string nombre, int idCategoria, decimal precio, int stock)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "UPDATE Producto SET Nombre = @Nombre, IdCategoria = @IdCategoria, Precio = @Precio, Stock = @Stock WHERE IdProducto = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nombre", nombre);
            cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
            cmd.Parameters.AddWithValue("@Precio", precio);
            cmd.Parameters.AddWithValue("@Stock", stock);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // VENTA

        public async Task InsertVenta(DateTime fecha, decimal total, decimal iva)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "INSERT INTO venta (Fecha, Total, IVA) VALUES (@Fecha, @Total, @IVA)";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Fecha", fecha);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.Parameters.AddWithValue("@IVA", iva);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Venta>> GetVentas()
        {
            List<Venta> ventas = new List<Venta>();
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "SELECT IdVenta, Fecha, Total, IVA FROM venta";
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                ventas.Add(new Venta
                {
                    IdVenta = reader.GetInt32(0),
                    Fecha = reader.GetDateTime(1),
                    Total = reader.GetDecimal(2),
                    IVA = reader.GetDecimal(3)
                });
            }
            return ventas;
        }

        public async Task DeleteVenta(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "DELETE FROM venta WHERE IdVenta = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task UpdateVenta(int id, decimal total, decimal iva)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            string query = "UPDATE venta SET Total = @Total, IVA = @IVA WHERE IdVenta = @Id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.Parameters.AddWithValue("@IVA", iva);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RestarStockProducto(int idProducto, int cantidadVendida)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // La consulta toma el Stock actual y le resta la cantidad que le pasemos
            string query = "UPDATE Producto SET Stock = Stock - @Cantidad WHERE IdProducto = @Id";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Cantidad", cantidadVendida);
            cmd.Parameters.AddWithValue("@Id", idProducto);

            await cmd.ExecuteNonQueryAsync();
        }

    }
}