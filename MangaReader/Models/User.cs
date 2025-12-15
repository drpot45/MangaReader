using System;
using System.ComponentModel.DataAnnotations;

namespace MangaReader.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(64)] // SHA256 hash
        public string PasswordHash { get; set; }

        public string LibraryPath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsAdmin { get; set; } = false;
    }
}