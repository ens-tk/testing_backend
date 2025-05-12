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


        public TaskPriority? ExtractPriorityFromTitle(ref string title)
        {
            TaskPriority? priority = null;

            if (title.Contains("!1"))
            {
                priority = TaskPriority.Critical;
                title = title.Replace("!1", "").Trim();
            }
            else if (title.Contains("!2"))
            {
                priority = TaskPriority.High;
                title = title.Replace("!2", "").Trim();
            }
            else if (title.Contains("!3"))
            {
                priority = TaskPriority.Medium;
                title = title.Replace("!3", "").Trim();
            }
            else if (title.Contains("!4"))
            {
                priority = TaskPriority.Low;
                title = title.Replace("!4", "").Trim();
            }

            return priority;
        }


        public DateTime? ExtractDeadlineFromTitle(ref string title)
        {
            DateTime? deadline = null;

            var regex = new System.Text.RegularExpressions.Regex(@"!before\s*(\d{2})[.-](\d{2})[.-](\d{4})");
            var match = regex.Match(title);
            if (match.Success)
            {
                var day = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var year = int.Parse(match.Groups[3].Value);

                try
                {
                    deadline = new DateTime(year, month, day).ToUniversalTime();
                }
                catch (Exception)
                {
                }

                title = regex.Replace(title, "").Trim();
            }

            return deadline;
        }


        public async Task<TaskModel> CreateTaskAsync(TaskDTO taskDto)
        {
            string title = taskDto.Title;

            TaskPriority? priorityFromTitle = ExtractPriorityFromTitle(ref title);
            DateTime? deadlineFromTitle = ExtractDeadlineFromTitle(ref title);

            if (string.IsNullOrWhiteSpace(title) || title.Length < 4)
            {
                throw new ArgumentException("Название задачи должно содержать минимум 4 символа после удаления макросов.");
            }

            var deadline = taskDto.Deadline?.ToUniversalTime() ?? deadlineFromTitle;
            var now = DateTime.UtcNow;

            var task = new TaskModel
            {
                Title = title,
                Description = taskDto.Description,
                Deadline = deadline,
                Priority = taskDto.Priority.HasValue ? taskDto.Priority.Value : TaskPriority.Medium,

                Status = (deadline.HasValue && now > deadline.Value) ? StatusTask.Overdue : StatusTask.Active,
                CreatedAt = now
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
            TaskPriority? priorityFromTitle = ExtractPriorityFromTitle(ref title);
            DateTime? deadlineFromTitle = ExtractDeadlineFromTitle(ref title);

            if (string.IsNullOrWhiteSpace(title) || title.Length < 4)
            {
                throw new ArgumentException("Название задачи должно содержать минимум 4 символа после удаления макросов.");
            }

            var deadline = taskDto.Deadline?.ToUniversalTime() ?? deadlineFromTitle ?? task.Deadline;
            var now = DateTime.UtcNow;

            task.Title = title;
            task.Description = taskDto.Description;
            task.Deadline = deadline;
            task.Priority = taskDto.Priority.HasValue ? taskDto.Priority.Value : TaskPriority.Medium;

            task.UpdatedAt = now;
            task.Status = (deadline.HasValue && now > deadline.Value) ? StatusTask.Overdue : StatusTask.Active;

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
                return null; // Задача не найдена
            }

            // Проверка, если задача уже завершена
            if (task.Status == StatusTask.Completed || task.Status == StatusTask.Late)
            {
                throw new InvalidOperationException("Task is already completed."); // Ошибка, если задача уже завершена
            }

            var now = DateTime.UtcNow;

            if (task.Deadline.HasValue && now > task.Deadline.Value)
            {
                task.Status = StatusTask.Late; // Если просрочена, меняем статус на Late
            }
            else
            {
                task.Status = StatusTask.Completed; // Если не просрочена, меняем на Completed
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel> MarkTaskAsActiveAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null; // Задача не найдена
            }

            // Проверка, если задача уже активна
            if (task.Status == StatusTask.Active || task.Status == StatusTask.Overdue)
            {
                throw new InvalidOperationException("Task is already active."); // Ошибка, если задача уже активна
            }

            var now = DateTime.UtcNow;

            if (task.Status == StatusTask.Overdue)
            {
                task.Status = StatusTask.Late; // Если задача просрочена, меняем на Late
            }
            else if (task.Deadline.HasValue && now > task.Deadline.Value)
            {
                task.Status = StatusTask.Overdue; // Если срок прошел, меняем на Overdue
            }
            else
            {
                task.Status = StatusTask.Active; // Если все в порядке, меняем на Active
            }

            await _context.SaveChangesAsync();
            return task;
        }



    }
}
