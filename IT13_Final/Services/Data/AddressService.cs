using System.Data;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace IT13_Final.Services.Data
{
    public interface IAddressService
    {
        Task<AddressModel?> GetAddressAsync(int userId, CancellationToken ct = default);
        Task SaveAddressAsync(int userId, AddressModel address, CancellationToken ct = default);
    }

    public sealed class AddressService : IAddressService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<AddressModel?> GetAddressAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"SELECT TOP 1 id, street, city, province, zip
                                  FROM dbo.tbl_addresses
                                  WHERE user_id = @userId AND archived_at IS NULL
                                  ORDER BY created_at DESC";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return new AddressModel
            {
                Id = reader.GetInt32(0),
                Street = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                City = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Province = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Zip = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            };
        }

        public async Task SaveAddressAsync(int userId, AddressModel address, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            if (address.Id > 0)
            {
                // Update existing
                const string updateSql = @"UPDATE dbo.tbl_addresses
                                           SET street = @street, city = @city, province = @province, zip = @zip, updated_at = SYSUTCDATETIME()
                                           WHERE id = @id AND user_id = @userId";

                await using var cmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
                cmd.Parameters.AddWithValue("@id", address.Id);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@street", (object?)address.Street ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@city", (object?)address.City ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@province", (object?)address.Province ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@zip", (object?)address.Zip ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            else
            {
                // Insert new
                const string insertSql = @"INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
                                           VALUES (@userId, @street, @city, @province, @zip, SYSUTCDATETIME())";

                await using var cmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@street", (object?)address.Street ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@city", (object?)address.City ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@province", (object?)address.Province ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@zip", (object?)address.Zip ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }
    }

    public sealed class AddressModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Street Address is required")]
        public string Street { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Zip Code is required")]
        public string Zip { get; set; } = string.Empty;
    }
}

