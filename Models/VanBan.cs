using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_VanBan")]
    public class VanBan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SoVanBan { get; set; } // Số văn bản: "01/TB-CNTT"

        [Required]
        [StringLength(500)]
        public string TenVanBan { get; set; }

        [Required]
        [StringLength(50)]
        public string TheLoai { get; set; } // "Thông báo", "Quyết định", "Công văn", "Kế hoạch", "Báo cáo"

        public DateTime NgayBanHanh { get; set; }

        [StringLength(20)]
        public string? NamHoc { get; set; }

        [StringLength(200)]
        public string? CoQuanBanHanh { get; set; } // "Phòng Đào tạo", "Khoa CNTT"...

        [StringLength(200)]
        public string? NguoiKy { get; set; }

        public string? TrichYeu { get; set; } // Trích yếu nội dung

        // File đính kèm
        public string? FilePath { get; set; } // Đường dẫn file lưu trên server
        public string? FileName { get; set; } // Tên file gốc
        public long? FileSize { get; set; } // Kích thước file (bytes)

        // Khóa ngoại - Khoa (nếu văn bản của khoa)
        [StringLength(10)]
        public string? MaKhoa { get; set; }
        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; } // "Công khai", "Nội bộ", "Lưu trữ"

        public DateTime? NgayTao { get; set; } = DateTime.Now;
        public string? NguoiTao { get; set; }
    }
}
