using TaskManager.Core.Models;

namespace TaskManager.Tests;

public class TaskContextTests
{
    [Fact]
    public void AddTask_AddsTaskToContext()
    {
        // Arrange
        var context = new TaskContext();
        // Act
        var task = context.AddTask(
            "Lære MVC",
            "Bygge TaskManager",
            new DateTime(2030, 1, 1)
        );
        // Assert
        Assert.Equal(1, context.Count);
        Assert.Equal(1, task.Id);
    }
    [Fact]
    public void AddTask_GeneratesUniqueIds()
    {
        var context = new TaskContext();

        var first = context.AddTask(
            "Task 1",
            "",
            new DateTime(2030, 1, 1)
        );
        var second = context.AddTask(
            "Task 2",
            "",
            new DateTime(2030, 2, 1)
        );

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }
    [Fact]
    public void CompleteTask_MarkTasksAsCompleted()
    {
        var context = new TaskContext();

        var task = context.AddTask(
            "Lære testing",
            "",
            new DateTime(2030, 1, 1)
        );

        var result = context.CompleteTask(task.Id);

        Assert.True(result);
        Assert.True(task.IsCompleted);
    }
    [Fact]
    public void CompleteTask_ReturnsFalse_WhenTaskDoesNotExist()
    {
        var context = new TaskContext();
        var result = context.CompleteTask(999);
        Assert.False(result);
    }
}