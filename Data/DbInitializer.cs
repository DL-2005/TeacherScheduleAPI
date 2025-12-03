using TeacherScheduleAPI.Models;
using TeacherScheduleAPI.Services;

namespace TeacherScheduleAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            Console.WriteLine("--> Checking database...");

            // Kiểm tra đã có admin chưa
            if (context.TaiKhoans.Any(tk => tk.ChucVu == "CQC"))
            {
                Console.WriteLine("--> Admin account already exists. Skipping seed.");
                return;
            }

            Console.WriteLine("--> Creating default admin account...");

            // Tạo duy nhất 1 tài khoản ADMIN
            var admin = new TaiKhoan
            {
                MaTK = "ADMIN",
                MatKhau = PasswordService.HashPassword("admin123"), // Mật khẩu mặc định
                ChucVu = "CQC",
                MaGV = null,
                MaKhoa = null,
                MaBM = null
            };

            context.TaiKhoans.Add(admin);
            context.SaveChanges();

            Console.WriteLine("--> ✅ Admin account created successfully!");
            Console.WriteLine("--> ========================================");
            Console.WriteLine("--> DEFAULT ADMIN CREDENTIALS:");
            Console.WriteLine("--> Username: ADMIN");
            Console.WriteLine("--> Password: admin123");
            Console.WriteLine("--> ⚠️  PLEASE CHANGE PASSWORD AFTER FIRST LOGIN!");
            Console.WriteLine("--> ========================================");
        }
    }
}