using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_NghienCuuKhoaHoc")]
    public class NghienCuuKhoaHoc
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại Giảng viên
        [Required]
        [StringLength(10)]
        public string MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        [Required]
        [StringLength(500)]
        public string TenDeTai { get; set; }

        [Required]
        [StringLength(50)]
        public string TheLoai { get; set; } // "Đề tài cấp cơ sở", "Đề tài cấp TP", "Seminar", "Tạp chí khoa học", "Sách/Giáo trình"

        [StringLength(50)]
        public string? VaiTro { get; set; } // "Chủ nhiệm", "Thành viên chính", "Thành viên"

        public int GioNCKH { get; set; } // Số giờ quy đổi NCKH

        [StringLength(20)]
        public string? NamHoc { get; set; } // Ví dụ: "2023-2024"

        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; } // "Đang thực hiện", "Đã hoàn thành", "Đã nghiệm thu"

        public string? MoTa { get; set; }
        public string? FileMinhChung { get; set; } // Đường dẫn file minh chứng
    }
}