using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace IT13_Final.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthUser?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    }

    public sealed class AuthService : IAuthService
    {
        // Adjust the connection string if your SQL instance or DB name differs
        // Try these alternatives if connection fails:
        // "Server=.\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
        // "Server=(local)\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
        // "Server=localhost,1433;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;"
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Integrated Security=SSPI;TrustServerCertificate=True;Connection Timeout=30;";

        public async Task<AuthUser?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hash the incoming password using SHA256 to compare against dbo.tbl_user.pwd_hash
                // If your hashes were generated with SQL Server HASHBYTES on NVARCHAR (e.g., N'password'),
                // use UTF-16LE (Encoding.Unicode) to match SQL Server's byte representation.
                string passwordHex = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                const string sql = @"SELECT TOP 1 u.id, u.pwd_hash, r.name AS RoleName,
                                             COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName
                                      FROM dbo.tbl_users u
                                      JOIN dbo.tbl_roles r ON r.id = u.role_id
                                      WHERE u.email = @email AND ISNULL(u.is_active,1)=1 AND u.archived_at IS NULL";

                await using var cmd = new SqlCommand(sql, connection)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@email", email.Trim());

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null; // user not found
                }

                int userId = reader.GetInt32(0);
                string dbHash = reader.GetString(1);
                string roleName = reader.GetString(2);
                string fullName = reader.FieldCount > 3 && !reader.IsDBNull(3) ? reader.GetString(3) : email;

                // Compare hashes (case-insensitive)
                if (!string.Equals(dbHash, passwordHex, StringComparison.OrdinalIgnoreCase))
                {
                    return null; // invalid password
                }

                return new AuthUser
                {
                    Id = userId,
                    Email = email,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                    Role = roleName.ToLowerInvariant()
                };
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // Log the SQL exception for debugging
                System.Diagnostics.Debug.WriteLine($"SQL Exception during authentication: {sqlEx.Message}");
                
                // Check if it's a connection timeout or connection issue
                if (sqlEx.Number == -2 || sqlEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Unable to connect to the database. Please check if SQL Server is running and accessible.", sqlEx);
                }
                
                // Re-throw other SQL exceptions
                throw;
            }
            catch (Exception ex)
            {
                // Log other exceptions
                System.Diagnostics.Debug.WriteLine($"Exception during authentication: {ex.Message}");
                throw;
            }
        }
    }
}


