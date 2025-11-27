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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonHoc>>> GetMonHocs()
        {
            return await _context.MonHocs.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<MonHoc>> PostMonHoc(MonHoc monHoc)
        {
            if (_context.MonHocs.Any(x => x.MaMH == monHoc.MaMH))
                return BadRequest("Mã môn học đã tồn tại.");

            _context.MonHocs.Add(monHoc);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetMonHocs", new { id = monHoc.MaMH }, monHoc); // Sửa lại một chút redirect
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonHoc(string id)
        {
            var monHoc = await _context.MonHocs.FindAsync(id);
            if (monHoc == null) return NotFound();

            _context.MonHocs.Remove(monHoc);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}