using System.ComponentModel.DataAnnotations;

namespace TeacherScheduleAPI.Entity
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string MaTK { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string MatKhau { get; set; }

        [Required]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression("^(GV|CQC|TK|TBM)$", ErrorMessage = "Chức vụ chỉ có thể là 'GV', 'CQC', 'TK' hoặc 'TBM'")]
        public string ChucVu { get; set; } // GV: Giảng viên, CQC: Cán bộ quản lý, TK: Trưởng khoa, TBM: Trưởng bộ môn

        [StringLength(10)]
        public string? MaGV { get; set; } // Bắt buộc nếu ChucVu = 'GV' hoặc 'TBM'

        [StringLength(10)]
        public string? MaKhoa { get; set; } // Bắt buộc nếu ChucVu = 'TK'

        [StringLength(10)]
        public string? MaBM { get; set; } // Bắt buộc nếu ChucVu = 'TBM'
    }
}