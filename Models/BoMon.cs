using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_BoMon")]
    public class BoMon
    {
        [Key]
        [StringLength(10)]
        public string MaBM { get; set; }

        [Required]
        [StringLength(100)]
        public string TenBM { get; set; }

        [StringLength(10)]
        public string? MaKhoa { get; set; } // Bộ môn thuộc khoa nào

        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; }

        public string? MoTa { get; set; }
    }
}