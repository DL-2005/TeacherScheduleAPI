using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_DinhMuc")]
    public class DinhMuc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string NamHoc { get; set; } // "2023-2024"

        // Định mức giảng dạy
        public decimal DinhMucGiangDay { get; set; } // Ví dụ: 270 tiết chuẩn

        // Định mức NCKH
        public decimal DinhMucNCKH { get; set; } // Ví dụ: 150 giờ

        // Định mức bồi dưỡng/sinh hoạt chuyên môn
        public decimal DinhMucBoiDuong { get; set; } // Ví dụ: 80 giờ

        // Hệ số quy đổi (nếu cần)
        public decimal? HeSoLopDong { get; set; } // Hệ số lớp đông
        public decimal? HeSoThucHanh { get; set; } // Hệ số thực hành

        public string? GhiChu { get; set; }
    }
}