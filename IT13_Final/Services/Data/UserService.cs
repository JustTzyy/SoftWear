using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IT13_Final.Services.Data
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException() : base("Email address already exists.")
        {
        }

        public EmailAlreadyExistsException(string message) : base(message)
        {
        }
    }

    public class AdminUserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public interface IUserService
    {
        Task UpdatePersonalInfoAsync(int userId, PersonalInfoModel info, CancellationToken ct = default);
        Task<PersonalInfoModel?> GetPersonalInfoAsync(int userId, CancellationToken ct = default);
        Task<bool> UpdateEmailAsync(int userId, string newEmail, CancellationToken ct = default);
        Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default);
        Task<List<AdminUserModel>> GetAdminUsersAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetAdminUsersCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<UserDetailsModel?> GetUserDetailsAsync(int userId, CancellationToken ct = default);
        Task<bool> MustChangePasswordAsync(int userId, CancellationToken ct = default);
        Task<int?> CreateAdminAsync(string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> UpdateAdminAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> ArchiveAdminAsync(int userId, CancellationToken ct = default);
        Task<List<ArchivedAdminUserModel>> GetArchivedAdminUsersAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedAdminUsersCountAsync(string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreAdminAsync(int userId, CancellationToken ct = default);
        Task<UserDetailsModel?> GetArchivedUserDetailsAsync(int userId, CancellationToken ct = default);
        Task<List<AdminUserModel>> GetSellerUsersAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetSellerUsersCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default);
        Task<int?> CreateSellerAsync(int adminId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> UpdateSellerAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> ArchiveSellerAsync(int userId, CancellationToken ct = default);
        Task<List<ArchivedAdminUserModel>> GetArchivedSellerUsersAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedSellerUsersCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreSellerAsync(int userId, CancellationToken ct = default);
        Task<List<AdminUserModel>> GetAccountingUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetAccountingUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<int?> CreateAccountingAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> UpdateAccountingAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> ArchiveAccountingAsync(int userId, CancellationToken ct = default);
        Task<List<ArchivedAdminUserModel>> GetArchivedAccountingUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedAccountingUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreAccountingAsync(int userId, CancellationToken ct = default);
        Task<List<AdminUserModel>> GetCashierUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetCashierUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<int?> CreateCashierAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> UpdateCashierAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> ArchiveCashierAsync(int userId, CancellationToken ct = default);
        Task<List<ArchivedAdminUserModel>> GetArchivedCashierUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedCashierUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreCashierAsync(int userId, CancellationToken ct = default);
        Task<List<AdminUserModel>> GetStockClerkUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetStockClerkUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<int?> CreateStockClerkAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> UpdateStockClerkAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default);
        Task<bool> ArchiveStockClerkAsync(int userId, CancellationToken ct = default);
        Task<List<ArchivedAdminUserModel>> GetArchivedStockClerkUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedStockClerkUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreStockClerkAsync(int userId, CancellationToken ct = default);
        Task<bool> CreatePermissionRequestAsync(int userId, string requestType, string requestDataJson, CancellationToken ct = default);
        Task<List<PermissionRequestModel>> GetPermissionRequestsAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetPermissionRequestsCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default);
        Task<PermissionRequestDetailsModel?> GetPermissionRequestDetailsAsync(int userId, CancellationToken ct = default);
        Task<bool> ApprovePermissionRequestAsync(int userId, CancellationToken ct = default);
        Task<bool> RejectPermissionRequestAsync(int userId, CancellationToken ct = default);
    }

    public class ArchivedAdminUserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ArchivedDate { get; set; }
    }

    public class UserDetailsModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly? Birthday { get; set; }
        public int? Age { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
    }

    public sealed class UserService : IUserService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<PersonalInfoModel?> GetPersonalInfoAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"SELECT fname, mname, lname, contact_no, bday, age, sex
                                  FROM dbo.tbl_users
                                  WHERE id = @userId AND archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            // Handle sex: convert to string (0=Male, 1=Female)
            // Sex might be stored as bit, tinyint, int, or string - handle all cases
            string sexValue = string.Empty;
            if (!reader.IsDBNull(6))
            {
                var sexObj = reader.GetValue(6);
                // Handle different data types
                if (sexObj is bool sexBool)
                {
                    sexValue = sexBool ? "Female" : "Male"; // true = 1 = Female, false = 0 = Male
                }
                else if (sexObj is byte sexByte)
                {
                    sexValue = sexByte == 0 ? "Male" : "Female";
                }
                else if (sexObj is int sexInt)
                {
                    sexValue = sexInt == 0 ? "Male" : "Female";
                }
                else if (sexObj is short sexShort)
                {
                    sexValue = sexShort == 0 ? "Male" : "Female";
                }
                else if (sexObj is string sexStr)
                {
                    var trimmed = sexStr.Trim();
                    if (trimmed == "Male" || trimmed == "Female")
                        sexValue = trimmed;
                    else if (trimmed == "0" || trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Male";
                    else if (trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Female";
                }
                else
                {
                    // Try to convert to int
                    try
                    {
                        int val = Convert.ToInt32(sexObj);
                        sexValue = val == 0 ? "Male" : "Female";
                    }
                    catch
                    {
                        sexValue = string.Empty;
                    }
                }
            }

            return new PersonalInfoModel
            {
                FirstName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                MiddleName = reader.IsDBNull(1) ? null : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Contact = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Birthday = reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
                Age = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                Sex = sexValue  // Direct assignment, no null check needed
            };
        }

        public async Task UpdatePersonalInfoAsync(int userId, PersonalInfoModel info, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"UPDATE dbo.tbl_users
                                  SET fname = @fname,
                                      mname = @mname,
                                      lname = @lname,
                                      contact_no = @contact,
                                      bday = @bday,
                                      age = @age,
                                      sex = @sex,
                                      updated_at = SYSUTCDATETIME()
                                  WHERE id = @userId AND archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@fname", (object?)info.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mname", (object?)info.MiddleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lname", (object?)info.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contact", (object?)info.Contact ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bday", info.Birthday.HasValue ? (object)info.Birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            cmd.Parameters.AddWithValue("@age", info.Age.HasValue ? (object)info.Age.Value : DBNull.Value);
            
            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(info.Sex))
            {
                sexValue = info.Sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }
            cmd.Parameters.AddWithValue("@sex", sexValue);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<bool> UpdateEmailAsync(int userId, string newEmail, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First check if the email is already taken by another user
            const string checkSql = @"SELECT COUNT(*) 
                                      FROM dbo.tbl_users 
                                      WHERE email = @email AND id != @userId AND archived_at IS NULL";
            
            await using var checkCmd = new SqlCommand(checkSql, conn) { CommandType = CommandType.Text };
            checkCmd.Parameters.AddWithValue("@email", newEmail.Trim());
            checkCmd.Parameters.AddWithValue("@userId", userId);
            
            var emailExists = (int)(await checkCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            if (emailExists)
            {
                return false; // Email already in use
            }

            // Update the email
            const string updateSql = @"UPDATE dbo.tbl_users
                                       SET email = @email,
                                           updated_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", newEmail.Trim());

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First verify the current password
            const string verifySql = @"SELECT pwd_hash 
                                       FROM dbo.tbl_users 
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var verifyCmd = new SqlCommand(verifySql, conn) { CommandType = CommandType.Text };
            verifyCmd.Parameters.AddWithValue("@userId", userId);

            string? storedHash = null;
            await using (var reader = await verifyCmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct) && !reader.IsDBNull(0))
                {
                    storedHash = reader.GetString(0);
                }
            }

            if (string.IsNullOrEmpty(storedHash))
            {
                return false; // User not found
            }

            // Hash the current password to compare
            string currentPasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(currentPassword)));
            
            // Verify current password matches
            if (!string.Equals(currentPasswordHash, storedHash, StringComparison.OrdinalIgnoreCase))
            {
                return false; // Current password is incorrect
            }

            // Hash the new password
            string newPasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(newPassword)));

            // Update the password and set must_change_pw to 0
            const string updateSql = @"UPDATE dbo.tbl_users
                                       SET pwd_hash = @pwdHash,
                                           must_change_pw = 0,
                                           updated_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@pwdHash", newPasswordHash);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<AdminUserModel>> GetAdminUsersAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName,
                               u.email,
                               u.created_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'admin' AND u.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.created_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<AdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new AdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetAdminUsersCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'admin' AND u.archived_at IS NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<UserDetailsModel?> GetUserDetailsAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get user details
            const string sql = @"SELECT u.id,
                                        COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName,
                                        u.email,
                                        u.bday,
                                        u.age,
                                        u.sex,
                                        u.contact_no,
                                        CASE WHEN ISNULL(u.is_active, 1) = 1 THEN 'ACTIVE' ELSE 'INACTIVE' END AS Status,
                                        u.created_at
                                 FROM dbo.tbl_users u
                                 WHERE u.id = @userId AND u.archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            // Handle sex conversion
            string sexValue = string.Empty;
            if (!reader.IsDBNull(5))
            {
                var sexObj = reader.GetValue(5);
                if (sexObj is bool sexBool)
                {
                    sexValue = sexBool ? "Female" : "Male";
                }
                else if (sexObj is byte sexByte)
                {
                    sexValue = sexByte == 0 ? "Male" : "Female";
                }
                else if (sexObj is int sexInt)
                {
                    sexValue = sexInt == 0 ? "Male" : "Female";
                }
                else if (sexObj is string sexStr)
                {
                    var trimmed = sexStr.Trim();
                    if (trimmed == "Male" || trimmed == "Female")
                        sexValue = trimmed;
                    else if (trimmed == "0" || trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Male";
                    else if (trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Female";
                }
                else
                {
                    try
                    {
                        int val = Convert.ToInt32(sexObj);
                        sexValue = val == 0 ? "Male" : "Female";
                    }
                    catch
                    {
                        sexValue = string.Empty;
                    }
                }
            }

            var details = new UserDetailsModel
            {
                Id = reader.GetInt32(0),
                FullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Birthday = reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3)),
                Age = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                Sex = sexValue,
                Contact = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Status = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                DateCreated = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8)
            };

            reader.Close();

            // Get address
            const string addressSql = @"SELECT TOP 1 street, city, province, zip
                                        FROM dbo.tbl_addresses
                                        WHERE user_id = @userId AND archived_at IS NULL
                                        ORDER BY created_at DESC";

            await using var addressCmd = new SqlCommand(addressSql, conn) { CommandType = CommandType.Text };
            addressCmd.Parameters.AddWithValue("@userId", userId);

            await using var addressReader = await addressCmd.ExecuteReaderAsync(ct);
            if (await addressReader.ReadAsync(ct))
            {
                details.Street = addressReader.IsDBNull(0) ? string.Empty : addressReader.GetString(0);
                details.City = addressReader.IsDBNull(1) ? string.Empty : addressReader.GetString(1);
                details.Province = addressReader.IsDBNull(2) ? string.Empty : addressReader.GetString(2);
                details.Zip = addressReader.IsDBNull(3) ? string.Empty : addressReader.GetString(3);
            }

            return details;
        }

        public async Task<UserDetailsModel?> GetArchivedUserDetailsAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Get archived user details
            const string sql = @"SELECT u.id,
                                        COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName,
                                        u.email,
                                        u.bday,
                                        u.age,
                                        u.sex,
                                        u.contact_no,
                                        CASE WHEN ISNULL(u.is_active, 1) = 1 THEN 'ACTIVE' ELSE 'INACTIVE' END AS Status,
                                        u.created_at
                                 FROM dbo.tbl_users u
                                 WHERE u.id = @userId AND u.archived_at IS NOT NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            // Handle sex conversion
            string sexValue = string.Empty;
            if (!reader.IsDBNull(5))
            {
                var sexObj = reader.GetValue(5);
                if (sexObj is bool sexBool)
                {
                    sexValue = sexBool ? "Female" : "Male";
                }
                else if (sexObj is byte sexByte)
                {
                    sexValue = sexByte == 0 ? "Male" : "Female";
                }
                else if (sexObj is int sexInt)
                {
                    sexValue = sexInt == 0 ? "Male" : "Female";
                }
                else if (sexObj is string sexStr)
                {
                    var trimmed = sexStr.Trim();
                    if (trimmed == "Male" || trimmed == "Female")
                        sexValue = trimmed;
                    else if (trimmed == "0" || trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Male";
                    else if (trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Female";
                }
                else
                {
                    try
                    {
                        int val = Convert.ToInt32(sexObj);
                        sexValue = val == 0 ? "Male" : "Female";
                    }
                    catch
                    {
                        sexValue = string.Empty;
                    }
                }
            }

            var details = new UserDetailsModel
            {
                Id = reader.GetInt32(0),
                FullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Birthday = reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3)),
                Age = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                Sex = sexValue,
                Contact = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Status = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                DateCreated = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8)
            };

            reader.Close();

            // Get address (including archived addresses for archived users)
            const string addressSql = @"SELECT TOP 1 street, city, province, zip
                                        FROM dbo.tbl_addresses
                                        WHERE user_id = @userId
                                        ORDER BY created_at DESC";

            await using var addressCmd = new SqlCommand(addressSql, conn) { CommandType = CommandType.Text };
            addressCmd.Parameters.AddWithValue("@userId", userId);

            await using var addressReader = await addressCmd.ExecuteReaderAsync(ct);
            if (await addressReader.ReadAsync(ct))
            {
                details.Street = addressReader.IsDBNull(0) ? string.Empty : addressReader.GetString(0);
                details.City = addressReader.IsDBNull(1) ? string.Empty : addressReader.GetString(1);
                details.Province = addressReader.IsDBNull(2) ? string.Empty : addressReader.GetString(2);
                details.Zip = addressReader.IsDBNull(3) ? string.Empty : addressReader.GetString(3);
            }

            return details;
        }

        public async Task<bool> MustChangePasswordAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"SELECT must_change_pw
                                 FROM dbo.tbl_users
                                 WHERE id = @userId AND archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = await cmd.ExecuteScalarAsync(ct);
            if (result == null || result == DBNull.Value)
            {
                return false;
            }

            // Handle different data types for must_change_pw (bit, tinyint, int)
            if (result is bool boolValue)
            {
                return boolValue;
            }
            else if (result is byte byteValue)
            {
                return byteValue != 0;
            }
            else if (result is int intValue)
            {
                return intValue != 0;
            }
            else if (result is short shortValue)
            {
                return shortValue != 0;
            }

            return false;
        }

        public async Task<int?> CreateAdminAsync(string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the admin role_id
            const string roleSql = @"SELECT id FROM dbo.tbl_roles WHERE LOWER(name) = 'admin'";
            await using var roleCmd = new SqlCommand(roleSql, conn) { CommandType = CommandType.Text };
            var roleResult = await roleCmd.ExecuteScalarAsync(ct);
            
            if (roleResult == null || roleResult == DBNull.Value)
            {
                throw new InvalidOperationException("Admin role not found in database.");
            }

            int roleId = Convert.ToInt32(roleResult);

            // Check if email already exists
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Hash the password using SHA256 (same method as UpdatePasswordAsync)
            string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Insert new admin user
            const string insertSql = @"INSERT INTO dbo.tbl_users 
                                        (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, is_active, must_change_pw, role_id, created_at)
                                        VALUES 
                                        (@email, @pwd_hash, @name, @fname, @mname, @lname, @contact_no, @bday, @age, @sex, @is_active, @must_change_pw, @role_id, SYSUTCDATETIME());
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
            insertCmd.Parameters.AddWithValue("@email", email.Trim());
            insertCmd.Parameters.AddWithValue("@pwd_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@sex", sexValue);
            insertCmd.Parameters.AddWithValue("@is_active", 1); // Admin users are active by default
            insertCmd.Parameters.AddWithValue("@must_change_pw", 1); // New admins must change password on first login
            insertCmd.Parameters.AddWithValue("@role_id", roleId);

            var newUserId = await insertCmd.ExecuteScalarAsync(ct);
            return newUserId != null && newUserId != DBNull.Value ? Convert.ToInt32(newUserId) : null;
        }

        public async Task<bool> UpdateAdminAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if email already exists (excluding current user)
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            checkEmailCmd.Parameters.AddWithValue("@userId", userId);
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Update admin user
            const string updateSql = @"UPDATE dbo.tbl_users 
                                       SET email = @email,
                                           name = @name,
                                           fname = @fname,
                                           mname = @mname,
                                           lname = @lname,
                                           contact_no = @contact_no,
                                           bday = @bday,
                                           age = @age,
                                           sex = @sex,
                                           updated_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", email.Trim());
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveAdminAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Archive the admin user by setting archived_at
            const string archiveSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var archiveCmd = new SqlCommand(archiveSql, conn) { CommandType = CommandType.Text };
            archiveCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await archiveCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedAdminUserModel>> GetArchivedAdminUsersAsync(string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.archived_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'admin' AND u.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.archived_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<ArchivedAdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new ArchivedAdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ArchivedDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetArchivedAdminUsersCountAsync(string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'admin' AND u.archived_at IS NOT NULL";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreAdminAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Restore the admin user by clearing archived_at
            const string restoreSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = NULL
                                       WHERE id = @userId AND archived_at IS NOT NULL";

            await using var restoreCmd = new SqlCommand(restoreSql, conn) { CommandType = CommandType.Text };
            restoreCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await restoreCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<AdminUserModel>> GetSellerUsersAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS FullName,
                               u.email,
                               u.created_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' AND u.archived_at IS NULL AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.created_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<AdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new AdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetSellerUsersCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' AND u.archived_at IS NULL AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<int?> CreateSellerAsync(int adminId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the seller role_id
            const string roleSql = @"SELECT id FROM dbo.tbl_roles WHERE LOWER(name) = 'seller'";
            await using var roleCmd = new SqlCommand(roleSql, conn) { CommandType = CommandType.Text };
            var roleResult = await roleCmd.ExecuteScalarAsync(ct);
            
            if (roleResult == null || roleResult == DBNull.Value)
            {
                throw new InvalidOperationException("Seller role not found in database.");
            }

            int roleId = Convert.ToInt32(roleResult);

            // Check if email already exists
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Hash the password using SHA256 (same method as UpdatePasswordAsync)
            string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Insert new seller user with user_id linking to admin
            const string insertSql = @"INSERT INTO dbo.tbl_users 
                                        (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, is_active, must_change_pw, role_id, user_id, created_at)
                                        VALUES 
                                        (@email, @pwd_hash, @name, @fname, @mname, @lname, @contact_no, @bday, @age, @sex, @is_active, @must_change_pw, @role_id, @user_id, SYSUTCDATETIME());
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
            insertCmd.Parameters.AddWithValue("@email", email.Trim());
            insertCmd.Parameters.AddWithValue("@pwd_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@sex", sexValue);
            insertCmd.Parameters.AddWithValue("@is_active", 1); // Seller users are active by default
            insertCmd.Parameters.AddWithValue("@must_change_pw", 1); // New sellers must change password on first login
            insertCmd.Parameters.AddWithValue("@role_id", roleId);
            insertCmd.Parameters.AddWithValue("@user_id", adminId); // Link seller to admin

            var newUserId = await insertCmd.ExecuteScalarAsync(ct);
            return newUserId != null && newUserId != DBNull.Value ? Convert.ToInt32(newUserId) : null;
        }

        public async Task<bool> UpdateSellerAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if email already exists (excluding current user)
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            checkEmailCmd.Parameters.AddWithValue("@userId", userId);
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Update seller user
            const string updateSql = @"UPDATE dbo.tbl_users 
                                     SET email = @email,
                                         name = @name,
                                         fname = @fname,
                                         mname = @mname,
                                         lname = @lname,
                                         contact_no = @contact_no,
                                         bday = @bday,
                                         age = @age,
                                         sex = @sex,
                                         updated_at = SYSUTCDATETIME()
                                     WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", email.Trim());
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveSellerAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Archive the seller user by setting archived_at
            const string archiveSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var archiveCmd = new SqlCommand(archiveSql, conn) { CommandType = CommandType.Text };
            archiveCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await archiveCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedAdminUserModel>> GetArchivedSellerUsersAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.archived_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' AND u.archived_at IS NOT NULL AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.archived_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<ArchivedAdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new ArchivedAdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ArchivedDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetArchivedSellerUsersCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' AND u.archived_at IS NOT NULL AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreSellerAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Restore the seller user by clearing archived_at
            const string restoreSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = NULL
                                       WHERE id = @userId AND archived_at IS NOT NULL";

            await using var restoreCmd = new SqlCommand(restoreSql, conn) { CommandType = CommandType.Text };
            restoreCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await restoreCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<AdminUserModel>> GetAccountingUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.created_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'accounting' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.created_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<AdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new AdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetAccountingUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'accounting' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<int?> CreateAccountingAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the accounting role_id
            const string roleSql = @"SELECT id FROM dbo.tbl_roles WHERE LOWER(name) = 'accounting'";
            await using var roleCmd = new SqlCommand(roleSql, conn) { CommandType = CommandType.Text };
            var roleResult = await roleCmd.ExecuteScalarAsync(ct);
            
            if (roleResult == null || roleResult == DBNull.Value)
            {
                throw new InvalidOperationException("Accounting role not found in database.");
            }

            int roleId = Convert.ToInt32(roleResult);

            // Check if email already exists
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Hash the password using SHA256 (same method as UpdatePasswordAsync)
            string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Insert new accounting user with user_id (seller ID)
            const string insertSql = @"INSERT INTO dbo.tbl_users 
                                        (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, is_active, must_change_pw, role_id, user_id, created_at)
                                        VALUES 
                                        (@email, @pwd_hash, @name, @fname, @mname, @lname, @contact_no, @bday, @age, @sex, @is_active, @must_change_pw, @role_id, @user_id, SYSUTCDATETIME());
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
            insertCmd.Parameters.AddWithValue("@email", email.Trim());
            insertCmd.Parameters.AddWithValue("@pwd_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@sex", sexValue);
            insertCmd.Parameters.AddWithValue("@is_active", 1); // Accounting users are active by default
            insertCmd.Parameters.AddWithValue("@must_change_pw", 1); // New accounting users must change password on first login
            insertCmd.Parameters.AddWithValue("@role_id", roleId);
            insertCmd.Parameters.AddWithValue("@user_id", sellerUserId); // Set user_id to the seller's ID

            var newUserId = await insertCmd.ExecuteScalarAsync(ct);
            return newUserId != null && newUserId != DBNull.Value ? Convert.ToInt32(newUserId) : null;
        }

        public async Task<bool> UpdateAccountingAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if email already exists (excluding current user)
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            checkEmailCmd.Parameters.AddWithValue("@userId", userId);
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Update accounting user
            const string updateSql = @"UPDATE dbo.tbl_users 
                                     SET email = @email,
                                         name = @name,
                                         fname = @fname,
                                         mname = @mname,
                                         lname = @lname,
                                         contact_no = @contact_no,
                                         bday = @bday,
                                         age = @age,
                                         sex = @sex,
                                         updated_at = SYSUTCDATETIME()
                                     WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", email.Trim());
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveAccountingAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Archive the accounting user by setting archived_at
            const string archiveSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var archiveCmd = new SqlCommand(archiveSql, conn) { CommandType = CommandType.Text };
            archiveCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await archiveCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedAdminUserModel>> GetArchivedAccountingUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.archived_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'accounting' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.archived_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<ArchivedAdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new ArchivedAdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ArchivedDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetArchivedAccountingUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'accounting' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreAccountingAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Restore the accounting user by clearing archived_at
            const string restoreSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = NULL
                                       WHERE id = @userId AND archived_at IS NOT NULL";

            await using var restoreCmd = new SqlCommand(restoreSql, conn) { CommandType = CommandType.Text };
            restoreCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await restoreCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<AdminUserModel>> GetCashierUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.created_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'cashier' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.created_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<AdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new AdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetCashierUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'cashier' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<int?> CreateCashierAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the cashier role_id
            const string roleSql = @"SELECT id FROM dbo.tbl_roles WHERE LOWER(name) = 'cashier'";
            await using var roleCmd = new SqlCommand(roleSql, conn) { CommandType = CommandType.Text };
            var roleResult = await roleCmd.ExecuteScalarAsync(ct);
            
            if (roleResult == null || roleResult == DBNull.Value)
            {
                throw new InvalidOperationException("Cashier role not found in database.");
            }

            int roleId = Convert.ToInt32(roleResult);

            // Check if email already exists
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Hash the password using SHA256 (same method as UpdatePasswordAsync)
            string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Insert new cashier user with user_id (seller ID)
            const string insertSql = @"INSERT INTO dbo.tbl_users 
                                        (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, is_active, must_change_pw, role_id, user_id, created_at)
                                        VALUES 
                                        (@email, @pwd_hash, @name, @fname, @mname, @lname, @contact_no, @bday, @age, @sex, @is_active, @must_change_pw, @role_id, @user_id, SYSUTCDATETIME());
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
            insertCmd.Parameters.AddWithValue("@email", email.Trim());
            insertCmd.Parameters.AddWithValue("@pwd_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@sex", sexValue);
            insertCmd.Parameters.AddWithValue("@is_active", 1); // Cashier users are active by default
            insertCmd.Parameters.AddWithValue("@must_change_pw", 1); // New cashier users must change password on first login
            insertCmd.Parameters.AddWithValue("@role_id", roleId);
            insertCmd.Parameters.AddWithValue("@user_id", sellerUserId); // Set user_id to the seller's ID

            var newUserId = await insertCmd.ExecuteScalarAsync(ct);
            return newUserId != null && newUserId != DBNull.Value ? Convert.ToInt32(newUserId) : null;
        }

        public async Task<bool> UpdateCashierAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if email already exists (excluding current user)
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            checkEmailCmd.Parameters.AddWithValue("@userId", userId);
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Update cashier user
            const string updateSql = @"UPDATE dbo.tbl_users 
                                     SET email = @email,
                                         name = @name,
                                         fname = @fname,
                                         mname = @mname,
                                         lname = @lname,
                                         contact_no = @contact_no,
                                         bday = @bday,
                                         age = @age,
                                         sex = @sex,
                                         updated_at = SYSUTCDATETIME()
                                     WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", email.Trim());
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveCashierAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Archive the cashier user by setting archived_at
            const string archiveSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var archiveCmd = new SqlCommand(archiveSql, conn) { CommandType = CommandType.Text };
            archiveCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await archiveCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedAdminUserModel>> GetArchivedCashierUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.archived_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'cashier' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.archived_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<ArchivedAdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new ArchivedAdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ArchivedDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetArchivedCashierUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'cashier' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreCashierAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Restore the cashier user by clearing archived_at
            const string restoreSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = NULL
                                       WHERE id = @userId AND archived_at IS NOT NULL";

            await using var restoreCmd = new SqlCommand(restoreSql, conn) { CommandType = CommandType.Text };
            restoreCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await restoreCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<AdminUserModel>> GetStockClerkUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.created_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'stockclerk' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.created_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<AdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new AdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DateCreated = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetStockClerkUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'stockclerk' AND u.archived_at IS NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<int?> CreateStockClerkAsync(int sellerUserId, string email, string password, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the stock clerk role_id
            const string roleSql = @"SELECT id FROM dbo.tbl_roles WHERE LOWER(name) = 'stockclerk'";
            await using var roleCmd = new SqlCommand(roleSql, conn) { CommandType = CommandType.Text };
            var roleResult = await roleCmd.ExecuteScalarAsync(ct);
            
            if (roleResult == null || roleResult == DBNull.Value)
            {
                throw new InvalidOperationException("Stock Clerk role not found in database.");
            }

            int roleId = Convert.ToInt32(roleResult);

            // Check if email already exists
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Hash the password using SHA256 (same method as UpdatePasswordAsync)
            string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Insert new stock clerk user with user_id (seller ID)
            const string insertSql = @"INSERT INTO dbo.tbl_users 
                                        (email, pwd_hash, name, fname, mname, lname, contact_no, bday, age, sex, is_active, must_change_pw, role_id, user_id, created_at)
                                        VALUES 
                                        (@email, @pwd_hash, @name, @fname, @mname, @lname, @contact_no, @bday, @age, @sex, @is_active, @must_change_pw, @role_id, @user_id, SYSUTCDATETIME());
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var insertCmd = new SqlCommand(insertSql, conn) { CommandType = CommandType.Text };
            insertCmd.Parameters.AddWithValue("@email", email.Trim());
            insertCmd.Parameters.AddWithValue("@pwd_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@sex", sexValue);
            insertCmd.Parameters.AddWithValue("@is_active", 1); // Stock clerk users are active by default
            insertCmd.Parameters.AddWithValue("@must_change_pw", 1); // New stock clerk users must change password on first login
            insertCmd.Parameters.AddWithValue("@role_id", roleId);
            insertCmd.Parameters.AddWithValue("@user_id", sellerUserId); // Set user_id to the seller's ID

            var newUserId = await insertCmd.ExecuteScalarAsync(ct);
            return newUserId != null && newUserId != DBNull.Value ? Convert.ToInt32(newUserId) : null;
        }

        public async Task<bool> UpdateStockClerkAsync(int userId, string email, string firstName, string? middleName, string lastName, string? contact, DateOnly? birthday, int? age, string? sex, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if email already exists (excluding current user)
            const string checkEmailSql = @"SELECT COUNT(*) FROM dbo.tbl_users WHERE email = @email AND id != @userId AND archived_at IS NULL";
            await using var checkEmailCmd = new SqlCommand(checkEmailSql, conn) { CommandType = CommandType.Text };
            checkEmailCmd.Parameters.AddWithValue("@email", email.Trim());
            checkEmailCmd.Parameters.AddWithValue("@userId", userId);
            var emailExists = (int)(await checkEmailCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
            
            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            // Build full name
            string fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                fullName = $"{firstName} {middleName} {lastName}".Trim();
            }

            // Convert sex string to integer (0=Male, 1=Female, or NULL)
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(sex))
            {
                sexValue = sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }

            // Update stock clerk user
            const string updateSql = @"UPDATE dbo.tbl_users 
                                     SET email = @email,
                                         name = @name,
                                         fname = @fname,
                                         mname = @mname,
                                         lname = @lname,
                                         contact_no = @contact_no,
                                         bday = @bday,
                                         age = @age,
                                         sex = @sex,
                                         updated_at = SYSUTCDATETIME()
                                     WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@email", email.Trim());
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@fname", (object?)firstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)middleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)lastName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@contact_no", (object?)contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", birthday.HasValue ? (object)birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)age ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> ArchiveStockClerkAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Archive the stock clerk user by setting archived_at
            const string archiveSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = SYSUTCDATETIME()
                                       WHERE id = @userId AND archived_at IS NULL";

            await using var archiveCmd = new SqlCommand(archiveSql, conn) { CommandType = CommandType.Text };
            archiveCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await archiveCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<ArchivedAdminUserModel>> GetArchivedStockClerkUsersAsync(int sellerUserId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.archived_at
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'stockclerk' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.archived_at DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var users = new List<ArchivedAdminUserModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(new ArchivedAdminUserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ArchivedDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                });
            }

            return users;
        }

        public async Task<int> GetArchivedStockClerkUsersCountAsync(int sellerUserId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'stockclerk' AND u.archived_at IS NOT NULL AND u.user_id = @sellerUserId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@sellerUserId", sellerUserId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> RestoreStockClerkAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Restore the stock clerk user by clearing archived_at
            const string restoreSql = @"UPDATE dbo.tbl_users 
                                       SET archived_at = NULL
                                       WHERE id = @userId AND archived_at IS NOT NULL";

            await using var restoreCmd = new SqlCommand(restoreSql, conn) { CommandType = CommandType.Text };
            restoreCmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await restoreCmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> CreatePermissionRequestAsync(int userId, string requestType, string requestDataJson, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // Check if there's an existing pending request
            const string getExistingSql = @"SELECT permission_request_type, permission_request_data
                                           FROM dbo.tbl_users
                                           WHERE id = @userId 
                                           AND permission_request_status = 'pending'
                                           AND archived_at IS NULL";

            await using var getCmd = new SqlCommand(getExistingSql, conn) { CommandType = CommandType.Text };
            getCmd.Parameters.AddWithValue("@userId", userId);

            string? existingRequestType = null;
            string? existingRequestDataJson = null;

            await using var reader = await getCmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                existingRequestType = reader.IsDBNull(0) ? null : reader.GetString(0);
                existingRequestDataJson = reader.IsDBNull(1) ? null : reader.GetString(1);
            }
            reader.Close();

            // Merge requests if there's an existing pending request
            string finalRequestType;
            string finalRequestDataJson;

            if (!string.IsNullOrEmpty(existingRequestType) && !string.IsNullOrEmpty(existingRequestDataJson))
            {
                // There's an existing request - merge them
                var combined = new CombinedRequestData();

                // Parse existing request
                if (existingRequestType == "personal_info")
                {
                    try
                    {
                        combined.PersonalInfo = System.Text.Json.JsonSerializer.Deserialize<PersonalInfoRequestData>(existingRequestDataJson);
                    }
                    catch { }
                }
                else if (existingRequestType == "address")
                {
                    try
                    {
                        combined.Address = System.Text.Json.JsonSerializer.Deserialize<AddressRequestData>(existingRequestDataJson);
                    }
                    catch { }
                }
                else if (existingRequestType == "combined")
                {
                    // Already a combined request - deserialize and update
                    try
                    {
                        combined = System.Text.Json.JsonSerializer.Deserialize<CombinedRequestData>(existingRequestDataJson) ?? new CombinedRequestData();
                    }
                    catch { }
                }

                // Add the new request
                if (requestType == "personal_info")
                {
                    try
                    {
                        combined.PersonalInfo = System.Text.Json.JsonSerializer.Deserialize<PersonalInfoRequestData>(requestDataJson);
                    }
                    catch { }
                }
                else if (requestType == "address")
                {
                    try
                    {
                        combined.Address = System.Text.Json.JsonSerializer.Deserialize<AddressRequestData>(requestDataJson);
                    }
                    catch { }
                }

                finalRequestType = "combined";
                finalRequestDataJson = System.Text.Json.JsonSerializer.Serialize(combined);
            }
            else
            {
                // No existing request - use the new one as-is
                finalRequestType = requestType;
                finalRequestDataJson = requestDataJson;
            }

            const string sql = @"UPDATE dbo.tbl_users
                                SET permission_request_type = @requestType,
                                    permission_request_data = @requestData,
                                    permission_request_status = 'pending',
                                    permission_request_date = SYSUTCDATETIME()
                                WHERE id = @userId AND archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@requestType", finalRequestType);
            cmd.Parameters.AddWithValue("@requestData", (object?)finalRequestDataJson ?? DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<List<PermissionRequestModel>> GetPermissionRequestsAsync(int adminId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT u.id,
                               COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                               u.email,
                               u.permission_request_type,
                               u.permission_request_date
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' 
                        AND u.permission_request_status = 'pending'
                        AND u.archived_at IS NULL
                        AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            sql += " ORDER BY u.permission_request_date DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var offset = (page - 1) * pageSize;
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var requests = new List<PermissionRequestModel>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                requests.Add(new PermissionRequestModel
                {
                    UserId = reader.GetInt32(0),
                    UserName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    UserEmail = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    RequestType = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    RequestedDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4)
                });
            }

            return requests;
        }

        public async Task<int> GetPermissionRequestsCountAsync(int adminId, string? searchTerm = null, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"SELECT COUNT(*)
                        FROM dbo.tbl_users u
                        JOIN dbo.tbl_roles r ON r.id = u.role_id
                        WHERE LOWER(r.name) = 'seller' 
                        AND u.permission_request_status = 'pending'
                        AND u.archived_at IS NULL
                        AND u.user_id = @AdminId";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (u.email LIKE @search OR u.fname LIKE @search OR u.lname LIKE @search OR u.name LIKE @search)";
            }

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<PermissionRequestDetailsModel?> GetPermissionRequestDetailsAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"SELECT u.id,
                                       COALESCE(u.name, (LTRIM(RTRIM(ISNULL(u.fname,''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname,''))))) AS name,
                                       u.email,
                                       u.fname,
                                       u.mname,
                                       u.lname,
                                       u.contact_no,
                                       u.bday,
                                       u.age,
                                       u.sex,
                                       u.permission_request_type,
                                       u.permission_request_data,
                                       u.permission_request_date
                                FROM dbo.tbl_users u
                                WHERE u.id = @userId 
                                AND u.permission_request_status = 'pending'
                                AND u.archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            // Handle sex conversion
            string sexValue = string.Empty;
            if (!reader.IsDBNull(9))
            {
                var sexObj = reader.GetValue(9);
                if (sexObj is bool sexBool)
                {
                    sexValue = sexBool ? "Female" : "Male";
                }
                else if (sexObj is byte sexByte)
                {
                    sexValue = sexByte == 0 ? "Male" : "Female";
                }
                else if (sexObj is int sexInt)
                {
                    sexValue = sexInt == 0 ? "Male" : "Female";
                }
                else if (sexObj is string sexStr)
                {
                    var trimmed = sexStr.Trim();
                    if (trimmed == "Male" || trimmed == "Female")
                        sexValue = trimmed;
                    else if (trimmed == "0" || trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Male";
                    else if (trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                        sexValue = "Female";
                }
            }

            return new PermissionRequestDetailsModel
            {
                UserId = reader.GetInt32(0),
                UserName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                UserEmail = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                CurrentFirstName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                CurrentMiddleName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                CurrentLastName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CurrentContact = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                CurrentBirthday = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                CurrentAge = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CurrentSex = sexValue,
                RequestType = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                RequestDataJson = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                RequestedDate = reader.IsDBNull(12) ? DateTime.MinValue : reader.GetDateTime(12)
            };
        }

        public async Task<bool> ApprovePermissionRequestAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            // First, get the request data
            const string getRequestSql = @"SELECT permission_request_type, permission_request_data
                                          FROM dbo.tbl_users
                                          WHERE id = @userId 
                                          AND permission_request_status = 'pending'
                                          AND archived_at IS NULL";

            await using var getCmd = new SqlCommand(getRequestSql, conn) { CommandType = CommandType.Text };
            getCmd.Parameters.AddWithValue("@userId", userId);

            string? requestType = null;
            string? requestDataJson = null;

            await using var reader = await getCmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                requestType = reader.IsDBNull(0) ? null : reader.GetString(0);
                requestDataJson = reader.IsDBNull(1) ? null : reader.GetString(1);
            }
            reader.Close();

            if (string.IsNullOrEmpty(requestType) || string.IsNullOrEmpty(requestDataJson))
            {
                return false;
            }

            bool success = false;

            try
            {
                // Handle combined requests
                if (requestType == "combined")
                {
                    var combined = System.Text.Json.JsonSerializer.Deserialize<CombinedRequestData>(requestDataJson);
                    if (combined != null)
                    {
                        bool personalSuccess = true;
                        bool addressSuccess = true;

                        // Process personal info if present
                        if (combined.PersonalInfo != null)
                        {
                            personalSuccess = await ProcessPersonalInfoUpdateAsync(conn, userId, combined.PersonalInfo, ct);
                        }

                        // Process address if present
                        if (combined.Address != null)
                        {
                            addressSuccess = await ProcessAddressUpdateAsync(conn, userId, combined.Address, ct);
                        }

                        success = personalSuccess && addressSuccess;
                    }
                }
                else if (requestType == "personal_info")
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<PersonalInfoRequestData>(requestDataJson);
                    if (data != null)
                    {
                        success = await ProcessPersonalInfoUpdateAsync(conn, userId, data, ct);
                    }
                }
                else if (requestType == "address")
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<AddressRequestData>(requestDataJson);
                    if (data != null)
                    {
                        success = await ProcessAddressUpdateAsync(conn, userId, data, ct);
                    }
                }

                // Clear permission request if successful
                if (success)
                {
                    const string clearRequestSql = @"UPDATE dbo.tbl_users
                                                       SET permission_request_type = NULL,
                                                           permission_request_data = NULL,
                                                           permission_request_status = NULL,
                                                           permission_request_date = NULL,
                                                           updated_at = SYSUTCDATETIME()
                                                       WHERE id = @userId";
                    await using var clearCmd = new SqlCommand(clearRequestSql, conn) { CommandType = CommandType.Text };
                    clearCmd.Parameters.AddWithValue("@userId", userId);
                    await clearCmd.ExecuteNonQueryAsync(ct);
                }
            }
            catch
            {
                return false;
            }

            return success;
        }

        private async Task<bool> ProcessPersonalInfoUpdateAsync(SqlConnection conn, int userId, PersonalInfoRequestData data, CancellationToken ct)
        {
            const string updateSql = @"UPDATE dbo.tbl_users
                                      SET fname = @fname,
                                          mname = @mname,
                                          lname = @lname,
                                          name = @name,
                                          contact_no = @contact,
                                          bday = @bday,
                                          age = @age,
                                          sex = @sex,
                                          updated_at = SYSUTCDATETIME()
                                      WHERE id = @userId AND archived_at IS NULL";

            await using var updateCmd = new SqlCommand(updateSql, conn) { CommandType = CommandType.Text };
            updateCmd.Parameters.AddWithValue("@userId", userId);
            updateCmd.Parameters.AddWithValue("@fname", (object?)data.FirstName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@mname", (object?)data.MiddleName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@lname", (object?)data.LastName ?? DBNull.Value);
            
            string fullName = $"{data.FirstName} {data.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(data.MiddleName))
            {
                fullName = $"{data.FirstName} {data.MiddleName} {data.LastName}".Trim();
            }
            updateCmd.Parameters.AddWithValue("@name", (object?)fullName ?? DBNull.Value);
            
            updateCmd.Parameters.AddWithValue("@contact", (object?)data.Contact ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@bday", data.Birthday.HasValue ? (object)data.Birthday.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            updateCmd.Parameters.AddWithValue("@age", (object?)data.Age ?? DBNull.Value);
            
            object? sexValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(data.Sex))
            {
                sexValue = data.Sex.Trim().ToLowerInvariant() switch
                {
                    "male" => 0,
                    "female" => 1,
                    _ => DBNull.Value
                };
            }
            updateCmd.Parameters.AddWithValue("@sex", sexValue);

            await updateCmd.ExecuteNonQueryAsync(ct);
            return true;
        }

        private async Task<bool> ProcessAddressUpdateAsync(SqlConnection conn, int userId, AddressRequestData data, CancellationToken ct)
        {
            // Get existing address ID
            const string getAddressSql = @"SELECT TOP 1 id FROM dbo.tbl_addresses 
                                          WHERE user_id = @userId AND archived_at IS NULL 
                                          ORDER BY created_at DESC";
            await using var getAddrCmd = new SqlCommand(getAddressSql, conn) { CommandType = CommandType.Text };
            getAddrCmd.Parameters.AddWithValue("@userId", userId);
            
            int? addressId = null;
            await using var addrReader = await getAddrCmd.ExecuteReaderAsync(ct);
            if (await addrReader.ReadAsync(ct))
            {
                addressId = addrReader.GetInt32(0);
            }
            addrReader.Close();

            if (addressId.HasValue)
            {
                // Update existing address
                const string updateAddrSql = @"UPDATE dbo.tbl_addresses
                                              SET street = @street, 
                                                  city = @city, 
                                                  province = @province, 
                                                  zip = @zip, 
                                                  updated_at = SYSUTCDATETIME()
                                              WHERE id = @id AND user_id = @userId";
                await using var updateAddrCmd = new SqlCommand(updateAddrSql, conn) { CommandType = CommandType.Text };
                updateAddrCmd.Parameters.AddWithValue("@id", addressId.Value);
                updateAddrCmd.Parameters.AddWithValue("@userId", userId);
                updateAddrCmd.Parameters.AddWithValue("@street", (object?)data.Street ?? DBNull.Value);
                updateAddrCmd.Parameters.AddWithValue("@city", (object?)data.City ?? DBNull.Value);
                updateAddrCmd.Parameters.AddWithValue("@province", (object?)data.Province ?? DBNull.Value);
                updateAddrCmd.Parameters.AddWithValue("@zip", (object?)data.Zip ?? DBNull.Value);
                await updateAddrCmd.ExecuteNonQueryAsync(ct);
            }
            else
            {
                // Insert new address
                const string insertAddrSql = @"INSERT INTO dbo.tbl_addresses (user_id, street, city, province, zip, created_at)
                                              VALUES (@userId, @street, @city, @province, @zip, SYSUTCDATETIME())";
                await using var insertAddrCmd = new SqlCommand(insertAddrSql, conn) { CommandType = CommandType.Text };
                insertAddrCmd.Parameters.AddWithValue("@userId", userId);
                insertAddrCmd.Parameters.AddWithValue("@street", (object?)data.Street ?? DBNull.Value);
                insertAddrCmd.Parameters.AddWithValue("@city", (object?)data.City ?? DBNull.Value);
                insertAddrCmd.Parameters.AddWithValue("@province", (object?)data.Province ?? DBNull.Value);
                insertAddrCmd.Parameters.AddWithValue("@zip", (object?)data.Zip ?? DBNull.Value);
                await insertAddrCmd.ExecuteNonQueryAsync(ct);
            }

            return true;
        }

        public async Task<bool> RejectPermissionRequestAsync(int userId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            const string sql = @"UPDATE dbo.tbl_users
                                SET permission_request_type = NULL,
                                    permission_request_data = NULL,
                                    permission_request_status = NULL,
                                    permission_request_date = NULL
                                WHERE id = @userId AND archived_at IS NULL";

            await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
            cmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }
    }

    public class PermissionRequestModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
    }

    public class PermissionRequestDetailsModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CurrentFirstName { get; set; } = string.Empty;
        public string CurrentMiddleName { get; set; } = string.Empty;
        public string CurrentLastName { get; set; } = string.Empty;
        public string CurrentContact { get; set; } = string.Empty;
        public DateOnly? CurrentBirthday { get; set; }
        public int? CurrentAge { get; set; }
        public string CurrentSex { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string RequestDataJson { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
    }

    public class PersonalInfoRequestData
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public DateOnly? Birthday { get; set; }
        public int? Age { get; set; }
        public string? Sex { get; set; }
    }

    public class AddressRequestData
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
    }

    public class CombinedRequestData
    {
        public PersonalInfoRequestData? PersonalInfo { get; set; }
        public AddressRequestData? Address { get; set; }
    }

    public sealed class PersonalInfoModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public DateOnly? Birthday { get; set; }
        public int? Age { get; set; }
        public string Sex { get; set; } = string.Empty;
    }
}

