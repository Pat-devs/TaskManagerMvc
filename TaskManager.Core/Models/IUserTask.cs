namespace TaskManager.Core.Models;

public interface IUserTask
{
    int Id { get; }
    string Title { get; set; }
    string Description { get; set; }
    bool IsCompleted { get; }
    DateTime DueDate { get; set; }
    void MarkAsCompleted();
}