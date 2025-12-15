using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MangaReader.Models;

namespace MangaReader
{
    public partial class ChapterSelectionForm : Form
    {
        private string mangaName;
        private string mangaPath;
        private User currentUser;
        private ListView listViewChapters;

        public ChapterSelectionForm(string mangaName, string mangaPath, User user)
        {
            this.mangaName = mangaName;
            this.mangaPath = mangaPath;
            this.currentUser = user;

            SetupForm();
            LoadChapters();
        }

        private void SetupForm()
        {
            this.Text = $"Select Chapter - {mangaName}";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            listViewChapters = new ListView
            {
                View = View.Details,
                Dock = DockStyle.Fill,
                FullRowSelect = true,
                GridLines = true
            };
            listViewChapters.Columns.Add("Chapter", 200);
            listViewChapters.Columns.Add("Pages", 100);
            listViewChapters.DoubleClick += ListViewChapters_DoubleClick;

            this.Controls.Add(listViewChapters);
        }

        private void LoadChapters()
        {
            var chapterFolders = Directory.GetDirectories(mangaPath)
                .OrderBy(f => f, new NaturalStringComparer())
                .ToList();

            foreach (var chapterFolder in chapterFolders)
            {
                var chapterName = Path.GetFileName(chapterFolder);
                var pageCount = Directory.GetFiles(chapterFolder, "*.jpg").Length;

                var item = new ListViewItem(chapterName);
                item.SubItems.Add(pageCount.ToString());
                listViewChapters.Items.Add(item);
            }
        }

        private void ListViewChapters_DoubleClick(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count > 0)
            {
                var chapterName = listViewChapters.SelectedItems[0].Text;
                var chapterPath = Path.Combine(mangaPath, chapterName);

                var readerForm = new ReaderForm(mangaName, chapterPath, currentUser, chapterName);
                readerForm.Show();
                this.Close();
            }
        }
    }
}