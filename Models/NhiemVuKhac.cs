using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_NhiemVuKhac")]
    public class NhiemVuKhac
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
        [StringLength(100)]
        public string CongViec { get; set; } // "Dự giờ", "Coi thi", "Chấm thi", "Hướng dẫn TTSP"...

        [StringLength(200)]
        public string? ChiTiet { get; set; } // Ví dụ: "Một lớp nào đó"

        public int SoGio { get; set; } // Số giờ chuyên môn

        [StringLength(20)]
        public string? NamHoc { get; set; }

        public DateTime? NgayThucHien { get; set; }

        public string? GhiChu { get; set; }
    }
}