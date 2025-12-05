using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Services;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// 1) CẤU HÌNH DATABASE
// ======================
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // → Chạy trên Render (PostgreSQL)
    Console.WriteLine("--> Running on Render: Using PostgreSQL");

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    var cs = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ToString();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(cs));
}
else
{
    // → Chạy Local (SQL Server)
    Console.WriteLine("--> Running Local: Using SQL Server");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=ScheduleDB;Trusted_Connection=True;TrustServerCertificate=True";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// ======================
// 2) JWT AUTH
// ======================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ======================
// 3) CORS
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ======================
// 4) Controllers + Swagger
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================
// 5) Custom Services
// ======================
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

// ======================
// 6) Middleware
// ======================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ======================
// 7) Auto-Migrate
// ======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
