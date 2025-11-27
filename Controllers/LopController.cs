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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lop>>> GetLops()
        {
            return await _context.Lops.Include(l => l.Khoa).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Lop>> PostLop(Lop lop)
        {
            // --- CHUẨN HÓA: VIẾT HOA ---
            lop.MaLop = lop.MaLop?.ToUpper();
            lop.MaKhoa = lop.MaKhoa?.ToUpper();
            // ---------------------------
            if (_context.Lops.Any(x => x.MaLop == lop.MaLop))
                return BadRequest("Mã lớp đã tồn tại.");

            _context.Lops.Add(lop);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetLops", new { id = lop.MaLop }, lop);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLop(string id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null) return NotFound();

            _context.Lops.Remove(lop);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}