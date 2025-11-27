using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhoaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhoaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/khoa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Khoa>>> GetAll()
            => Ok(await _context.Khoas.ToListAsync());

        // GET: api/khoa/{maKhoa}
        [HttpGet("{maKhoa}")]
        public async Task<ActionResult<Khoa>> Get(string maKhoa)
        {
            var item = await _context.Khoas.FindAsync(maKhoa);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/khoa
        [HttpPost]
        public async Task<ActionResult<Khoa>> Create(Khoa model)
        {
            // Thêm dòng này: Tự động viết hoa Mã Khoa (ví dụ: "cntt" -> "CNTT")
            model.MaKhoa = model.MaKhoa?.ToUpper();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Khoas.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { maKhoa = model.MaKhoa }, model);
        }

        // PUT: api/khoa/{maKhoa}
        [HttpPut("{maKhoa}")]
        public async Task<IActionResult> Update(string maKhoa, Khoa model)
        {
            if (maKhoa != model.MaKhoa) return BadRequest();
            _context.Entry(model).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Khoas.AnyAsync(e => e.MaKhoa == maKhoa)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/khoa/{maKhoa}
        [HttpDelete("{maKhoa}")]
        public async Task<IActionResult> Delete(string maKhoa)
        {
            var e = await _context.Khoas.FindAsync(maKhoa);
            if (e == null) return NotFound();
            _context.Khoas.Remove(e);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}