namespace TodoApi.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Email { get; set; }   // User's email (identifier)
        public string Password { get; set; } // User's password
    }
}
