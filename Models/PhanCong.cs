using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_PhanCong")]
    public class PhanCong
    {
        [Key]
        public int Id { get; set; } // Tự tăng

        // Khóa ngoại GV
        [StringLength(10)]
        public string MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        // Khóa ngoại Môn Học
        [StringLength(10)]
        public string MaMH { get; set; }
        [ForeignKey("MaMH")]
        public MonHoc? MonHoc { get; set; }

        // Khóa ngoại Lớp
        [StringLength(10)]
        public string MaLop { get; set; }
        [ForeignKey("MaLop")]
        public Lop? Lop { get; set; }

        public int TietBatDau { get; set; }
        public int SoTiet { get; set; }
        public int Thu { get; set; } // 2 đến 8
        public string? ThoiGianHoc { get; set; } // Ví dụ: "HK1-2024"
        public string? PhongHoc { get; set; }
        public string? GhiChu { get; set; }
    }
}