using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using TaskTide.Models;


namespace TaskTide.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly IDbConnection _db;

    public TaskController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var tasks = await _db.QueryAsync<Task>("SELECT * FROM Tasks WHERE UserId = @UserId", new { UserId = userId });
        return Ok(tasks);
    }
     
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskModel task)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        task.UserId = userId;
        var sql = "INSERT INTO Tasks (UserId, Title, Description, DueDate, IsCompleted) VALUES (@UserId, @Title, @Description, @DueDate, @IsCompleted); SELECT SCOPE_IDENTITY();";
        var id = await _db.ExecuteScalarAsync<int>(sql, task);
        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskModel task)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (task.UserId != userId) return Unauthorized();
        var sql = "UPDATE Tasks SET Title = @Title, Description = @Description, DueDate = @DueDate, IsCompleted = @IsCompleted WHERE Id = @Id AND UserId = @UserId";
        var rows = await _db.ExecuteAsync(sql, new { Id = id, UserId = userId, task.Title, task.Description, task.DueDate, task.IsCompleted });
        return rows > 0 ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var sql = "DELETE FROM Tasks WHERE Id = @Id AND UserId = @UserId";
        var rows = await _db.ExecuteAsync(sql, new { Id = id, UserId = userId });
        return rows > 0 ? NoContent() : NotFound();
    }
}
