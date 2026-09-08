using TaskManager.Core.Models; 

namespace TaskManager.Tests;

public class UserTaskTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var dueDate = new DateTime(2030, 1, 1);
        // Act
        var task = new UserTask(
            1,
            "Filme video",
            "Filme MVC-video",
            dueDate
        );
        // Assert
        Assert.Equal(1, task.Id);
        Assert.Equal("Filme video", task.Title);
        Assert.Equal("Filme MVC-video", task.Description);
        Assert.Equal(dueDate, task.DueDate);
        Assert.False(task.IsCompleted);
    }
    [Fact]
    public void MarkAsCompleted_SetsIsCompletedToTrue()
    {
        // Arrange
        var task = new UserTask(
            1,
            "Filme video",
            "Filme MVC-video",
            new DateTime(2030,1,1)
        );

        // Act
        task.MarkAsCompleted();

        // Assert
        Assert.True(task.IsCompleted);
    }
    [Fact]
    public void Constructor_Throws_WhenTitleIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => 
            new UserTask(
                1, 
                "", 
                "Beskrivelse",
                new DateTime(2030,1,1)
            ));
    }
}