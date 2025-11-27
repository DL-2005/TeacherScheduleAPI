using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_Lop")]
    public class Lop
    {
        [Key]
        [StringLength(10)]
        public string MaLop { get; set; }

        public int SiSo { get; set; }

        [StringLength(10)]
        public string? MaKhoa { get; set; } // Khóa ngoại trỏ về Khoa
        [ForeignKey("MaKhoa")]
        public Khoa? Khoa { get; set; }

        public string? Nganh { get; set; }
        public string? NamHoc { get; set; }
    }
}