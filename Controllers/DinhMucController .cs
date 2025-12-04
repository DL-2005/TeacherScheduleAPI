using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DinhMucController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DinhMucController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DinhMuc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DinhMuc>>> GetAll()
        {
            return await _context.DinhMucs.OrderByDescending(d => d.NamHoc).ToListAsync();
        }

        // GET: api/DinhMuc/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DinhMuc>> Get(int id)
        {
            var item = await _context.DinhMucs.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy định mức" });

            return item;
        }

        // GET: api/DinhMuc/NamHoc/{namHoc}
        [HttpGet("NamHoc/{namHoc}")]
        public async Task<ActionResult<DinhMuc>> GetByNamHoc(string namHoc)
        {
            var item = await _context.DinhMucs.FirstOrDefaultAsync(d => d.NamHoc == namHoc);
            if (item == null)
                return NotFound(new { message = $"Không tìm thấy định mức cho năm học {namHoc}" });

            return item;
        }

        // POST: api/DinhMuc
        [HttpPost]
        [Authorize(Roles = "CQC")]
        public async Task<ActionResult<DinhMuc>> Create(DinhMuc model)
        {
            // Kiểm tra năm học đã tồn tại
            if (await _context.DinhMucs.AnyAsync(d => d.NamHoc == model.NamHoc))
                return BadRequest(new { message = $"Định mức cho năm học {model.NamHoc} đã tồn tại" });

            _context.DinhMucs.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/DinhMuc/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> Update(int id, DinhMuc model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "Id không khớp" });

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.DinhMucs.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/DinhMuc/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DinhMucs.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.DinhMucs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}