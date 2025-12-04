using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhiemVuKhacController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NhiemVuKhacController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/NhiemVuKhac
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhiemVuKhac>>> GetAll([FromQuery] string? namHoc, [FromQuery] string? maKhoa)
        {
            var query = _context.NhiemVuKhacs
                .Include(n => n.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(n => n.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(n => n.GiangVien.MaKhoa == maKhoa.ToUpper());

            return await query.OrderByDescending(n => n.Id).ToListAsync();
        }

        // GET: api/NhiemVuKhac/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NhiemVuKhac>> Get(int id)
        {
            var item = await _context.NhiemVuKhacs
                .Include(n => n.GiangVien)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ" });

            return item;
        }

        // GET: api/NhiemVuKhac/GiangVien/{maGV}
        [HttpGet("GiangVien/{maGV}")]
        public async Task<ActionResult> GetByGiangVien(string maGV, [FromQuery] string? namHoc)
        {
            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var query = _context.NhiemVuKhacs
                .Where(n => n.MaGV == maGV)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(n => n.NamHoc == namHoc);

            var danhSach = await query.OrderByDescending(n => n.Id).ToListAsync();

            // Thống kê theo loại công việc
            var thongKeTheoCongViec = danhSach
                .GroupBy(n => n.CongViec)
                .Select(g => new
                {
                    congViec = g.Key,
                    soLan = g.Count(),
                    tongGio = g.Sum(x => x.SoGio)
                })
                .ToList();

            return Ok(new
            {
                giangVien = new
                {
                    giangVien.MaGV,
                    giangVien.TenGV,
                    giangVien.MaKhoa,
                    TenKhoa = giangVien.Khoa?.TenKhoa
                },
                tongSoGio = danhSach.Sum(n => n.SoGio),
                soNhiemVu = danhSach.Count,
                thongKeTheoCongViec,
                danhSach = danhSach.Select(n => new
                {
                    n.Id,
                    n.CongViec,
                    n.ChiTiet,
                    n.SoGio,
                    n.NamHoc,
                    n.NgayThucHien,
                    n.GhiChu
                })
            });
        }

        // POST: api/NhiemVuKhac
        [HttpPost]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult<NhiemVuKhac>> Create(NhiemVuKhac model)
        {
            model.MaGV = model.MaGV?.ToUpper();

            if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == model.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            model.GiangVien = null;

            _context.NhiemVuKhacs.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/NhiemVuKhac/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<IActionResult> Update(int id, NhiemVuKhac model)
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
                if (!await _context.NhiemVuKhacs.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/NhiemVuKhac/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NhiemVuKhacs.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.NhiemVuKhacs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}