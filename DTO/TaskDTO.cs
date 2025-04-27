using testing_back.Models;

namespace testing_back.DTO
{
    public class TaskDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public TaskPriority? Priority { get; set; }
    }
}
