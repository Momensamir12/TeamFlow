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
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Only configure Kestrel port for local development
// Azure App Service manages ports automatically via environment variable
var backendUrl = builder.Configuration["AppSettings:BackendUrl"];
if (!string.IsNullOrEmpty(backendUrl) && !builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(new Uri(backendUrl).Port);
    });
}
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
        var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"];
        
        // In production, if no frontend URL is configured, allow any origin temporarily
        // You should configure the actual frontend URL in Azure App Service settings
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            policy.WithOrigins(frontendUrl);
        }
        else if (builder.Environment.IsProduction())
        {
            // Allow all origins in production if frontend URL is not configured
            // This is a temporary solution - set FrontendUrl in Azure configuration
            policy.AllowAnyOrigin();
        }
        else
        {
            // Fallback for local development
            policy.WithOrigins("http://localhost:5173");
        }
        
        policy.AllowAnyHeader()
              .AllowAnyMethod();
        
        // AllowCredentials cannot be used with AllowAnyOrigin
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            policy.AllowCredentials();
        }
        
        policy.WithExposedHeaders("Content-Length", "X-JSON-Response");
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TeamFlow API",
        Version = "v1",
        Description = "Collaborative Task Management Platform API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "TeamFlow"
        }
    });
    
    // Add XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Add JWT bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Configure database based on environment
if (builder.Environment.IsProduction())
{
    // Use SQLite for production
    var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=teamflow.db";
    builder.Services.AddDbContext<SecurityDbContext>(options =>
        options.UseSqlite(sqliteConnection));
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(sqliteConnection));
}
else
{
    // Use MySQL for development
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
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrEmpty(jwtSecretKey))
    {
        throw new InvalidOperationException(
            "JWT Secret Key is not configured. Please set 'JwtSettings:SecretKey' in your configuration or Azure App Service settings.");
    }
    
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamFlow API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationsHub")
    .RequireAuthorization();
app.Run();
