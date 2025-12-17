using System;

namespace MangaReader.Models
{
    public class MangaInfo
    {
        public int Id { get; set; }
        public string MangaName { get; set; } // Should match the folder name
        public string Title { get; set; }     // Full title (can differ from folder name)
        public string Author { get; set; }
        public int ReleaseYear { get; set; }
        public string Description { get; set; }
        public string CoverImagePath { get; set; } // Path to the cover image file
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}