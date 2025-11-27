using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_Khoa")] // Ánh xạ vào bảng tb_Khoa
    public class Khoa
    {
        [Key]
        [StringLength(10)]
        public string MaKhoa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKhoa { get; set; }

        public string? Email { get; set; }
        public string? DienThoai { get; set; }
    }
}