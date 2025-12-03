using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;
using TeacherScheduleAPI.Services;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "CQC")] // Chỉ admin mới được quản lý tài khoản
    public class TaiKhoanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TaiKhoan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaiKhoan>>> GetAll()
        {
            return await _context.TaiKhoans
                .Include(tk => tk.GiangVien)
                .ToListAsync();
        }

        // GET: api/TaiKhoan/{maTK}
        [HttpGet("{maTK}")]
        public async Task<ActionResult<TaiKhoan>> Get(string maTK)
        {
            var taiKhoan = await _context.TaiKhoans
                .Include(tk => tk.GiangVien)
                .FirstOrDefaultAsync(tk => tk.MaTK == maTK);

            if (taiKhoan == null)
                return NotFound();

            return taiKhoan;
        }

        // PUT: api/TaiKhoan/{maTK}
        [HttpPut("{maTK}")]
        public async Task<IActionResult> Update(string maTK, TaiKhoan model)
        {
            if (maTK != model.MaTK)
                return BadRequest("Mã tài khoản không khớp");

            // Không cho phép cập nhật mật khẩu qua API này (dùng ChangePassword)
            var existingTK = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(tk => tk.MaTK == maTK);
            if (existingTK == null)
                return NotFound();

            model.MatKhau = existingTK.MatKhau; // Giữ nguyên mật khẩu cũ

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TaiKhoans.AnyAsync(e => e.MaTK == maTK))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/TaiKhoan/{maTK}
        [HttpDelete("{maTK}")]
        public async Task<IActionResult> Delete(string maTK)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);
            if (taiKhoan == null)
                return NotFound();

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/TaiKhoan/{maTK}/ResetPassword
        [HttpPost("{maTK}/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string maTK, [FromBody] string newPassword)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);
            if (taiKhoan == null)
                return NotFound();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự");

            taiKhoan.MatKhau = PasswordService.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reset mật khẩu thành công" });
        }
    }
}