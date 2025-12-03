using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoMonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BoMonController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BoMon
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoMon>>> GetAll()
        {
            return await _context.BoMons
                .Include(bm => bm.Khoa)
                .ToListAsync();
        }

        // GET: api/BoMon/{maBM}
        [HttpGet("{maBM}")]
        public async Task<ActionResult<BoMon>> Get(string maBM)
        {
            var boMon = await _context.BoMons
                .Include(bm => bm.Khoa)
                .FirstOrDefaultAsync(bm => bm.MaBM == maBM);

            if (boMon == null)
                return NotFound();

            return boMon;
        }

        // GET: api/BoMon/Khoa/{maKhoa} - Lấy bộ môn theo khoa
        [HttpGet("Khoa/{maKhoa}")]
        public async Task<ActionResult<IEnumerable<BoMon>>> GetByKhoa(string maKhoa)
        {
            return await _context.BoMons
                .Where(bm => bm.MaKhoa == maKhoa)
                .Include(bm => bm.Khoa)
                .ToListAsync();
        }

        // POST: api/BoMon
        [HttpPost]
        [Authorize(Roles = "CQC,TK")] // Admin hoặc Trưởng khoa mới được tạo
        public async Task<ActionResult<BoMon>> Create(BoMon model)
        {
            model.MaBM = model.MaBM?.ToUpper();
            model.MaKhoa = model.MaKhoa?.ToUpper();

            if (await _context.BoMons.AnyAsync(bm => bm.MaBM == model.MaBM))
                return BadRequest(new { message = "Mã bộ môn đã tồn tại" });

            if (!string.IsNullOrEmpty(model.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == model.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            _context.BoMons.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { maBM = model.MaBM }, model);
        }

        // PUT: api/BoMon/{maBM}
        [HttpPut("{maBM}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Update(string maBM, BoMon model)
        {
            if (maBM != model.MaBM)
                return BadRequest();

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BoMons.AnyAsync(e => e.MaBM == maBM))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/BoMon/{maBM}
        [HttpDelete("{maBM}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> Delete(string maBM)
        {
            var boMon = await _context.BoMons.FindAsync(maBM);
            if (boMon == null)
                return NotFound();

            _context.BoMons.Remove(boMon);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}