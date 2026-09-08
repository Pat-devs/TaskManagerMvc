namespace TaskManager.Core.Models;

public class UserTask : IUserTask
{
    public UserTask(
        int id,
        string title,
        string description,
        DateTime dueDate)
    {
        Id = id;
        Title = title;
        Description = description;
        DueDate = dueDate;
    }

    public int Id { get; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; private set; }
    public DateTime DueDate { get; set; }
    public void MarkAsCompleted()
    {
        throw new NotImplementedException();
    }
}