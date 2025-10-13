
namespace DevBoard.Domain.Enums
{
    public enum TaskItemStatus
    {
        Todo = 0,
        InProgress = 1,
        Review = 2,
        Done = 3,
        Blocked = 4
    }

    public enum TaskItemPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
