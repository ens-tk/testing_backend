using testing_back.DTO;
using testing_back.Models;

namespace testing_back.Service
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskModel>> GetAllTasksSortedAsync(string? sortBy);
        Task<TaskModel> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskDTO taskDto);
        Task<TaskModel> UpdateTaskAsync(int id, TaskDTO taskDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<TaskModel> MarkTaskAsCompletedAsync(int id);
        Task<TaskModel> MarkTaskAsActiveAsync(int id);
    }
}
