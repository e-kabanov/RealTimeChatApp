using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealTimeChatApp.Data;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Extensions;
using RealTimeChatApp.Hubs;
using RealTimeChatApp.Models;
using RealTimeChatApp.Services;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var connectionString = builder.Configuration["ConnectionStrings:DefaultConnectionString"];
builder.Services.AddDbContext<ChatDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSignalR();
builder.Logging.AddConsole();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.Configure<AppDisplaySettings>(builder.Configuration.GetSection("AppSettings"));

//var settings = new AppDisplaySettings();
//builder.Configuration.GetSection("AppSettings").Bind(settings);
//builder.Services.AddSingleton(settings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Для SignalR - токен передается в query string
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
        


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // URL вашего фронтенда
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();



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

app.MapHub<ChatHub>("/chatHub");


app.MapGet("api/users/search/{userName}", UserHandler.GetUserId).RequireAuthorization();
app.MapGet("api/users/{id}", UserHandler.GetUserInfo).RequireAuthorization();


app.MapGet("/settings", (IOptions<AppDisplaySettings> options) => 
{
    AppDisplaySettings settings = options.Value;

    return settings;

});

//app.MapGet("/settings", (AppDisplaySettings settings) => settings);

    






app.Run();

public static class UserHandler
{
    public static async Task<IResult> GetUserId (string userName, ChatDbContext _context)
    {
        if (string.IsNullOrEmpty(userName))
            return Results.BadRequest("Имя пользователя не может быть пустым.");

        var users = await _context.Users
        .Where(u => u.UserName != null && u.UserName.Contains(userName))
        .Select(u => new UserSearchResult(u.Id, u.UserName, u.AvatarUrl))
        .ToListAsync();

        return users.Any() ? Results.Ok(users) : Results.NotFound("Пользователь не найден");

    }

    public static async Task<IResult> GetUserInfo(int id, ChatDbContext _context)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return Results.Problem(detail: "Пользователь не найден", statusCode: 404);

        }

        return Results.Ok(new
        {
            userName = user.UserName,
            avatarUrl = user.AvatarUrl,
            createdAt = user.createdAt,
            email = user.Email
        });
    }

       public record UserSearchResult(int Id, string UserName, string AvatarUrl);

}

 





