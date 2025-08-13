using TodoApi.DTOs.Todo;

namespace TodoApi.DTOs
{
    public class UserResponseDto
    {
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<TodoResponseDto> Todos { get; set; } = new List<TodoResponseDto>();
    }
}
