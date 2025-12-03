using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanCongController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhanCongController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PhanCong
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhanCong>>> GetPhanCongs()
        {
            return await _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .ToListAsync();
        }

        // GET: api/PhanCong/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhanCong>> GetPhanCong(int id)
        {
            var phanCong = await _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .FirstOrDefaultAsync(pc => pc.Id == id);

            if (phanCong == null)
                return NotFound(new { message = "Không tìm thấy phân công" });

            return phanCong;
        }

        // POST: api/PhanCong
        [HttpPost]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<ActionResult<PhanCong>> PostPhanCong(PhanCong phanCong)
        {
            phanCong.MaGV = phanCong.MaGV?.ToUpper();
            phanCong.MaMH = phanCong.MaMH?.ToUpper();
            phanCong.MaLop = phanCong.MaLop?.ToUpper();

            // 1. Kiểm tra các mã tồn tại
            if (!await _context.GiangViens.AnyAsync(g => g.MaGV == phanCong.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            if (!await _context.MonHocs.AnyAsync(m => m.MaMH == phanCong.MaMH))
                return BadRequest(new { message = "Mã môn học không tồn tại" });

            if (!await _context.Lops.AnyAsync(l => l.MaLop == phanCong.MaLop))
                return BadRequest(new { message = "Mã lớp không tồn tại" });

            // 2. Kiểm tra xung đột lịch GIẢNG VIÊN
            var teacherConflict = await CheckTeacherConflict(phanCong, null);
            if (teacherConflict != null)
                return BadRequest(new { message = teacherConflict });

            // 3. Kiểm tra xung đột PHÒNG HỌC
            if (!string.IsNullOrEmpty(phanCong.PhongHoc))
            {
                var roomConflict = await CheckRoomConflict(phanCong, null);
                if (roomConflict != null)
                    return BadRequest(new { message = roomConflict });
            }

            _context.PhanCongs.Add(phanCong);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhanCong", new { id = phanCong.Id }, phanCong);
        }

        // PUT: api/PhanCong/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<IActionResult> PutPhanCong(int id, PhanCong phanCong)
        {
            if (id != phanCong.Id)
                return BadRequest(new { message = "Id không khớp" });

            phanCong.MaGV = phanCong.MaGV?.ToUpper();
            phanCong.MaMH = phanCong.MaMH?.ToUpper();
            phanCong.MaLop = phanCong.MaLop?.ToUpper();

            // 1. Kiểm tra các mã tồn tại
            if (!await _context.GiangViens.AnyAsync(g => g.MaGV == phanCong.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            if (!await _context.MonHocs.AnyAsync(m => m.MaMH == phanCong.MaMH))
                return BadRequest(new { message = "Mã môn học không tồn tại" });

            if (!await _context.Lops.AnyAsync(l => l.MaLop == phanCong.MaLop))
                return BadRequest(new { message = "Mã lớp không tồn tại" });

            // 2. Kiểm tra xung đột lịch GIẢNG VIÊN (loại trừ chính nó)
            var teacherConflict = await CheckTeacherConflict(phanCong, id);
            if (teacherConflict != null)
                return BadRequest(new { message = teacherConflict });

            // 3. Kiểm tra xung đột PHÒNG HỌC (loại trừ chính nó)
            if (!string.IsNullOrEmpty(phanCong.PhongHoc))
            {
                var roomConflict = await CheckRoomConflict(phanCong, id);
                if (roomConflict != null)
                    return BadRequest(new { message = roomConflict });
            }

            _context.Entry(phanCong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PhanCongs.AnyAsync(e => e.Id == id))
                    return NotFound(new { message = "Không tìm thấy phân công" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/PhanCong/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> DeletePhanCong(int id)
        {
            var phanCong = await _context.PhanCongs.FindAsync(id);
            if (phanCong == null)
                return NotFound(new { message = "Không tìm thấy phân công" });

            _context.PhanCongs.Remove(phanCong);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper: Kiểm tra xung đột lịch giảng viên
        private async Task<string?> CheckTeacherConflict(PhanCong phanCong, int? excludeId)
        {
            var query = _context.PhanCongs.Where(pc =>
                pc.MaGV == phanCong.MaGV &&
                pc.Thu == phanCong.Thu &&
                (
                    (phanCong.TietBatDau >= pc.TietBatDau && phanCong.TietBatDau < (pc.TietBatDau + pc.SoTiet)) ||
                    ((phanCong.TietBatDau + phanCong.SoTiet) > pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) <= (pc.TietBatDau + pc.SoTiet)) ||
                    (phanCong.TietBatDau <= pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) >= (pc.TietBatDau + pc.SoTiet))
                )
            );

            // Loại trừ chính nó khi update
            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            var hasConflict = await query.AnyAsync();

            return hasConflict ? "Giảng viên đã có lịch dạy trùng vào thời gian này!" : null;
        }

        // Helper: Kiểm tra xung đột phòng học
        private async Task<string?> CheckRoomConflict(PhanCong phanCong, int? excludeId)
        {
            var query = _context.PhanCongs.Where(pc =>
                pc.PhongHoc == phanCong.PhongHoc &&
                pc.Thu == phanCong.Thu &&
                (
                    (phanCong.TietBatDau >= pc.TietBatDau && phanCong.TietBatDau < (pc.TietBatDau + pc.SoTiet)) ||
                    ((phanCong.TietBatDau + phanCong.SoTiet) > pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) <= (pc.TietBatDau + pc.SoTiet)) ||
                    (phanCong.TietBatDau <= pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) >= (pc.TietBatDau + pc.SoTiet))
                )
            );

            // Loại trừ chính nó khi update
            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            var hasConflict = await query.AnyAsync();

            return hasConflict ? $"Phòng {phanCong.PhongHoc} đã có lịch sử dụng trùng vào thời gian này!" : null;
        }
    }
}