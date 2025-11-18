using App.Application.Data;
using App.Application.MappingProfiles;
using App.Domain.Model;
using App.Infrastructure.Data;
using AutoMapper;

namespace TeamFlow.Api.Tests.Integration;

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly AppDbContext AppDbContext;
    protected readonly SecurityDbContext SecurityDbContext;
    protected readonly IMapper Mapper;
    protected readonly User TestUser;

    protected BaseIntegrationTest()
    {
        AppDbContext = TestDbContextFactory.CreateInMemoryAppContext();
        SecurityDbContext = TestDbContextFactory.CreateInMemorySecurityContext();
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProjectMappingProfile>();
            cfg.AddProfile<TaskMappingProfile>();
            cfg.AddProfile<WorkspaceMappingProfile>();
        });
        Mapper = config.CreateMapper();

        TestUser = TestUserFactory.CreateTestUser();
        AppDbContext.Users.Add(TestUser);
        AppDbContext.SaveChanges();
    }

    public void Dispose()
    {
        AppDbContext.Dispose();
        SecurityDbContext.Dispose();
    }
}
