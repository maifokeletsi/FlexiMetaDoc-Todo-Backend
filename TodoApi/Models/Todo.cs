using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class Todo
    {
        public int Id { get; set; } // PK

        // Foreign key
        public string UserId { get; set; }

        public string Title { get; set; }
        public bool Completed { get; set; }

        // Navigation property to User
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
