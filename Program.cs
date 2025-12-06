using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Services;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// 0) PORT CONFIG FOR RENDER
// ======================
var renderPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");


// ======================
// 1) DATABASE CONFIG
// ======================
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // → Running on Render
    Console.WriteLine("--> Running on Render: Using PostgreSQL");

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);

    // Fix lỗi Port = -1 (Render không gửi port trong URL)
    int port = uri.Port == -1 ? 5432 : uri.Port;

    var connStr = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ToString();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connStr));
}
else
{
    // → Running Local
    Console.WriteLine("--> Running Local: Using PostgreSQL (Local Connection String)");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=schedule_local;Username=postgres;Password=admin";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
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
// 4) CONTROLLERS + SWAGGER
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ======================
// 5) CUSTOM SERVICES
// ======================
builder.Services.AddScoped<IExportService, ExportService>();


var app = builder.Build();


// ======================
// 6) MIDDLEWARE PIPELINE
// ======================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ======================
// 7) AUTO MIGRATION
// ======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("--> Running database migration...");
        db.Database.Migrate();
        Console.WriteLine("--> Migration complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Migration error: " + ex.Message);
    }
}

// trigger deploy
//help
app.Run();
