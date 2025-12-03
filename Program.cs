using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH DATABASE THÔNG MINH ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("--> Đang chạy trên Cloud: Sử dụng PostgreSQL");

    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var builderDb = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builderDb.ToString()));
}
else
{
    Console.WriteLine("--> Đang chạy Local: Sử dụng SQL Server");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Server=(localdb)\\mssqllocaldb;Database=ScheduleDB;Trusted_Connection=True;MultipleActiveResultSets=true";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// --- 2. CẤU HÌNH JWT AUTHENTICATION ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyMinimum32Characters!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TeacherScheduleAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TeacherScheduleAPIUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// --- 3. CẤU HÌNH CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- 4. CÁC DỊCH VỤ CƠ BẢN ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Cấu hình Swagger để hỗ trợ JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 5. MIDDLEWARE ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// *** QUAN TRỌNG: Thứ tự phải đúng ***
app.UseAuthentication(); // Phải đặt trước UseAuthorization
app.UseAuthorization();

app.MapControllers();

// --- 6. TỰ ĐỘNG MIGRATION ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Lệnh này tương đương với 'Update-Database'
        context.Database.Migrate();
        Console.WriteLine("--> Đã cập nhật Database thành công!");
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> Lỗi khi tạo Database: " + ex.Message);
    }
}

app.Run();