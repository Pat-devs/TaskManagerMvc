namespace TaskManager.Core.Models;

public interface ITaskContext
{
    int Count { get; }
    IUserTask AddTask(
        string title,
        string description,
        DateTime dueDate
    );

    List<IUserTask> GetAllTasks();
    IUserTask? GetTaskById(int id);
    bool CompleteTask(int id);
}