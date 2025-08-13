namespace TodoApi.Models
{
    public class UserRole
    {
        public string UserEmail { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
