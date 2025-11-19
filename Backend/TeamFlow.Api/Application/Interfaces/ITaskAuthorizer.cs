using App.Domain.Model;
using System.Security.Claims;

namespace App.Application.Interfaces;

public interface ITaskAuthorizer
{
    Task<bool> CanModifyAsync(UserTask task);
    Task<bool> CanAccessAsync(UserTask task);
    Task EnsureCanModifyAsync(UserTask task);
    Task EnsureCanAccessAsync(UserTask task);
}