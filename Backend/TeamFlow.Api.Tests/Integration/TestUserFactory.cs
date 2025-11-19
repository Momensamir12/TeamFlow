using App.Domain.Model;

namespace TeamFlow.Api.Tests.Integration;

public static class TestUserFactory
{
    public static User CreateTestUser(string? username = null, string? email = null)
    {
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = username ?? "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = email ?? "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        return testUser;
    }
};