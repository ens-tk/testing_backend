using System.ComponentModel.DataAnnotations.Schema;

namespace testing_back.Models
{
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class TaskModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public StatusTask Status { get; set; } = StatusTask.Active;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public void UpdateStatus()
        {
            if (Status == StatusTask.Completed) return;

            if (Deadline.HasValue && Deadline.Value < DateTime.UtcNow)
            {
                Status = StatusTask.Overdue;
            }
            else if (Deadline.HasValue && Deadline.Value.AddDays(3) < DateTime.UtcNow)
            {
                Status = StatusTask.Late;
            }
        }
    }
}
