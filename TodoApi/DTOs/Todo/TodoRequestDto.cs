namespace TodoApi.DTOs.Todo
{
    public class TodoRequestDto
    {
        public string UserId { get; set; }  // FK to User.Email
        public string Title { get; set; }
        public bool Completed { get; set; }
    }
}
