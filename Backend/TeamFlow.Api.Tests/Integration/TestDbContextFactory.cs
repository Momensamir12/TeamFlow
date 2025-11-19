using App.Application.Data;
using App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TeamFlow.Api.Tests.Integration;

public static class TestDbContextFactory
{
    public static SecurityDbContext CreateInMemorySecurityContext()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new SecurityDbContext(options);
        return context;
    }
    
    public static AppDbContext CreateInMemoryAppContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new AppDbContext(options);
        return context;
    }
}
