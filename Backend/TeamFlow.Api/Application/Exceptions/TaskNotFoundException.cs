namespace App.Application.Exceptions;

public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(Guid taskId) 
        : base($"Task with id '{taskId}' not found.") { }
}