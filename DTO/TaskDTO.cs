using testing_back.Models;
using System.Text.Json.Serialization;

namespace testing_back.DTO
{
    public class TaskDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskPriority? Priority { get; set; }
    }
}
