using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo các bảng
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<BoMon> BoMons { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<MonHoc> MonHocs { get; set; }
        public DbSet<Lop> Lops { get; set; }
        public DbSet<PhanCong> PhanCongs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ TaiKhoan để tránh xung đột
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.GiangVien)
                .WithMany()
                .HasForeignKey(tk => tk.MaGV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.Khoa)
                .WithMany()
                .HasForeignKey(tk => tk.MaKhoa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.BoMon)
                .WithMany()
                .HasForeignKey(tk => tk.MaBM)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}