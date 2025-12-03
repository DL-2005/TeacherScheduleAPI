using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LopController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LopController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Lop
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lop>>> GetLops()
        {
            return await _context.Lops.Include(l => l.Khoa).ToListAsync();
        }

        // GET: api/Lop/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Lop>> GetLop(string id)
        {
            var lop = await _context.Lops
                .Include(l => l.Khoa)
                .FirstOrDefaultAsync(l => l.MaLop == id);

            if (lop == null)
                return NotFound(new { message = "Không tìm thấy lớp" });

            return lop;
        }

        // POST: api/Lop
        [HttpPost]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult<Lop>> PostLop(Lop lop)
        {
            lop.MaLop = lop.MaLop?.ToUpper();
            lop.MaKhoa = lop.MaKhoa?.ToUpper();

            if (await _context.Lops.AnyAsync(x => x.MaLop == lop.MaLop))
                return BadRequest(new { message = "Mã lớp đã tồn tại" });

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(lop.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == lop.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            lop.Khoa = null; // Ngắt navigation property

            _context.Lops.Add(lop);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLop", new { id = lop.MaLop }, lop);
        }

        // PUT: api/Lop/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> PutLop(string id, Lop lop)
        {
            if (id != lop.MaLop)
                return BadRequest(new { message = "Mã lớp không khớp" });

            lop.MaKhoa = lop.MaKhoa?.ToUpper();

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(lop.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == lop.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            lop.Khoa = null; // Ngắt navigation property

            _context.Entry(lop).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Lops.AnyAsync(e => e.MaLop == id))
                    return NotFound(new { message = "Không tìm thấy lớp" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Lop/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> DeleteLop(string id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null)
                return NotFound(new { message = "Không tìm thấy lớp" });

            // Kiểm tra xem lớp có đang được phân công không
            var hasPhanCong = await _context.PhanCongs.AnyAsync(pc => pc.MaLop == id);
            if (hasPhanCong)
            {
                return BadRequest(new { message = "Không thể xóa lớp đang có phân công giảng dạy" });
            }

            _context.Lops.Remove(lop);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}