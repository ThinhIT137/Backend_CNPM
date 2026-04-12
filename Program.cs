using backend.Hubs;
using backend.Middlewares;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Bỏ qua lỗi vòng lặp vô tận khi convert object lồng nhau sang JSON
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
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
                .WithOrigins("http://localhost:3000", "https://utctrek.vercel.app")
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Nếu request gửi đến Hub và có chứa token trong URL
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                {
                    context.Token = accessToken; // Lấy token bỏ vào context cho .NET xác thực
                }
                return Task.CompletedTask;
            }
        };
    });
// ------------------------------------------------------------------------------

// ------------------------------------- Services Interface -------------------------------------
builder.Services.AddScoped<ITouristPlaceService, TouristPlaceService>();
builder.Services.AddScoped<ITouristAreaService, TouristAreaService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IInteractionService, InteractionService>();
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

app.UseStaticFiles();

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.MapHub<NotificationHub>("/hubs/notification");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");
//app.Run($"http://0.0.0.0:{port}");
app.Run();
