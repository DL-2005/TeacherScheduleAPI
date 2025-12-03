using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonHocController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MonHocController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MonHoc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonHoc>>> GetMonHocs()
        {
            return await _context.MonHocs.ToListAsync();
        }

        // GET: api/MonHoc/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MonHoc>> GetMonHoc(string id)
        {
            var monHoc = await _context.MonHocs.FindAsync(id);
            if (monHoc == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            return monHoc;
        }

        // POST: api/MonHoc
        [HttpPost]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult<MonHoc>> PostMonHoc(MonHoc monHoc)
        {
            monHoc.MaMH = monHoc.MaMH?.ToUpper();

            if (await _context.MonHocs.AnyAsync(x => x.MaMH == monHoc.MaMH))
                return BadRequest(new { message = "Mã môn học đã tồn tại" });

            _context.MonHocs.Add(monHoc);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonHoc", new { id = monHoc.MaMH }, monHoc);
        }

        // PUT: api/MonHoc/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> PutMonHoc(string id, MonHoc monHoc)
        {
            if (id != monHoc.MaMH)
                return BadRequest(new { message = "Mã môn học không khớp" });

            _context.Entry(monHoc).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.MonHocs.AnyAsync(e => e.MaMH == id))
                    return NotFound(new { message = "Không tìm thấy môn học" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/MonHoc/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> DeleteMonHoc(string id)
        {
            var monHoc = await _context.MonHocs.FindAsync(id);
            if (monHoc == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            // Kiểm tra xem môn học có đang được phân công không
            var hasPhanCong = await _context.PhanCongs.AnyAsync(pc => pc.MaMH == id);
            if (hasPhanCong)
            {
                return BadRequest(new { message = "Không thể xóa môn học đang có phân công giảng dạy" });
            }

            _context.MonHocs.Remove(monHoc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}