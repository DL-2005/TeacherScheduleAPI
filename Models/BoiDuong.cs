using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_BoiDuong")]
    public class BoiDuong
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
        [StringLength(200)]
        public string NoiDung { get; set; } // Ví dụ: "Bồi dưỡng sử dụng thiết bị dạy học hiện đại"

        [StringLength(200)]
        public string? ChiTiet { get; set; } // Ví dụ: "Học 1 thứ gì đó"

        public int GioBoiDuong { get; set; } // Số giờ bồi dưỡng

        [StringLength(20)]
        public string? NamHoc { get; set; }

        public DateTime? NgayThucHien { get; set; }

        public string? GhiChu { get; set; }
    }
}