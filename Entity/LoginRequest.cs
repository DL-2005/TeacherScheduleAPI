using System.ComponentModel.DataAnnotations;

namespace TeacherScheduleAPI.Entity
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string MaTK { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string MatKhau { get; set; }
    }
}