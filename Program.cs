using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserManagementAPI.Data;
using UserManagementAPI.Repositories;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UserManagementAPI.Middleware;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//using Middleware.ErrorHandlerMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Corrected this line

var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (string.IsNullOrEmpty(secretKey))
{
    secretKey = "my_secret_key"; // Provide a default secret key if not set in the environment
}
if (string.IsNullOrEmpty(secretKey))
{
    throw new ArgumentNullException(nameof(secretKey), "Secret key must not be null or empty.");
}
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "your_issuer_here",
        ValidAudience = "your_audience_here",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagementAPI v1"));
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add this line to enable authentication middleware
app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
