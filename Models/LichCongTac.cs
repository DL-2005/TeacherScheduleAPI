using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_LichCongTac")]
    public class LichCongTac
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime NgayThang { get; set; }

        public TimeSpan? ThoiGianBatDau { get; set; }
        public TimeSpan? ThoiGianKetThuc { get; set; }

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; }

        [StringLength(200)]
        public string? DiaDiem { get; set; }

        [StringLength(500)]
        public string? ThanhPhan { get; set; } // Thành phần tham dự

        [StringLength(200)]
        public string? ChuTri { get; set; } // Người chủ trì

        // Khóa ngoại - Người phụ trách (nếu có)
        [StringLength(10)]
        public string? MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        // Khóa ngoại - Khoa (nếu lịch công tác của khoa)
        [StringLength(10)]
        public string? MaKhoa { get; set; }
        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; }

        [StringLength(50)]
        public string? LoaiLich { get; set; } // "Họp", "Hội nghị", "Tập huấn", "Khác"

        [StringLength(50)]
        public string? TrangThai { get; set; } // "Đã lên lịch", "Đang diễn ra", "Hoàn thành", "Hủy"

        public string? GhiChu { get; set; }

        public DateTime? NgayTao { get; set; } = DateTime.Now;
    }
}
