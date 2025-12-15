using System;

namespace MangaReader.Models
{
    public class ReadingProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MangaName { get; set; }
        public string CurrentChapter { get; set; } = "Chapter 01";
        public int CurrentPage { get; set; } = 0;
        public DateTime LastRead { get; set; } = DateTime.Now;
    }
}