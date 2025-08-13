namespace TodoApi.DTOs.Todo
{
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }
}
