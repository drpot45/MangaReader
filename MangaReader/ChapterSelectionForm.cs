using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MangaReader.Models;

namespace MangaReader
{
    public partial class ChapterSelectionForm : Form
    {
        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {
            // Your code to run when the binding source changes goes here
            // For example: Update a label, validate data, etc.
        }
        // ===== DECLARE ALL CONTROLS AND FIELDS HERE =====
        private User currentUser;
        private string selectedMangaPath;
        private string selectedMangaName;
        private List<string> allMangaInLibrary;
        private int currentMangaIndex;
        private MangaInfo currentMangaInfo;

        

        // ===== CONSTRUCTOR =====
        public ChapterSelectionForm(User user, string mangaName, string mangaPath, List<string> allMangaList)
        {
            // InitializeComponent() is called from the Designer.cs file
            InitializeComponent();

            // Now do your custom setup
            currentUser = user;
            selectedMangaName = mangaName;
            selectedMangaPath = mangaPath;
            allMangaInLibrary = allMangaList;
            currentMangaIndex = allMangaList.IndexOf(mangaName);

            LoadMangaData();
            LoadChapters();
            UpdateNavigationButtons();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            // Wire up events for buttons
            btnPrevManga.Click += BtnPrevManga_Click;
            btnNextManga.Click += BtnNextManga_Click;
            btnRead.Click += BtnRead_Click;
            listViewChapters.DoubleClick += ListViewChapters_DoubleClick;
        }

        // ===== MANGA DATA METHODS =====
        private void LoadMangaData()
        {
            LoadMangaInfoFromDatabase();

            string infoFilePath = Path.Combine(selectedMangaPath, "info.txt");
            DateTime fileLastModified = File.Exists(infoFilePath) ? File.GetLastWriteTime(infoFilePath) : DateTime.MinValue;

            if (currentMangaInfo == null || fileLastModified > currentMangaInfo.LastUpdated)
            {
                ParseInfoFile(infoFilePath);
                SaveMangaInfoToDatabase();
            }

            FindCoverImage();
            UpdateMangaDisplay();
        }

