namespace TodoApi.Models
{
    public class Todo
    {
        public int UserId { get; set; }
        public int Id { get; set; }  // Unique identifier for each todo
        public string Title { get; set; }
        public bool Completed { get; set; }
    }
}
