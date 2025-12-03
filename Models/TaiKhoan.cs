using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [StringLength(50)]
        public string MaTK { get; set; }

        [Required]
        public string MatKhau { get; set; }

        [Required]
        [StringLength(10)]
        public string ChucVu { get; set; } // 'GV', 'CQC', 'TK', 'TBM'

        // Nếu là Giảng viên hoặc Trưởng bộ môn
        [StringLength(10)]
        public string? MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        // Nếu là Trưởng khoa
        [StringLength(10)]
        public string? MaKhoa { get; set; }
        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; }

        // Nếu là Trưởng bộ môn
        [StringLength(10)]
        public string? MaBM { get; set; }
        [ForeignKey("MaBM")]
        public BoMon? BoMon { get; set; }
    }
}