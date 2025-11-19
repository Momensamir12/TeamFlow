using Microsoft.EntityFrameworkCore;
using App.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using Scalar.AspNetCore;
using App.Infrastructure.Settings;
using App.Infrastructure.Auth.Repositories;
using App.Infrastructure.Authentication.Service;
using FluentValidation.AspNetCore;
using FluentValidation;
using App.Application.Authentication.Validations;
using App.Application.Data;
using App.Application.Service;
using App.Infrastructure.Repositories;
using App.Application.Services;
using App.Application.MappingProfiles;
using App.Application.Interfaces;
using App.Infrastructure.Services;
using App.Infrastructure.Authorization.Requirements;
using App.Infrastructure.Authorization.Handlers;
using App.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using App.Api.Middleware;
using System.Text.Json.Serialization;
using App.Application.EventDispatcher;
using App.Application.EventHandler;
using Microsoft.AspNetCore.Identity;
using App.Infrastructure.Auth.Entities;
using App.Application.Dto;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var backendUrl = builder.Configuration["AppSettings:BackendUrl"]!;
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
    
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(new Uri(backendUrl).Port);
});
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));
    
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"]!;
        policy.WithOrigins(frontendUrl) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Length", "X-JSON-Response");
    });
});

builder.Services.AddHttpClient();
builder.Services.AddControllers()
    .AddJsonOptions(o => 
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordDto>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordDto>();

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SecurityDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        ValidateIssuerSigningKey = true

    };
    
    //JWT authentication for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationsHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailVerified", policy =>
        policy.Requirements.Add(new EmailVerifiedRequirement()));

    options.AddPolicy("TaskOwner", policy =>
        policy.Requirements.Add(new TaskOwnerRequirement()));
    
    options.AddPolicy("TaskAccess", policy =>
        policy.Requirements.Add(new TaskAccessRequirement()));
    
    options.AddPolicy("WorkspaceAdmin", policy =>
        policy.Requirements.Add(new WorkspaceAdminRequirement()));
    
    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(new WorkspaceMemberRequirement()));
    
    options.AddPolicy("WorkspaceAccess", policy =>
        policy.Requirements.Add(new WorkspaceAccessRequirement()));
    
    options.AddPolicy("ProjectAdmin", policy =>
        policy.Requirements.Add(new ProjectAdminRequirement()));
    
    options.AddPolicy("ProjectMember", policy =>
        policy.Requirements.Add(new ProjectMemberRequirement()));
    
    options.AddPolicy("ProjectAccess", policy =>
        policy.Requirements.Add(new ProjectAccessRequirement()));
});

// Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, EmailVerifiedHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectAccessHandler>();

// Authorizers
builder.Services.AddScoped<ITaskAuthorizer, TaskAuthorizer>();
builder.Services.AddScoped<IWorkspaceAuthorizer, WorkspaceAuthorizer>();
builder.Services.AddScoped<IProjectAuthorizer, ProjectAuthorizer>();

builder.Services.AddScoped<IProjectRepository, EFProjectRepository>();

builder.Services.AddAutoMapper(typeof(TaskMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(WorkspaceMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(ProjectMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(TaskCommentMappingProfile).Assembly);


builder.Services.AddHttpContextAccessor();

// Password Hasher
builder.Services.AddScoped<IPasswordHasher<SecurityUser>, PasswordHasher<SecurityUser>>();

// Repositories
builder.Services.AddScoped<ISecurityUserRepository, EFSecurityUserRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<ITaskRepository, EFTaskRepository>();
builder.Services.AddScoped<ITaskCommentRepository, EFTaskCommentRepository>();
builder.Services.AddScoped<IWorkspaceRepository, EFWorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceInvitationRepository, EFWorkspaceInvitationRepository>();
builder.Services.AddScoped<IProjectRepository, EFProjectRepository>();  

// Services
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<RegisterationService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TaskCommentService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<UserValidator>();
builder.Services.AddScoped<ITaskAuthorizer, TaskAuthorizer>();
builder.Services.AddScoped<IEmailService, GmailEmailService>();
builder.Services.AddScoped<IWorkspaceAuthorizer, WorkspaceAuthorizer>();
builder.Services.AddScoped<TemplateRenderer>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TaskAssignedEventHandler).Assembly));
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<DomainEventDispatcher>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});



var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationsHub")
    .RequireAuthorization();
app.Run();
