using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MangaReader.Models;
using System.Data.SQLite;

namespace MangaReader
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private List<string> mangaList = new List<string>();
        private ListView listViewManga;
        private Button btnSetPath;
        private Label lblPath;

        public MainForm(User user)
        {
            currentUser = user;
            SetupForm();
            LoadLibrary();
        }

        private void SetupForm()
        {
            this.Text = "Manga Library";
            this.Size = new Size(600, 400);

            // Menu
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Set Library Path", null, (s, e) => SetLibraryPath());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Controls
            btnSetPath = new Button
            {
                Text = "📁 Set Library Path",
                Location = new Point(10, 30),
                Size = new Size(150, 30)
            };
            btnSetPath.Click += (s, e) => SetLibraryPath();

            lblPath = new Label
            {
                Location = new Point(170, 35),
                Size = new Size(400, 20),
                Text = currentUser.LibraryPath ?? "No library path set"
            };

            // Manga list
            listViewManga = new ListView
            {
                View = View.Details,
                Location = new Point(10, 70),
                Size = new Size(560, 280),
                FullRowSelect = true,
                GridLines = true
            };
            listViewManga.Columns.Add("Manga Title", 300);
            listViewManga.Columns.Add("Chapters", 100);
            listViewManga.Columns.Add("Last Read", 150);
            listViewManga.DoubleClick += ListViewManga_DoubleClick;

            this.Controls.AddRange(new Control[] { menuStrip, btnSetPath, lblPath, listViewManga });
        }

        private void SetLibraryPath()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select your manga library folder";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    currentUser.LibraryPath = dialog.SelectedPath;
                    lblPath.Text = dialog.SelectedPath;

                    // Save to database
                    using (var connection = new MangaContext().GetConnection())
                    {
                        connection.Open();
                        string query = "UPDATE Users SET LibraryPath = @path WHERE Id = @id";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@path", dialog.SelectedPath);
                            command.Parameters.AddWithValue("@id", currentUser.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadLibrary();
                }
            }
        }

        private void LoadLibrary()
        {
            if (string.IsNullOrEmpty(currentUser.LibraryPath) || !Directory.Exists(currentUser.LibraryPath))
                return;

            listViewManga.Items.Clear();
            mangaList.Clear();

            // Get all folders (each folder is a manga)
            foreach (var mangaFolder in Directory.GetDirectories(currentUser.LibraryPath))
            {
                var mangaName = Path.GetFileName(mangaFolder);
                mangaList.Add(mangaName);

                // Count chapters (subfolders)
                int chapterCount = Directory.GetDirectories(mangaFolder).Length;

                // Get last read progress
                string lastRead = GetLastReadProgress(mangaName);

                var item = new ListViewItem(mangaName);
                item.SubItems.Add(chapterCount.ToString());
                item.SubItems.Add(lastRead);
                listViewManga.Items.Add(item);
            }
        }

        private string GetLastReadProgress(string mangaName)
        {
            try
            {
                string progressFile = Path.Combine(Application.StartupPath, "reading_progress.txt");

                if (File.Exists(progressFile))
                {
                    string[] lines = File.ReadAllLines(progressFile);

                    // Find most recent entry for this user and manga
                    string latestEntry = null;
                    DateTime latestDate = DateTime.MinValue;

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 5)
                        {
                            int userId = int.Parse(parts[0]);
                            string storedManga = parts[1];
                            string storedChapter = parts[2];
                            int page = int.Parse(parts[3]);
                            DateTime date = DateTime.Parse(parts[4]);

                            if (userId == currentUser.Id && storedManga == mangaName)
                            {
                                if (date > latestDate)
                                {
                                    latestDate = date;
                                    latestEntry = line;
                                }
                            }
                        }
                    }

                    if (latestEntry != null)
                    {
                        string[] parts = latestEntry.Split('|');
                        return $"Ch {parts[2]} - Page {int.Parse(parts[3]) + 1}";
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return "Not started";
        }

        private void ListViewManga_DoubleClick(object sender, EventArgs e)
        {
            if (listViewManga.SelectedItems.Count > 0)
            {
                var mangaName = listViewManga.SelectedItems[0].Text;
                OpenManga(mangaName);
            }
        }

        private void OpenManga(string mangaName)
        {
            var mangaPath = Path.Combine(currentUser.LibraryPath, mangaName);

            // Check if manga has chapters (subfolders)
            var chapterFolders = Directory.GetDirectories(mangaPath);

            if (chapterFolders.Length > 0)
            {
                // Show chapter selection
                var chapterForm = new ChapterSelectionForm(mangaName, mangaPath, currentUser);
                chapterForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("No chapters found in this manga folder. Each manga should have chapter subfolders.");
            }
        }
    }
}