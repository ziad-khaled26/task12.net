using System.ComponentModel.DataAnnotations;

namespace To_Do_App.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }
    }
}
