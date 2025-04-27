using Microsoft.AspNetCore.Mvc;
using testing_back.DTO;
using testing_back.Service;

namespace testing_back.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks([FromQuery] string? sortBy = null)
        {
            var tasks = await _taskService.GetAllTasksSortedAsync(sortBy);
            return Ok(tasks);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (taskDto == null)
            {
                return BadRequest("Task data is required.");
            }

            var task = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetAllTasks), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDTO taskDto)
        {
            if (taskDto == null)
            {
                return BadRequest("Task data is required.");
            }

            var updatedTask = await _taskService.UpdateTaskAsync(id, taskDto);
            if (updatedTask == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success)
            {
                return NotFound("Task not found.");
            }
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> MarkTaskAsCompleted(int id)
        {
            var task = await _taskService.MarkTaskAsCompletedAsync(id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }

        [HttpPatch("{id}/active")]
        public async Task<IActionResult> MarkTaskAsActive(int id)
        {
            var task = await _taskService.MarkTaskAsActiveAsync(id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }
    }
}
