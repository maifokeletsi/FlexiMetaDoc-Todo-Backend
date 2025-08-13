using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.DTOs.Todo;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoResponseDto>>> GetTodos()
        {
            var todos = await _context.Todos
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Completed = t.Completed
                })
                .ToListAsync();

            return Ok(todos);
        }

        // GET: api/Todo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoResponseDto>> GetTodoById(int id)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id)
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Completed = t.Completed
                })
                .FirstOrDefaultAsync();

            if (todo == null)
                return NotFound();

            return Ok(todo);
        }

        // POST: api/Todo
        [HttpPost]
        public async Task<ActionResult<TodoResponseDto>> CreateTodo(TodoRequestDto request)
        {
            // Optional: Check if user exists before adding
            var userExists = await _context.Users.AnyAsync(u => u.Email == request.UserId);
            if (!userExists)
                return BadRequest("User not found.");

            var todo = new Todo
            {
                UserId = request.UserId,
                Title = request.Title,
                Completed = request.Completed
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            var response = new TodoResponseDto
            {
                Id = todo.Id,
                UserId = todo.UserId,
                Title = todo.Title,
                Completed = todo.Completed
            };

            return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, response);
        }

        // PUT: api/Todo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, TodoRequestDto request)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            todo.UserId = request.UserId;
            todo.Title = request.Title;
            todo.Completed = request.Completed;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Todo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
