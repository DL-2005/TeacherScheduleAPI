using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using Npgsql; // Thư viện PostgreSQL

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH DATABASE THÔNG MINH (QUAN TRỌNG) ---
// Kiểm tra xem có biến môi trường DATABASE_URL không (Do Render cung cấp)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // === TRƯỜNG HỢP 1: Đang chạy trên Render/Railway (Dùng PostgreSQL) ===
    Console.WriteLine("--> Đang chạy trên Cloud: Sử dụng PostgreSQL");

    // Phân tích chuỗi kết nối của Render để nạp vào Npgsql
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
        TrustServerCertificate = true // Render dùng chứng chỉ tự ký nên cần dòng này
    };

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builderDb.ToString()));
}
else
{
    // === TRƯỜNG HỢP 2: Đang chạy trên máy cá nhân (Dùng SQL Server) ===
    Console.WriteLine("--> Đang chạy Local: Sử dụng SQL Server");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Server=(localdb)\\mssqllocaldb;Database=ScheduleDB;Trusted_Connection=True;MultipleActiveResultSets=true";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// --- 2. CẤU HÌNH CORS (Kết nối Frontend) ---
// Tạm thời cho phép tất cả (AllowAnyOrigin) để dễ test, sau này siết lại sau
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- 3. CÁC DỊCH VỤ CƠ BẢN ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 4. MIDDLEWARE ---

// Cho phép Swagger hiện thị ngay cả trên Render (để giảng viên chấm bài dễ hơn)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Kích hoạt CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// --- 5. TỰ ĐỘNG KHỞI TẠO DATABASE (AUTO MIGRATION) ---
// Đoạn này giúp tạo bảng tự động khi deploy lên Render mà không cần gõ lệnh
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Lệnh này tương đương với 'Update-Database'
        context.Database.Migrate();
        Console.WriteLine("--> Đã cập nhật Database thành công!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> Lỗi khi tạo Database: " + ex.Message);
    }
}

app.Run();