        private void LoadMangaInfoFromDatabase()
        {
            using (var connection = new MangaContext().GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM MangaInfo WHERE MangaName = @name";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", selectedMangaName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentMangaInfo = new MangaInfo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                MangaName = reader["MangaName"].ToString(),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                ReleaseYear = reader["ReleaseYear"] != DBNull.Value ? Convert.ToInt32(reader["ReleaseYear"]) : 0,
                                Description = reader["Description"].ToString(),
                                CoverImagePath = reader["CoverImagePath"].ToString(),
                                LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                            };
                        }
                    }
                }
            }
        }

        private void ParseInfoFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                currentMangaInfo = new MangaInfo
                {
                    MangaName = selectedMangaName,
                    Title = selectedMangaName,
                    Author = "Unknown",
                    ReleaseYear = DateTime.Now.Year,
                    Description = "No description available."
                };
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                currentMangaInfo = new MangaInfo { MangaName = selectedMangaName };

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length < 2) continue;

                    string key = parts[0].Trim().ToLower();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "title": currentMangaInfo.Title = value; break;
                        case "author": currentMangaInfo.Author = value; break;
                        case "year":
                            if (int.TryParse(value, out int year))
                                currentMangaInfo.ReleaseYear = year;
                            break;
                        case "description": currentMangaInfo.Description = value; break;
                        case "cover": currentMangaInfo.CoverImagePath = value; break;
                    }
                }

                if (string.IsNullOrEmpty(currentMangaInfo.Title))
                    currentMangaInfo.Title = selectedMangaName;
                if (string.IsNullOrEmpty(currentMangaInfo.Author))
                    currentMangaInfo.Author = "Unknown";
                if (currentMangaInfo.ReleaseYear == 0)
                    currentMangaInfo.ReleaseYear = DateTime.Now.Year;
                if (string.IsNullOrEmpty(currentMangaInfo.Description))
                    currentMangaInfo.Description = "No description available.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading info.txt: {ex.Message}");
            }
        }

        private void SaveMangaInfoToDatabase()
        {
            if (currentMangaInfo == null) return;

            using (var connection = new MangaContext().GetConnection())
            {
                connection.Open();

                string query = @"
                INSERT OR REPLACE INTO MangaInfo 
                (MangaName, Title, Author, ReleaseYear, Description, CoverImagePath, LastUpdated) 
                VALUES (@name, @title, @author, @year, @desc, @cover, @updated)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", currentMangaInfo.MangaName);
                    cmd.Parameters.AddWithValue("@title", currentMangaInfo.Title);
                    cmd.Parameters.AddWithValue("@author", currentMangaInfo.Author);
                    cmd.Parameters.AddWithValue("@year", currentMangaInfo.ReleaseYear);
                    cmd.Parameters.AddWithValue("@desc", currentMangaInfo.Description);
                    cmd.Parameters.AddWithValue("@cover", currentMangaInfo.CoverImagePath ?? "");
                    cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void FindCoverImage()
        {
            if (!string.IsNullOrEmpty(currentMangaInfo.CoverImagePath))
            {
                string fullPath = Path.Combine(selectedMangaPath, currentMangaInfo.CoverImagePath);
                if (File.Exists(fullPath))
                {
                    currentMangaInfo.CoverImagePath = fullPath;
                    return;
                }
            }

            var chapterFolders = Directory.GetDirectories(selectedMangaPath)
                                         .OrderBy(f => f, new NaturalStringComparer())
                                         .ToArray();

            if (chapterFolders.Length > 0)
            {
                var firstChapter = chapterFolders[0];
                var imageFiles = Directory.GetFiles(firstChapter, "*.jpg")
                                         .Concat(Directory.GetFiles(firstChapter, "*.png"))
                                         .Concat(Directory.GetFiles(firstChapter, "*.jpeg"))
                                         .OrderBy(f => f, new NaturalStringComparer())
                                         .ToArray();

                if (imageFiles.Length > 0)
                {
                    currentMangaInfo.CoverImagePath = imageFiles[0];
                }
            }
        }

        private void UpdateMangaDisplay()
        {
            lblTitle.Text = currentMangaInfo.Title;
            lblAuthorYear.Text = $"{currentMangaInfo.Author} • {currentMangaInfo.ReleaseYear}";
            txtDescription.Text = currentMangaInfo.Description;

            if (!string.IsNullOrEmpty(currentMangaInfo.CoverImagePath) && File.Exists(currentMangaInfo.CoverImagePath))
            {
                try
                {
                    if (picCover.Image != null)
                        picCover.Image.Dispose();

                    picCover.Image = Image.FromFile(currentMangaInfo.CoverImagePath);
                }
                catch
                {
                    picCover.Image = null;
                }
            }
        }

        // ===== CHAPTER METHODS =====
        private void LoadChapters()
        {
            listViewChapters.Items.Clear();

            var chapterFolders = Directory.GetDirectories(selectedMangaPath)
                .OrderBy(f => f, new NaturalStringComparer())
                .ToList();

            foreach (var chapterFolder in chapterFolders)
            {
                var chapterName = Path.GetFileName(chapterFolder);
                var pageCount = Directory.GetFiles(chapterFolder, "*.jpg").Length +
                               Directory.GetFiles(chapterFolder, "*.png").Length;

                string lastRead = GetChapterProgress(chapterName);

                var item = new ListViewItem(chapterName);
                item.SubItems.Add(pageCount.ToString());
                item.SubItems.Add(lastRead);
                item.Tag = chapterFolder;

                listViewChapters.Items.Add(item);
            }
        }

        private string GetChapterProgress(string chapterName)
        {
            // Implement with your progress tracking system
            return "Not started";
        }

        private void UpdateNavigationButtons()
        {
            btnPrevManga.Enabled = (currentMangaIndex > 0);
            btnNextManga.Enabled = (currentMangaIndex < allMangaInLibrary.Count - 1);
        }

        // ===== EVENT HANDLERS =====
        private void BtnPrevManga_Click(object sender, EventArgs e)
        {
            if (currentMangaIndex > 0)
            {
                NavigateToManga(currentMangaIndex - 1);
            }
        }

        private void BtnNextManga_Click(object sender, EventArgs e)
        {
            if (currentMangaIndex < allMangaInLibrary.Count - 1)
            {
                NavigateToManga(currentMangaIndex + 1);
            }
        }

        private void NavigateToManga(int newIndex)
        {
            string newMangaName = allMangaInLibrary[newIndex];
            string newMangaPath = Path.Combine(currentUser.LibraryPath, newMangaName);

            selectedMangaName = newMangaName;
            selectedMangaPath = newMangaPath;
            currentMangaIndex = newIndex;

            LoadMangaData();
            LoadChapters();
            UpdateNavigationButtons();
            this.Text = $"Manga Details - {selectedMangaName}";
        }

        private void ListViewChapters_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedChapter();
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            OpenSelectedChapter();
        }

        private void OpenSelectedChapter()
        {
            if (listViewChapters.SelectedItems.Count > 0)
            {
                var selectedItem = listViewChapters.SelectedItems[0];
                string chapterPath = selectedItem.Tag.ToString();
                string chapterName = selectedItem.Text;

                var readerForm = new ReaderForm(selectedMangaName, chapterPath, currentUser, chapterName);
                readerForm.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a chapter to read.");
            }
        }

        // ===== FORM CLEANUP =====
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (picCover.Image != null)
            {
                picCover.Image.Dispose();
                picCover.Image = null;
            }
        }
    }
}