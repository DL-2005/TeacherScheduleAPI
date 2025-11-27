using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_MonHoc")]
    public class MonHoc
    {
        [Key]
        [StringLength(10)]
        public string MaMH { get; set; } // Sửa thành String cho khớp SQL

        [Required]
        [StringLength(100)]
        public string TenMH { get; set; }

        public int SoTinChi { get; set; }
        public string? HeDaoTao { get; set; }
    }
}