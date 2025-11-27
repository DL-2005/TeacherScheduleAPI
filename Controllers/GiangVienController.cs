using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiangVienController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GiangVienController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiangVien>>> GetGiangViens()
        {
            // Include Khoa để hiện tên Khoa của giảng viên
            return await _context.GiangViens.Include(g => g.Khoa).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GiangVien>> GetGiangVien(string id)
        {
            var giangVien = await _context.GiangViens.FindAsync(id);
            if (giangVien == null) return NotFound();
            return giangVien;
        }

        [HttpPost]
        public async Task<ActionResult<GiangVien>> PostGiangVien(GiangVien giangVien)
        {
            // Kiểm tra trùng mã GV
            if (_context.GiangViens.Any(x => x.MaGV == giangVien.MaGV))
                return BadRequest("Mã giảng viên đã tồn tại.");
            // Tự động viết hoa Mã GV và Mã Khoa
            giangVien.MaGV = giangVien.MaGV?.ToUpper();
            giangVien.MaKhoa = giangVien.MaKhoa?.ToUpper();
            // --- THÊM DÒNG NÀY ĐỂ SỬA LỖI TRIỆT ĐỂ ---
            // Ngắt bỏ đối tượng Khoa đi kèm, chỉ lấy MaKhoa để liên kết
            giangVien.Khoa = null;
            // -----------------------------------------

            _context.GiangViens.Add(giangVien);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Hiện lỗi rõ hơn nếu có
            }

            return CreatedAtAction("GetGiangVien", new { id = giangVien.MaGV }, giangVien);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGiangVien(string id)
        {
            var giangVien = await _context.GiangViens.FindAsync(id);
            if (giangVien == null) return NotFound();

            _context.GiangViens.Remove(giangVien);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}