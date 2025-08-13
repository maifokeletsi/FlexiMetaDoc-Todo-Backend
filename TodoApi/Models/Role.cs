using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g. "Admin", "User"

        // Navigation property for related Users
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
