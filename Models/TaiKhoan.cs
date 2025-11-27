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
        public string MatKhau { get; set; } // Lưu ý: Thực tế nên mã hóa MD5/Bcrypt

        [StringLength(10)]
        public string? MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        [Required]
        [StringLength(3)]
        public string ChucVu { get; set; } // 'GV' hoặc 'CQC'
    }
}