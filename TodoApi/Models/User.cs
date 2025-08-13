using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Key]
        public string Email { get; set; }  // Unique identifier for each user
        public string Password { get; set; }

        public ICollection<Todo> Todos { get; set; } = new List<Todo>();

        // New: Many-to-Many relationship
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
       
    }
}
