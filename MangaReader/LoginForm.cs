using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MangaReader.Models;
using System.Data.SQLite;

namespace MangaReader
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;

        public LoginForm()
        {
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Manga Reader - Login";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblUsername = new Label { Text = "Username:", Location = new Point(20, 30), Size = new Size(80, 20) };
            txtUsername = new TextBox { Location = new Point(100, 30), Size = new Size(160, 20) };

            Label lblPassword = new Label { Text = "Password:", Location = new Point(20, 70), Size = new Size(80, 20) };
            txtPassword = new TextBox { Location = new Point(100, 70), Size = new Size(160, 20), UseSystemPasswordChar = true };

            btnLogin = new Button { Text = "Login", Location = new Point(100, 110), Size = new Size(75, 30) };
            btnRegister = new Button { Text = "Register", Location = new Point(185, 110), Size = new Size(75, 30) };

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;

            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnRegister });
        }



        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            try
            {
                using (var connection = new MangaContext().GetConnection())
                {
                    connection.Open();
                    var hash = ComputeSha256Hash(txtPassword.Text);

                    string query = "SELECT * FROM Users WHERE Username = @username AND PasswordHash = @hash";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        command.Parameters.AddWithValue("@hash", hash);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                User user = new User
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Username = reader["Username"].ToString(),
                                    LibraryPath = reader["LibraryPath"]?.ToString(),
                                    IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                                };

                                var mainForm = new MainForm(user);
                                mainForm.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login Error: {ex.Message}");
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            if (txtUsername.Text.Length < 3)
            {
                MessageBox.Show("Username must be at least 3 characters");
                return;
            }

            if (txtPassword.Text.Length < 3)
            {
                MessageBox.Show("Password must be at least 3 characters");
                return;
            }

            try
            {
                using (var connection = new MangaContext().GetConnection())
                {
                    connection.Open();

                    // Check if username exists
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose another.");
                            return;
                        }
                    }

                    // Register new user
                    var hash = ComputeSha256Hash(txtPassword.Text);
                    string insertQuery = "INSERT INTO Users (Username, PasswordHash) VALUES (@username, @hash)";
                    using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@hash", hash);
                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Registration successful! You can now login.");
                    txtUsername.Clear();
                    txtPassword.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration Error: {ex.Message}");
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}