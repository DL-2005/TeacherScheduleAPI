using System.ComponentModel.DataAnnotations;

namespace TeacherScheduleAPI.Entity
{
    public class ChangePasswordRequest
    {
        [Required]
        public string MatKhauCu { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string MatKhauMoi { get; set; }

        [Required]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhauMoi { get; set; }
    }
}