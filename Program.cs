using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using backend.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------- giới hạn request ----------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("api", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 20,
                TokensPerPeriod = 5,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }
        )
    );

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers["Retry-After"] = "1";
        await context.HttpContext.Response.WriteAsync("Too many requests");
    };
});

// ------------------------------------- nextJs ----------------------------------
builder.Services.AddCors(options =>  // cho phép FrontEnd đọc 
{
    options.AddPolicy("AllowNextJs",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
// -------------------------------------------------------------------------------

// ---------------------------------- database -----------------------------------
builder.Services.AddDbContext<CnpmContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSQL"))); // Kết nối database PostgeSQL
// -------------------------------------------------------------------------------

// ------------------------------------- JWT -------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });
// ------------------------------------------------------------------------------

// ------------------------------------- Services Interface -------------------------------------
builder.Services.AddScoped<ITouristPlaceService, TouristPlaceService>();
builder.Services.AddScoped<ITouristAreaService, TouristAreaService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// ------------------------------------------------------------------------------

var app = builder.Build();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");
app.Run($"http://0.0.0.0:{port}");
app.Run();
