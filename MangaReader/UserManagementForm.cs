using System;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MangaReader.Models;

namespace MangaReader
{
    public partial class UserManagementForm : Form
    {
        // We need the current admin to prevent them from deleting/toggling themselves
        private User currentAdminUser;
        private DataGridView dataGridViewUsers;
        private Button btnDeleteUser;
        private Button btnToggleAdmin;
        private Button btnRefresh;
        private Label lblWarning;

        public UserManagementForm(User adminUser)
        {
            currentAdminUser = adminUser;
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.Text = "User Management";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Warning label at the top
            lblWarning = new Label
            {
                Text = "⚠️ You cannot delete or change the role of your own account.",
                ForeColor = Color.OrangeRed,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // DataGridView to display users
            dataGridViewUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = SystemColors.Window
            };

            // Button panel at the bottom
            var panelButtons = new Panel { Height = 50, Dock = DockStyle.Bottom };

            btnDeleteUser = new Button
            {
                Text = "🗑️ Delete Selected User",
                Size = new Size(150, 35),
                Location = new Point(20, 8),
                BackColor = Color.LightCoral,
                Enabled = false // Disabled until a user is selected
            };
            btnDeleteUser.Click += BtnDeleteUser_Click;

            btnToggleAdmin = new Button
            {
                Text = "🛡️ Toggle Admin Role",
                Size = new Size(150, 35),
                Location = new Point(190, 8),
                BackColor = Color.LightGoldenrodYellow,
                Enabled = false
            };
            btnToggleAdmin.Click += BtnToggleAdmin_Click;

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Size = new Size(100, 35),
                Location = new Point(560, 8),
                BackColor = Color.LightGray
            };
            btnRefresh.Click += (s, e) => LoadUserData();

            panelButtons.Controls.AddRange(new Control[] { btnDeleteUser, btnToggleAdmin, btnRefresh });

            // Assemble the form
            this.Controls.Add(dataGridViewUsers);
            this.Controls.Add(panelButtons);
            this.Controls.Add(lblWarning);

            // Wire up selection change event
            dataGridViewUsers.SelectionChanged += DataGridViewUsers_SelectionChanged;
        }

        private void LoadUserData()
        {
            try
            {
                using (var connection = new MangaContext().GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Id, Username, IsAdmin FROM Users ORDER BY Id";
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        var dataTable = new System.Data.DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGridView
                        dataGridViewUsers.DataSource = dataTable;

                        // Format the columns
                        if (dataGridViewUsers.Columns.Count > 0)
                        {
                            dataGridViewUsers.Columns["Id"].Width = 50;
                            dataGridViewUsers.Columns["Username"].Width = 200;
                            dataGridViewUsers.Columns["IsAdmin"].Width = 100;

                            // Optional: Make the IsAdmin column show Yes/No
                            dataGridViewUsers.Columns["IsAdmin"].DefaultCellStyle.Format = "Yes/No";
                        }
                    }
                }
                // Clear selection after refresh
                dataGridViewUsers.ClearSelection();
                btnDeleteUser.Enabled = false;
                btnToggleAdmin.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridViewUsers_SelectionChanged(object sender, EventArgs e)
        {
            // Enable buttons only if a single row is selected AND it's not the current admin
            bool isRowSelected = dataGridViewUsers.SelectedRows.Count == 1;

            if (isRowSelected)
            {
                var selectedRow = dataGridViewUsers.SelectedRows[0];
                int selectedUserId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                string selectedUsername = selectedRow.Cells["Username"].Value.ToString();

                // Disable buttons if the selected user is the current admin
                bool isCurrentUser = (selectedUserId == currentAdminUser.Id);

                btnDeleteUser.Enabled = !isCurrentUser;
                btnToggleAdmin.Enabled = !isCurrentUser;

                // Update warning text
                lblWarning.Text = isCurrentUser
                    ? "⚠️ You cannot modify your own account."
                    : $"Selected user: '{selectedUsername}' (ID: {selectedUserId})";
            }
            else
            {
                btnDeleteUser.Enabled = false;
                btnToggleAdmin.Enabled = false;
                lblWarning.Text = "⚠️ You cannot delete or change the role of your own account.";
            }
        }

        private void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count != 1) return;

            var selectedRow = dataGridViewUsers.SelectedRows[0];
            int userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string username = selectedRow.Cells["Username"].Value.ToString();

            // Confirm deletion
            var result = MessageBox.Show($"Are you SURE you want to delete user '{username}'? This action cannot be undone.",
                                         "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (var connection = new MangaContext().GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Users WHERE Id = @id";
                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // Also delete their reading progress (optional but recommended)
                    DeleteUserProgress(userId);

                    MessageBox.Show($"User '{username}' has been deleted.", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUserData(); // Refresh the list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Deletion failed: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteUserProgress(int userId)
        {
            try
            {
                using (var connection = new MangaContext().GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM ReadingProgress WHERE UserId = @userId";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Silently fail if progress table doesn't exist or other error */ }
        }

        private void BtnToggleAdmin_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count != 1) return;

            var selectedRow = dataGridViewUsers.SelectedRows[0];
            int userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string username = selectedRow.Cells["Username"].Value.ToString();
            bool currentAdminStatus = Convert.ToBoolean(selectedRow.Cells["IsAdmin"].Value);
            bool newAdminStatus = !currentAdminStatus;

            string action = newAdminStatus ? "promote to ADMIN" : "demote to REGULAR USER";
            var result = MessageBox.Show($"Are you sure you want to {action} for user '{username}'?",
                                         "Confirm Role Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (var connection = new MangaContext().GetConnection())
                    {
                        connection.Open();
                        string query = "UPDATE Users SET IsAdmin = @isAdmin WHERE Id = @id";
                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@isAdmin", newAdminStatus);
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show($"User '{username}' is now a {(newAdminStatus ? "Admin" : "Regular User")}.",
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUserData(); // Refresh the list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Role change failed: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}