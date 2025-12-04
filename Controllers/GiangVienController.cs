using Microsoft.AspNetCore.Authorization;
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

        // GET: api/GiangVien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiangVien>>> GetGiangViens()
        {
            return await _context.GiangViens
                .Include(g => g.Khoa)
                .Include(g => g.BoMon)
                .ToListAsync();
        }

        // GET: api/GiangVien/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GiangVien>> GetGiangVien(string id)
        {
            var giangVien = await _context.GiangViens
                .Include(g => g.Khoa)
                .Include(g => g.BoMon)
                .FirstOrDefaultAsync(g => g.MaGV == id);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            return giangVien;
        }

        // POST: api/GiangVien
        [HttpPost]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult<GiangVien>> PostGiangVien(GiangVien giangVien)
        {
            giangVien.MaGV = giangVien.MaGV?.ToUpper();
            giangVien.MaKhoa = giangVien.MaKhoa?.ToUpper();
            giangVien.MaBM = giangVien.MaBM?.ToUpper();

            // Kiểm tra trùng mã GV
            if (await _context.GiangViens.AnyAsync(x => x.MaGV == giangVien.MaGV))
                return BadRequest(new { message = "Mã giảng viên đã tồn tại" });

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(giangVien.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == giangVien.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            // Kiểm tra bộ môn tồn tại
            if (!string.IsNullOrEmpty(giangVien.MaBM))
            {
                if (!await _context.BoMons.AnyAsync(bm => bm.MaBM == giangVien.MaBM))
                    return BadRequest(new { message = "Mã bộ môn không tồn tại" });
            }

            // Ngắt navigation property
            giangVien.Khoa = null;
            giangVien.BoMon = null;

            _context.GiangViens.Add(giangVien);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGiangVien", new { id = giangVien.MaGV }, giangVien);
        }

        // PUT: api/GiangVien/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> PutGiangVien(string id, GiangVien giangVien)
        {
            // Chuyển về uppercase để so sánh
            id = id?.ToUpper();
            giangVien.MaGV = giangVien.MaGV?.ToUpper();
            giangVien.MaKhoa = giangVien.MaKhoa?.ToUpper();
            giangVien.MaBM = giangVien.MaBM?.ToUpper();

            // Kiểm tra id khớp với MaGV trong body
            if (id != giangVien.MaGV)
                return BadRequest(new { message = "Mã giảng viên không khớp" });

            // Kiểm tra giảng viên tồn tại
            if (!await _context.GiangViens.AnyAsync(g => g.MaGV == id))
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(giangVien.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == giangVien.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            // Kiểm tra bộ môn tồn tại
            if (!string.IsNullOrEmpty(giangVien.MaBM))
            {
                if (!await _context.BoMons.AnyAsync(bm => bm.MaBM == giangVien.MaBM))
                    return BadRequest(new { message = "Mã bộ môn không tồn tại" });
            }

            // Ngắt navigation property để tránh lỗi
            giangVien.Khoa = null;
            giangVien.BoMon = null;

            _context.Entry(giangVien).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.GiangViens.AnyAsync(e => e.MaGV == id))
                    return NotFound(new { message = "Không tìm thấy giảng viên" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/GiangVien/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> DeleteGiangVien(string id)
        {
            var giangVien = await _context.GiangViens.FindAsync(id);
            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // Kiểm tra xem giảng viên có phân công không
            var hasPhanCong = await _context.PhanCongs.AnyAsync(pc => pc.MaGV == id);
            if (hasPhanCong)
            {
                return BadRequest(new { message = "Không thể xóa giảng viên đang có phân công giảng dạy" });
            }

            // Kiểm tra xem giảng viên có tài khoản không
            var hasTaiKhoan = await _context.TaiKhoans.AnyAsync(tk => tk.MaGV == id);
            if (hasTaiKhoan)
            {
                return BadRequest(new { message = "Không thể xóa giảng viên đang có tài khoản. Vui lòng xóa tài khoản trước" });
            }

            _context.GiangViens.Remove(giangVien);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}