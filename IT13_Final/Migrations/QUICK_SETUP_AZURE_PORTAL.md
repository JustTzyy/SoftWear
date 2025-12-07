# Quick Setup via Azure Portal Query Editor

Since direct connection is having issues, use Azure Portal Query Editor - it works without DNS/network access!

## Steps:

### 1. Open Azure Portal Query Editor

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your **SQL Database** resource (not the server)
   - Look for `db_SoftWear` or your database name
3. In the left menu, click **"Query editor (preview)"**
4. Login with:
   - **Login:** `justin`
   - **Password:** `JussPogi27`
   - Click **"OK"**

### 2. Run the Setup Script

1. In the Query Editor, you'll see a text area
2. Open the file: `C:\Github\SoftWear\IT13_Final\Migrations\Azure_SQL_Setup.sql`
3. **Copy the ENTIRE contents** of the file
4. **Paste** into the Query Editor
5. Click **"Run"** button (or press F5)

### 3. Wait for Completion

- The script will create the database and all tables
- You'll see progress messages in the results pane
- Look for "All migrations completed!" message

### 4. Verify Setup

1. In Azure Portal, go back to your SQL Database
2. Click **"Query editor"** again
3. Run this query to verify:

```sql
USE db_SoftWear;
SELECT COUNT(*) as TableCount 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';
```

You should see **23 tables** created.

## Alternative: Use SQL Server Management Studio

If you prefer SSMS:

1. **Download SSMS:** https://aka.ms/ssmsfullsetup
2. **Connect:**
   - Server: `jussstzy.database.windows.net`
   - Authentication: SQL Server Authentication
   - Login: `justin`
   - Password: `JussPogi27`
3. **Open:** `C:\Github\SoftWear\IT13_Final\Migrations\Azure_SQL_Setup.sql`
4. **Execute:** Press F5

## Troubleshooting

### "Cannot connect" in Query Editor
- Make sure you're on the **Database** resource, not the Server
- Try refreshing the page
- Check that the database exists

### "Login failed"
- Verify username: `justin`
- Verify password: `JussPogi27`
- Make sure SQL authentication is enabled

### Script errors
- Make sure you copied the ENTIRE script
- Check for any error messages in red
- Some errors are normal if tables already exist

## After Setup

Once the database is set up, your application will automatically connect using the connection strings that are already configured in all service files!












