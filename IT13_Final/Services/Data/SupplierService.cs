using System.Data;
using Microsoft.Data.SqlClient;

namespace IT13_Final.Services.Data
{
    public class SupplierModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }

    public class ArchivedSupplierModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime ArchivedDate { get; set; }
    }

    public class SupplierDetailsModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
    }

    public interface ISupplierService
    {
        Task<List<SupplierModel>> GetSuppliersAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetSuppliersCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default);
        Task<SupplierDetailsModel?> GetSupplierDetailsAsync(int supplierId, int userId, CancellationToken ct = default);
        Task<int?> CreateSupplierAsync(int userId, string companyName, string? contactPerson, string? email, string? contactNumber, string status, AddressModel? address, CancellationToken ct = default);
        Task<bool> UpdateSupplierAsync(int supplierId, int userId, string companyName, string? contactPerson, string? email, string? contactNumber, string status, AddressModel? address, CancellationToken ct = default);
        Task<bool> ArchiveSupplierAsync(int supplierId, int userId, CancellationToken ct = default);
        Task<List<ArchivedSupplierModel>> GetArchivedSuppliersAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<int> GetArchivedSuppliersCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default);
        Task<bool> RestoreSupplierAsync(int supplierId, int userId, CancellationToken ct = default);
        Task<SupplierDetailsModel?> GetArchivedSupplierDetailsAsync(int supplierId, int userId, CancellationToken ct = default);
    }

    public class SupplierService : ISupplierService
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=db_SoftWear;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<List<SupplierModel>> GetSuppliersAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var suppliers = new List<SupplierModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, company_name, contact_person, email, contact_number, status, created_at
                FROM dbo.tbl_suppliers
                WHERE archived_at IS NULL AND user_id = @UserId
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (company_name LIKE @SearchTerm OR contact_person LIKE @SearchTerm OR email LIKE @SearchTerm OR contact_number LIKE @SearchTerm)") + @"
                ORDER BY created_at DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                suppliers.Add(new SupplierModel
                {
                    Id = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    ContactPerson = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? "Active" : reader.GetString(5),
                    DateCreated = reader.GetDateTime(6)
                });
            }

            return suppliers;
        }

        public async Task<int> GetSuppliersCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_suppliers
                WHERE archived_at IS NULL AND user_id = @UserId
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (company_name LIKE @SearchTerm OR contact_person LIKE @SearchTerm OR email LIKE @SearchTerm OR contact_number LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<SupplierDetailsModel?> GetSupplierDetailsAsync(int supplierId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.company_name, s.contact_person, s.email, s.contact_number, s.status, s.created_at,
                       a.street, a.city, a.province, a.zip
                FROM dbo.tbl_suppliers s
                LEFT JOIN dbo.tbl_addresses a ON a.supplier_id = s.id AND a.archived_at IS NULL
                WHERE s.id = @SupplierId AND s.user_id = @UserId AND s.archived_at IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SupplierId", supplierId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SupplierDetailsModel
                {
                    Id = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    ContactPerson = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? "Active" : reader.GetString(5),
                    DateCreated = reader.GetDateTime(6),
                    Street = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    City = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    Province = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Zip = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                };
            }

            return null;
        }

        public async Task<int?> CreateSupplierAsync(int userId, string companyName, string? contactPerson, string? email, string? contactNumber, string status, AddressModel? address, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO dbo.tbl_suppliers (user_id, company_name, contact_person, email, contact_number, status, created_at)
                    VALUES (@UserId, @CompanyName, @ContactPerson, @Email, @ContactNumber, @Status, SYSUTCDATETIME());
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CompanyName", companyName);
                cmd.Parameters.AddWithValue("@ContactPerson", (object?)contactPerson ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactNumber", (object?)contactNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", status);

                var result = await cmd.ExecuteScalarAsync(ct);
                var supplierId = result != null ? Convert.ToInt32(result) : (int?)null;

                if (supplierId.HasValue && address != null && 
                    (!string.IsNullOrWhiteSpace(address.Street) || !string.IsNullOrWhiteSpace(address.City) || 
                     !string.IsNullOrWhiteSpace(address.Province) || !string.IsNullOrWhiteSpace(address.Zip)))
                {
                    var addressSql = @"
                        INSERT INTO dbo.tbl_addresses (user_id, supplier_id, street, city, province, zip, created_at)
                        VALUES (@UserId, @SupplierId, @Street, @City, @Province, @Zip, SYSUTCDATETIME())";

                    using var addressCmd = new SqlCommand(addressSql, conn, transaction);
                    addressCmd.Parameters.AddWithValue("@UserId", userId);
                    addressCmd.Parameters.AddWithValue("@SupplierId", supplierId.Value);
                    addressCmd.Parameters.AddWithValue("@Street", (object?)address.Street ?? DBNull.Value);
                    addressCmd.Parameters.AddWithValue("@City", (object?)address.City ?? DBNull.Value);
                    addressCmd.Parameters.AddWithValue("@Province", (object?)address.Province ?? DBNull.Value);
                    addressCmd.Parameters.AddWithValue("@Zip", (object?)address.Zip ?? DBNull.Value);
                    await addressCmd.ExecuteNonQueryAsync(ct);
                }

                transaction.Commit();
                return supplierId;
            }
            catch
            {
                transaction.Rollback();
                return null;
            }
        }

        public async Task<bool> UpdateSupplierAsync(int supplierId, int userId, string companyName, string? contactPerson, string? email, string? contactNumber, string status, AddressModel? address, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                var sql = @"
                    UPDATE dbo.tbl_suppliers 
                    SET company_name = @CompanyName, 
                        contact_person = @ContactPerson,
                        email = @Email,
                        contact_number = @ContactNumber,
                        status = @Status,
                        updated_at = SYSUTCDATETIME()
                    WHERE id = @SupplierId AND user_id = @UserId AND archived_at IS NULL";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@SupplierId", supplierId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CompanyName", companyName);
                cmd.Parameters.AddWithValue("@ContactPerson", (object?)contactPerson ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactNumber", (object?)contactNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", status);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Update or insert address
                if (address != null)
                {
                    // Check if address exists
                    var checkAddressSql = @"SELECT id FROM dbo.tbl_addresses WHERE supplier_id = @SupplierId AND archived_at IS NULL";
                    using var checkCmd = new SqlCommand(checkAddressSql, conn, transaction);
                    checkCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                    var addressId = await checkCmd.ExecuteScalarAsync(ct);

                    if (addressId != null && addressId != DBNull.Value)
                    {
                        // Update existing address
                        var updateAddressSql = @"
                            UPDATE dbo.tbl_addresses 
                            SET street = @Street, city = @City, province = @Province, zip = @Zip, updated_at = SYSUTCDATETIME()
                            WHERE id = @AddressId AND supplier_id = @SupplierId";
                        using var updateCmd = new SqlCommand(updateAddressSql, conn, transaction);
                        updateCmd.Parameters.AddWithValue("@AddressId", addressId);
                        updateCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                        updateCmd.Parameters.AddWithValue("@Street", (object?)address.Street ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@City", (object?)address.City ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Province", (object?)address.Province ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Zip", (object?)address.Zip ?? DBNull.Value);
                        await updateCmd.ExecuteNonQueryAsync(ct);
                    }
                    else if (!string.IsNullOrWhiteSpace(address.Street) || !string.IsNullOrWhiteSpace(address.City) || 
                             !string.IsNullOrWhiteSpace(address.Province) || !string.IsNullOrWhiteSpace(address.Zip))
                    {
                        // Insert new address
                        var insertAddressSql = @"
                            INSERT INTO dbo.tbl_addresses (user_id, supplier_id, street, city, province, zip, created_at)
                            VALUES (@UserId, @SupplierId, @Street, @City, @Province, @Zip, SYSUTCDATETIME())";
                        using var insertCmd = new SqlCommand(insertAddressSql, conn, transaction);
                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                        insertCmd.Parameters.AddWithValue("@Street", (object?)address.Street ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@City", (object?)address.City ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Province", (object?)address.Province ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Zip", (object?)address.Zip ?? DBNull.Value);
                        await insertCmd.ExecuteNonQueryAsync(ct);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<bool> ArchiveSupplierAsync(int supplierId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Archive supplier
                var sql = @"
                    UPDATE dbo.tbl_suppliers
                    SET archived_at = SYSUTCDATETIME(), status = 'Archived', updated_at = SYSUTCDATETIME()
                    WHERE id = @SupplierId AND user_id = @UserId AND archived_at IS NULL";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@SupplierId", supplierId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Archive associated address
                var addressSql = @"
                    UPDATE dbo.tbl_addresses
                    SET archived_at = SYSUTCDATETIME(), updated_at = SYSUTCDATETIME()
                    WHERE supplier_id = @SupplierId AND archived_at IS NULL";

                using var addressCmd = new SqlCommand(addressSql, conn, transaction);
                addressCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                await addressCmd.ExecuteNonQueryAsync(ct);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<List<ArchivedSupplierModel>> GetArchivedSuppliersAsync(int userId, string? searchTerm = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var suppliers = new List<ArchivedSupplierModel>();
            var offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT id, company_name, contact_person, email, contact_number, archived_at
                FROM dbo.tbl_suppliers
                WHERE archived_at IS NOT NULL AND user_id = @UserId
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (company_name LIKE @SearchTerm OR contact_person LIKE @SearchTerm OR email LIKE @SearchTerm OR contact_number LIKE @SearchTerm)") + @"
                ORDER BY archived_at DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                suppliers.Add(new ArchivedSupplierModel
                {
                    Id = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    ContactPerson = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ArchivedDate = reader.GetDateTime(5)
                });
            }

            return suppliers;
        }

        public async Task<int> GetArchivedSuppliersCountAsync(int userId, string? searchTerm = null, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT COUNT(*)
                FROM dbo.tbl_suppliers
                WHERE archived_at IS NOT NULL AND user_id = @UserId
                " + (string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (company_name LIKE @SearchTerm OR contact_person LIKE @SearchTerm OR email LIKE @SearchTerm OR contact_number LIKE @SearchTerm)");

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }

            var count = await cmd.ExecuteScalarAsync(ct);
            return count != null ? Convert.ToInt32(count) : 0;
        }

        public async Task<bool> RestoreSupplierAsync(int supplierId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            using var transaction = conn.BeginTransaction();

            try
            {
                // Restore supplier
                var sql = @"
                    UPDATE dbo.tbl_suppliers
                    SET archived_at = NULL, status = 'Active', updated_at = SYSUTCDATETIME()
                    WHERE id = @SupplierId AND user_id = @UserId AND archived_at IS NOT NULL";

                using var cmd = new SqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@SupplierId", supplierId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Restore associated address
                var addressSql = @"
                    UPDATE dbo.tbl_addresses
                    SET archived_at = NULL, updated_at = SYSUTCDATETIME()
                    WHERE supplier_id = @SupplierId AND archived_at IS NOT NULL";

                using var addressCmd = new SqlCommand(addressSql, conn, transaction);
                addressCmd.Parameters.AddWithValue("@SupplierId", supplierId);
                await addressCmd.ExecuteNonQueryAsync(ct);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<SupplierDetailsModel?> GetArchivedSupplierDetailsAsync(int supplierId, int userId, CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT s.id, s.company_name, s.contact_person, s.email, s.contact_number, s.status, s.created_at,
                       a.street, a.city, a.province, a.zip
                FROM dbo.tbl_suppliers s
                LEFT JOIN dbo.tbl_addresses a ON a.supplier_id = s.id
                WHERE s.id = @SupplierId AND s.user_id = @UserId AND s.archived_at IS NOT NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SupplierId", supplierId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new SupplierDetailsModel
                {
                    Id = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    ContactPerson = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ContactNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? "Archived" : reader.GetString(5),
                    DateCreated = reader.GetDateTime(6),
                    Street = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    City = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    Province = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Zip = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                };
            }

            return null;
        }
    }
}

