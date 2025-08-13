using TodoApi.DTOs.Todo;

namespace TodoApi.DTOs
{
    public class UserRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        // Optional: Role IDs to assign to the user
        public List<int> RoleIds { get; set; } = new List<int>();

        
    }
}
