using TaskManager.Core.Models;

namespace TaskManager.Core.Views;

public interface IViewGenerator
{
    void DisplayMainMenu();
    void DisplayTask(
        IEnumerable<IUserTask> tasks,
        string header
    );
    void DisplayMessage(string message);
    string GetInput(string prompt);
    int GetIntInput(string prompt);
    DateTime GetDateInput(string prompt);
    void WaitForKey();
}