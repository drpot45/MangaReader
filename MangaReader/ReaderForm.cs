using MangaReader.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MangaReader
{
    public partial class ReaderForm : Form
    {
        private string mangaName;
        private string chapterPath;
        private User currentUser;
        private string chapterName;

        private List<string> imageFiles = new List<string>();
        private int currentIndex = 0;

        private PictureBox pictureBox;
        private Button btnPrev;
        private Button btnNext;
        private Label lblStatus;

        public ReaderForm(string mangaName, string path, User user, string chapterName = null)
        {
            this.mangaName = mangaName;
            this.chapterPath = path;
            this.currentUser = user;
            this.chapterName = chapterName ?? "Chapter 01";

            SetupForm();
            LoadImages();
            LoadProgress();
        }

        private void SetupForm()
        {
            this.Text = $"Reading: {mangaName} - {chapterName}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // PictureBox for displaying images
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            // Controls panel
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Gray
            };

            btnPrev = new Button
            {
                Text = "◀ Previous",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            btnPrev.Click += (s, e) => PreviousPage();

            btnNext = new Button
            {
                Text = "Next ▶",
                Location = new Point(120, 10),
                Size = new Size(100, 30)
            };
            btnNext.Click += (s, e) => NextPage();

            lblStatus = new Label
            {
                Location = new Point(250, 15),
                Size = new Size(200, 20),
                Text = "Page 0/0"
            };

            panel.Controls.AddRange(new Control[] { btnPrev, btnNext, lblStatus });

            this.Controls.Add(pictureBox);
            this.Controls.Add(panel);

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += ReaderForm_KeyDown;

            // Save progress when closing
            this.FormClosing += (s, e) => SaveProgress();
        }

        private void LoadImages()
        {
            imageFiles = Directory.GetFiles(chapterPath, "*.jpg")
                .Concat(Directory.GetFiles(chapterPath, "*.png"))
                .Concat(Directory.GetFiles(chapterPath, "*.jpeg"))
                .OrderBy(f => f, new NaturalStringComparer())
                .ToList();

            if (imageFiles.Count > 0)
            {
                lblStatus.Text = $"Page 1/{imageFiles.Count}";
                ShowImage(0);
            }
        }

        private void ShowImage(int index)
        {
            if (index >= 0 && index < imageFiles.Count)
            {
                try
                {
                    // Dispose of previous image to free memory
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }

                    // Load new image
                    using (var tempImage = Image.FromFile(imageFiles[index]))
                    {
                        pictureBox.Image = new Bitmap(tempImage);
                    }

                    currentIndex = index;
                    lblStatus.Text = $"Page {index + 1}/{imageFiles.Count}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
        }

        private void PreviousPage()
        {
            if (currentIndex > 0)
            {
                ShowImage(currentIndex - 1);
                SaveProgress();
            }
        }

        private void NextPage()
        {
            if (currentIndex < imageFiles.Count - 1)
            {
                ShowImage(currentIndex + 1);
                SaveProgress();
            }
        }

        private void ReaderForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.PageUp)
                PreviousPage();
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Space)
                NextPage();
            else if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void LoadProgress()
        {
            try
            {
                string progressFile = GetProgressFilePath();

                if (File.Exists(progressFile))
                {
                    string[] lines = File.ReadAllLines(progressFile);

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 5)
                        {
                            int userId = int.Parse(parts[0]);
                            string storedManga = parts[1];
                            string storedChapter = parts[2];

                            if (userId == currentUser.Id &&
                                storedManga == mangaName &&
                                storedChapter == chapterName)
                            {
                                currentIndex = int.Parse(parts[3]);
                                ShowImage(currentIndex);
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        private void SaveProgress()
        {
            try
            {
                string progressFile = GetProgressFilePath();
                List<string> lines = new List<string>();

                if (File.Exists(progressFile))
                {
                    lines = File.ReadAllLines(progressFile).ToList();

                    // Remove old entry for this user+manga+chapter
                    lines.RemoveAll(line =>
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 3)
                        {
                            int userId = int.Parse(parts[0]);
                            string storedManga = parts[1];
                            string storedChapter = parts[2];
                            return userId == currentUser.Id &&
                                   storedManga == mangaName &&
                                   storedChapter == chapterName;
                        }
                        return false;
                    });
                }

                // Add new entry
                // Format: UserId|MangaName|ChapterName|PageNumber|DateTime
                string newEntry = $"{currentUser.Id}|{mangaName}|{chapterName}|{currentIndex}|{DateTime.Now}";
                lines.Add(newEntry);

                // Save to file
                File.WriteAllLines(progressFile, lines);
            }
            catch
            {
                // Ignore errors
            }
        }

        private string GetProgressFilePath()
        {
            return Path.Combine(Application.StartupPath, "reading_progress.txt");
        }
    }
}