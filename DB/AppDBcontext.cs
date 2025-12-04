using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ==================== CÁC BẢNG CŨ ====================
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<BoMon> BoMons { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<MonHoc> MonHocs { get; set; }
        public DbSet<Lop> Lops { get; set; }
        public DbSet<PhanCong> PhanCongs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }

        // ==================== CÁC BẢNG MỚI ====================
        public DbSet<NghienCuuKhoaHoc> NghienCuuKhoaHocs { get; set; }
        public DbSet<BoiDuong> BoiDuongs { get; set; }
        public DbSet<NhiemVuKhac> NhiemVuKhacs { get; set; }
        public DbSet<DinhMuc> DinhMucs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== CẤU HÌNH TaiKhoan ====================
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

            // ==================== CẤU HÌNH NCKH ====================
            modelBuilder.Entity<NghienCuuKhoaHoc>()
                .HasOne(n => n.GiangVien)
                .WithMany()
                .HasForeignKey(n => n.MaGV)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== CẤU HÌNH BoiDuong ====================
            modelBuilder.Entity<BoiDuong>()
                .HasOne(b => b.GiangVien)
                .WithMany()
                .HasForeignKey(b => b.MaGV)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== CẤU HÌNH NhiemVuKhac ====================
            modelBuilder.Entity<NhiemVuKhac>()
                .HasOne(n => n.GiangVien)
                .WithMany()
                .HasForeignKey(n => n.MaGV)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}