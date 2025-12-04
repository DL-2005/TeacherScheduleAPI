using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoiDuongController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BoiDuongController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BoiDuong
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoiDuong>>> GetAll([FromQuery] string? namHoc, [FromQuery] string? maKhoa)
        {
            var query = _context.BoiDuongs
                .Include(b => b.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(b => b.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(b => b.GiangVien.MaKhoa == maKhoa.ToUpper());

            return await query.OrderByDescending(b => b.Id).ToListAsync();
        }

        // GET: api/BoiDuong/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BoiDuong>> Get(int id)
        {
            var item = await _context.BoiDuongs
                .Include(b => b.GiangVien)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy hoạt động bồi dưỡng" });

            return item;
        }

        // GET: api/BoiDuong/GiangVien/{maGV}
        [HttpGet("GiangVien/{maGV}")]
        public async Task<ActionResult> GetByGiangVien(string maGV, [FromQuery] string? namHoc)
        {
            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var query = _context.BoiDuongs
                .Where(b => b.MaGV == maGV)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(b => b.NamHoc == namHoc);

            var danhSach = await query.OrderByDescending(b => b.Id).ToListAsync();

            return Ok(new
            {
                giangVien = new
                {
                    giangVien.MaGV,
                    giangVien.TenGV,
                    giangVien.MaKhoa,
                    TenKhoa = giangVien.Khoa?.TenKhoa
                },
                tongGioBoiDuong = danhSach.Sum(b => b.GioBoiDuong),
                soHoatDong = danhSach.Count,
                danhSach = danhSach.Select(b => new
                {
                    b.Id,
                    b.NoiDung,
                    b.ChiTiet,
                    b.GioBoiDuong,
                    b.NamHoc,
                    b.NgayThucHien,
                    b.GhiChu
                })
            });
        }

        // POST: api/BoiDuong
        [HttpPost]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult<BoiDuong>> Create(BoiDuong model)
        {
            model.MaGV = model.MaGV?.ToUpper();

            if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == model.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            model.GiangVien = null;

            _context.BoiDuongs.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/BoiDuong/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<IActionResult> Update(int id, BoiDuong model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "Id không khớp" });

            model.MaGV = model.MaGV?.ToUpper();
            model.GiangVien = null;

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BoiDuongs.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/BoiDuong/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.BoiDuongs.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.BoiDuongs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}