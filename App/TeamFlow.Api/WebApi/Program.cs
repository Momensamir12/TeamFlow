using Microsoft.EntityFrameworkCore;
using App.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // React app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
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


builder.Services.Configure<JwtSettings>(
builder.Configuration.GetSection("JwtSettings"));

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
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TaskOwner", policy =>
        policy.Requirements.Add(new TaskOwnerRequirement()));
    
    options.AddPolicy("TaskAccess", policy =>
        policy.Requirements.Add(new TaskAccessRequirement()));
});

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskAccessHandler>();

builder.Services.AddAutoMapper(typeof(TaskMappingProfile).Assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ISecurityUserRepository, EFSecurityUserRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<ITaskRepository, EFTaskRepository>();

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<RegisterationService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserValidator>();
builder.Services.AddScoped<ITaskAuthorizer, TaskAuthorizer>();


var app = builder.Build();

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
app.Run();
