using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json; // ⬅️ THÊM DÒNG NÀY

namespace TeacherScheduleAPI.Entity
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [JsonProperty("MaTK")] // ⬅️ THÊM THUỘC TÍNH ÁNH XẠ
        public string MaTK { get; set; } = string.Empty; // Gán mặc định

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [JsonProperty("MatKhau")] // ⬅️ THÊM THUỘC TÍNH ÁNH XẠ
        public string MatKhau { get; set; } = string.Empty;
    }
}