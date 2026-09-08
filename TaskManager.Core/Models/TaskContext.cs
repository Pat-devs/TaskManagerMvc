namespace TaskManager.Core.Models;

public class TaskContext : ITaskContext
{
    private readonly List<IUserTask> _tasks = new();
    private int _nextId;
    public int Count => _tasks.Count();
    public IUserTask AddTask(
        string title,
        string description,
        DateTime dueDate)

    {
        var task = UserTask(
            ++_nextId,
            title,
            description,
            dueDate);

        _tasks.Add(task);

        return task;
    }

    public List<IUserTask> GetAllTasks()
    {
        return new List<IUserTask>(_tasks);
    }
    public IUserTask? GetTaskById(int id)
    {
        return _tasks.FirstOrDefault(task => task.Id == id);
    }
    public bool CompleteTaskint (int id)
    {
        var task = GetTaskById(id);
        if (task is null)
        {
            return false;
        }

        task.MarkAsCompleted();

        return true;
    }

}