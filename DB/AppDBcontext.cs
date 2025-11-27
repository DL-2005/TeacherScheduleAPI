using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo các bảng khớp với SQL
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<MonHoc> MonHocs { get; set; }
        public DbSet<Lop> Lops { get; set; }
        public DbSet<PhanCong> PhanCongs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
    }
}