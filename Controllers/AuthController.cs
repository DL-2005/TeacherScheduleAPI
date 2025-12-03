using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Entity;
using TeacherScheduleAPI.Models;
using TeacherScheduleAPI.Services;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            // Tìm tài khoản (không phân biệt hoa thường)
            var taiKhoan = await _context.TaiKhoans
                .Include(tk => tk.GiangVien)
                .FirstOrDefaultAsync(tk => tk.MaTK.ToUpper() == request.MaTK.ToUpper());

            if (taiKhoan == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập không tồn tại" });
            }

            // Kiểm tra mật khẩu
            if (!PasswordService.VerifyPassword(request.MatKhau, taiKhoan.MatKhau))
            {
                return Unauthorized(new { message = "Mật khẩu không đúng" });
            }

            // Tạo JWT Token
            var token = GenerateJwtToken(taiKhoan);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            return Ok(new LoginResponse
            {
                MaTK = taiKhoan.MaTK,
                ChucVu = taiKhoan.ChucVu,
                MaGV = taiKhoan.MaGV,
                TenGV = taiKhoan.GiangVien?.TenGV,
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        // POST: api/Auth/Register
        [HttpPost("Register")]
        [Authorize(Roles = "CQC")] // Chỉ admin mới được tạo tài khoản
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            // Kiểm tra tài khoản đã tồn tại
            if (await _context.TaiKhoans.AnyAsync(tk => tk.MaTK == request.MaTK))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            }

            // Validate theo từng chức vụ
            switch (request.ChucVu.ToUpper())
            {
                case "GV": // Giảng viên
                    if (string.IsNullOrEmpty(request.MaGV))
                        return BadRequest(new { message = "Giảng viên phải có Mã GV" });

                    if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == request.MaGV))
                        return BadRequest(new { message = "Mã giảng viên không tồn tại" });

                    if (await _context.TaiKhoans.AnyAsync(tk => tk.MaGV == request.MaGV))
                        return BadRequest(new { message = "Giảng viên này đã có tài khoản" });
                    break;

                case "TK": // Trưởng khoa
                    if (string.IsNullOrEmpty(request.MaKhoa))
                        return BadRequest(new { message = "Trưởng khoa phải có Mã Khoa" });

                    if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == request.MaKhoa))
                        return BadRequest(new { message = "Mã khoa không tồn tại" });

                    if (await _context.TaiKhoans.AnyAsync(tk => tk.MaKhoa == request.MaKhoa && tk.ChucVu == "TK"))
                        return BadRequest(new { message = "Khoa này đã có trưởng khoa" });
                    break;

                case "TBM": // Trưởng bộ môn
                    if (string.IsNullOrEmpty(request.MaGV))
                        return BadRequest(new { message = "Trưởng bộ môn phải là một giảng viên (cần Mã GV)" });

                    if (string.IsNullOrEmpty(request.MaBM))
                        return BadRequest(new { message = "Trưởng bộ môn phải có Mã Bộ Môn" });

                    if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == request.MaGV))
                        return BadRequest(new { message = "Mã giảng viên không tồn tại" });

                    if (!await _context.BoMons.AnyAsync(bm => bm.MaBM == request.MaBM))
                        return BadRequest(new { message = "Mã bộ môn không tồn tại" });

                    if (await _context.TaiKhoans.AnyAsync(tk => tk.MaBM == request.MaBM && tk.ChucVu == "TBM"))
                        return BadRequest(new { message = "Bộ môn này đã có trưởng bộ môn" });
                    break;

                case "CQC": // Cán bộ quản lý
                    // CQC không cần liên kết gì
                    break;

                default:
                    return BadRequest(new { message = "Chức vụ không hợp lệ" });
            }

            // Tạo tài khoản mới
            var taiKhoan = new TaiKhoan
            {
                MaTK = request.MaTK.ToUpper(),
                MatKhau = PasswordService.HashPassword(request.MatKhau),
                ChucVu = request.ChucVu.ToUpper(),
                MaGV = request.MaGV?.ToUpper(),
                MaKhoa = request.MaKhoa?.ToUpper(),
                MaBM = request.MaBM?.ToUpper()
            };

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công" });
        }

        // POST: api/Auth/ChangePassword
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequest request)
        {
            // Lấy MaTK từ token
            var maTK = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maTK))
            {
                return Unauthorized(new { message = "Không xác định được tài khoản" });
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);
            if (taiKhoan == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại" });
            }

            // Kiểm tra mật khẩu cũ
            if (!PasswordService.VerifyPassword(request.MatKhauCu, taiKhoan.MatKhau))
            {
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });
            }

            // Cập nhật mật khẩu mới
            taiKhoan.MatKhau = PasswordService.HashPassword(request.MatKhauMoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }

        // GET: api/Auth/Me - Lấy thông tin user hiện tại
        [HttpGet("Me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            var maTK = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maTK))
            {
                return Unauthorized();
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(tk => tk.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .Include(tk => tk.GiangVien)
                    .ThenInclude(gv => gv.BoMon)
                .Include(tk => tk.Khoa)
                .Include(tk => tk.BoMon)
                    .ThenInclude(bm => bm.Khoa)
                .FirstOrDefaultAsync(tk => tk.MaTK == maTK);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            // Tạo response tùy theo chức vụ
            var response = new
            {
                taiKhoan.MaTK,
                taiKhoan.ChucVu,
                GiangVien = taiKhoan.GiangVien != null ? new
                {
                    taiKhoan.GiangVien.MaGV,
                    taiKhoan.GiangVien.TenGV,
                    taiKhoan.GiangVien.Email,
                    taiKhoan.GiangVien.SDT,
                    taiKhoan.GiangVien.MaKhoa,
                    TenKhoa = taiKhoan.GiangVien.Khoa?.TenKhoa,
                    taiKhoan.GiangVien.MaBM,
                    TenBoMon = taiKhoan.GiangVien.BoMon?.TenBM
                } : null,
                Khoa = taiKhoan.Khoa != null ? new
                {
                    taiKhoan.Khoa.MaKhoa,
                    taiKhoan.Khoa.TenKhoa,
                    taiKhoan.Khoa.Email,
                    taiKhoan.Khoa.DienThoai
                } : null,
                BoMon = taiKhoan.BoMon != null ? new
                {
                    taiKhoan.BoMon.MaBM,
                    taiKhoan.BoMon.TenBM,
                    taiKhoan.BoMon.MaKhoa,
                    TenKhoa = taiKhoan.BoMon.Khoa?.TenKhoa
                } : null
            };

            return Ok(response);
        }
     
        // Hàm tạo JWT Token
        private string GenerateJwtToken(TaiKhoan taiKhoan)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyMinimum32Characters!!")
            );
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTK),
                new Claim(ClaimTypes.Role, taiKhoan.ChucVu),
            };

            // Thêm claim tùy theo chức vụ
            if (!string.IsNullOrEmpty(taiKhoan.MaGV))
                claims.Add(new Claim("MaGV", taiKhoan.MaGV));

            if (!string.IsNullOrEmpty(taiKhoan.MaKhoa))
                claims.Add(new Claim("MaKhoa", taiKhoan.MaKhoa));

            if (!string.IsNullOrEmpty(taiKhoan.MaBM))
                claims.Add(new Claim("MaBM", taiKhoan.MaBM));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "TeacherScheduleAPI",
                audience: _configuration["Jwt:Audience"] ?? "TeacherScheduleAPIUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}