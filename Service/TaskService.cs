using Microsoft.EntityFrameworkCore;
using testing_back.Data;
using testing_back.DTO;
using testing_back.Models;

namespace testing_back.Service
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskModel>> GetAllTasksSortedAsync(string? sortBy)
        {
            Console.WriteLine($"Received sortBy: {sortBy}");
            IQueryable<TaskModel> query = _context.Tasks;

            switch (sortBy?.ToLower())
            {
                case "status-active":
                    query = query.OrderBy(t => t.Status != StatusTask.Active);
                    break;
                case "status-late":
                    query = query.OrderBy(t => t.Status != StatusTask.Overdue && t.Status != StatusTask.Late);
                    break;

                case "priority-low":
                    query = query.OrderBy(t => t.Priority);
                    break;
                case "priority-high":
                    query = query.OrderByDescending(t => t.Priority);
                    break;

                case "deadline-asc":
                    query = query.OrderBy(t => t.Deadline);
                    break;
                case "deadline-desc":
                    query = query.OrderByDescending(t => t.Deadline);
                    break;

                case "created-oldest":
                    query = query.OrderBy(t => t.CreatedAt);
                    break;
                case "created-newest":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;

                default:
                    query = query.OrderBy(t => t.Id);
                    break;
            }

            return await query.ToListAsync();
        }




        public async Task<TaskModel> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskModel> CreateTaskAsync(TaskDTO taskDto)
        {
            string title = taskDto.Title;
            TaskPriority? priorityFromTitle = null;
            DateTime? deadlineFromTitle = null;

            if (title.Contains("!1"))
            {
                priorityFromTitle = TaskPriority.Critical;
                title = title.Replace("!1", "").Trim();
            }
            else if (title.Contains("!2"))
            {
                priorityFromTitle = TaskPriority.High;
                title = title.Replace("!2", "").Trim();
            }
            else if (title.Contains("!3"))
            {
                priorityFromTitle = TaskPriority.Medium;
                title = title.Replace("!3", "").Trim();
            }
            else if (title.Contains("!4"))
            {
                priorityFromTitle = TaskPriority.Low;
                title = title.Replace("!4", "").Trim();
            }

            var regex = new System.Text.RegularExpressions.Regex(@"!before\s*(\d{2})[.-](\d{2})[.-](\d{4})");
            var match = regex.Match(title);
            if (match.Success)
            {
                var day = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var year = int.Parse(match.Groups[3].Value);

                try
                {
                    deadlineFromTitle = new DateTime(year, month, day).ToUniversalTime();
                }
                catch (Exception)
                {
                }

                title = regex.Replace(title, "").Trim();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Название задачи не может быть пустым. Пожалуйста, введите название.");
            }

            var task = new TaskModel
            {
                Title = title,
                Description = taskDto.Description,
                Deadline = taskDto.Deadline?.ToUniversalTime() ?? deadlineFromTitle,
                Priority = taskDto.Priority ?? priorityFromTitle ?? TaskPriority.Medium
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }



        public async Task<TaskModel> UpdateTaskAsync(int id, TaskDTO taskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null;
            }

            string title = taskDto.Title;
            TaskPriority? priorityFromTitle = null;

            if (title.Contains("!1"))
            {
                priorityFromTitle = TaskPriority.Critical;
                title = title.Replace("!1", "").Trim();
            }
            else if (title.Contains("!2"))
            {
                priorityFromTitle = TaskPriority.High;
                title = title.Replace("!2", "").Trim();
            }
            else if (title.Contains("!3"))
            {
                priorityFromTitle = TaskPriority.Medium;
                title = title.Replace("!3", "").Trim();
            }
            else if (title.Contains("!4"))
            {
                priorityFromTitle = TaskPriority.Low;
                title = title.Replace("!4", "").Trim();
            }

            task.Title = title;
            task.Description = taskDto.Description;
            task.Deadline = taskDto.Deadline;
            task.Priority = taskDto.Priority ?? priorityFromTitle ?? task.Priority;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return task;
        }


        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskModel> MarkTaskAsCompletedAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null;
            }

            task.Status = task.Deadline < DateTime.UtcNow ? StatusTask.Late : StatusTask.Completed;
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel> MarkTaskAsActiveAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null;
            }

            task.Status = task.Deadline < DateTime.UtcNow ? StatusTask.Overdue : StatusTask.Active;
            await _context.SaveChangesAsync();
            return task;
        }
    }
}
