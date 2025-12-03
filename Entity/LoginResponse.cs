namespace TeacherScheduleAPI.Entity
{
    public class LoginResponse
    {
        public string MaTK { get; set; }
        public string ChucVu { get; set; }
        public string? MaGV { get; set; }
        public string? TenGV { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}