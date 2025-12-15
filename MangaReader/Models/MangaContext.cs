using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace MangaReader.Models
{
    public class MangaContext
    {
        private string connectionString;

        public MangaContext()
        {
            // Set database path to application folder
            string dbPath = Path.Combine(Application.StartupPath, "mangalibrary.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Check if database exists, if not create it
                string dbPath = Path.Combine(Application.StartupPath, "mangalibrary.db");

                bool isNewDatabase = !File.Exists(dbPath);

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create Users table if it doesn't exist
                    string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        LibraryPath TEXT,
                        CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        IsAdmin BOOLEAN DEFAULT 0
                    )";

                    // Create ReadingProgress table if it doesn't exist
                    string createProgressTable = @"
                    CREATE TABLE IF NOT EXISTS ReadingProgress (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        MangaName TEXT NOT NULL,
                        CurrentChapter TEXT DEFAULT 'Chapter 01',
                        CurrentPage INTEGER DEFAULT 0,
                        LastRead DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                    using (var command = new SQLiteCommand(createUsersTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SQLiteCommand(createProgressTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // If it's a new database, add a default admin user
                    if (isNewDatabase)
                    {
                        AddDefaultAdmin(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}\n\nMake sure SQLite is installed correctly.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddDefaultAdmin(SQLiteConnection connection)
        {
            try
            {
                // Default admin: username=admin, password=admin
                string defaultHash = ComputeSha256Hash("admin");
                string insertAdmin = @"
                INSERT INTO Users (Username, PasswordHash, IsAdmin) 
                VALUES ('admin', @hash, 1)";

                using (var command = new SQLiteCommand(insertAdmin, connection))
                {
                    command.Parameters.AddWithValue("@hash", defaultHash);
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // Ignore if admin already exists
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }
    }
}