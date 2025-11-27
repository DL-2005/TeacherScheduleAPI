using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_GiangVien")]
    public class GiangVien
    {
        [Key]
        [StringLength(10)]
        public string MaGV { get; set; }

        [Required]
        [StringLength(100)]
        public string TenGV { get; set; }

        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public string? SDT { get; set; }
        public string? Email { get; set; }

        // --- BẠN ĐANG THIẾU ĐOẠN NÀY ---
        [StringLength(10)]
        public string? MaKhoa { get; set; } // Khóa ngoại lưu mã

        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; } // Object để dùng lệnh .Include()
        // --------------------------------
    }
}