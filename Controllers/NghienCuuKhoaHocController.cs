using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NghienCuuKhoaHocController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NghienCuuKhoaHocController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/NghienCuuKhoaHoc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NghienCuuKhoaHoc>>> GetAll(
            [FromQuery] string? namHoc,
            [FromQuery] string? theLoai,
            [FromQuery] string? maKhoa)
        {
            var query = _context.NghienCuuKhoaHocs
                .Include(n => n.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(n => n.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(theLoai))
                query = query.Where(n => n.TheLoai == theLoai);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(n => n.GiangVien.MaKhoa == maKhoa.ToUpper());

            return await query.OrderByDescending(n => n.Id).ToListAsync();
        }

        // GET: api/NghienCuuKhoaHoc/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NghienCuuKhoaHoc>> Get(int id)
        {
            var item = await _context.NghienCuuKhoaHocs
                .Include(n => n.GiangVien)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy đề tài NCKH" });

            return item;
        }

        // GET: api/NghienCuuKhoaHoc/GiangVien/{maGV} - Lấy NCKH của 1 giảng viên
        [HttpGet("GiangVien/{maGV}")]
        public async Task<ActionResult> GetByGiangVien(string maGV, [FromQuery] string? namHoc)
        {
            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var query = _context.NghienCuuKhoaHocs
                .Where(n => n.MaGV == maGV)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(n => n.NamHoc == namHoc);

            var danhSach = await query.OrderByDescending(n => n.Id).ToListAsync();

            // Thống kê theo thể loại
            var thongKeTheoLoai = danhSach
                .GroupBy(n => n.TheLoai)
                .Select(g => new
                {
                    theLoai = g.Key,
                    soLuong = g.Count(),
                    tongGio = g.Sum(x => x.GioNCKH)
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
                tongSoGioNCKH = danhSach.Sum(n => n.GioNCKH),
                tongSoDeTai = danhSach.Count,
                thongKeTheoLoai,
                danhSach = danhSach.Select(n => new
                {
                    n.Id,
                    n.TenDeTai,
                    n.TheLoai,
                    n.VaiTro,
                    n.GioNCKH,
                    n.NamHoc,
                    n.TrangThai,
                    n.NgayBatDau,
                    n.NgayKetThuc
                })
            });
        }

        // GET: api/NghienCuuKhoaHoc/TongHop - Tổng hợp NCKH toàn trường/khoa
        [HttpGet("TongHop")]
        public async Task<ActionResult> TongHop([FromQuery] string? namHoc, [FromQuery] string? maKhoa, [FromQuery] string? theLoai)
        {
            var query = _context.NghienCuuKhoaHocs
                .Include(n => n.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(n => n.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(n => n.GiangVien.MaKhoa == maKhoa.ToUpper());

            if (!string.IsNullOrEmpty(theLoai))
                query = query.Where(n => n.TheLoai == theLoai);

            var danhSach = await query.ToListAsync();

            return Ok(new
            {
                namHoc,
                maKhoa,
                theLoai,
                tongSoDeTai = danhSach.Count,
                tongSoGioNCKH = danhSach.Sum(n => n.GioNCKH),
                thongKeTheoLoai = danhSach
                    .GroupBy(n => n.TheLoai)
                    .Select(g => new
                    {
                        theLoai = g.Key,
                        soLuong = g.Count(),
                        tongGio = g.Sum(x => x.GioNCKH)
                    }),
                danhSach = danhSach.Select(n => new
                {
                    n.Id,
                    n.MaGV,
                    TenGV = n.GiangVien?.TenGV,
                    MaKhoa = n.GiangVien?.MaKhoa,
                    n.TenDeTai,
                    n.TheLoai,
                    n.VaiTro,
                    n.GioNCKH,
                    n.TrangThai
                })
            });
        }

        // POST: api/NghienCuuKhoaHoc
        [HttpPost]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult<NghienCuuKhoaHoc>> Create(NghienCuuKhoaHoc model)
        {
            model.MaGV = model.MaGV?.ToUpper();

            if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == model.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            model.GiangVien = null; // Ngắt navigation

            _context.NghienCuuKhoaHocs.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/NghienCuuKhoaHoc/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<IActionResult> Update(int id, NghienCuuKhoaHoc model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "Id không khớp" });

            model.MaGV = model.MaGV?.ToUpper();

            if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == model.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            model.GiangVien = null;

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.NghienCuuKhoaHocs.AnyAsync(e => e.Id == id))
                    return NotFound(new { message = "Không tìm thấy đề tài NCKH" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/NghienCuuKhoaHoc/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NghienCuuKhoaHocs.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy đề tài NCKH" });

            _context.NghienCuuKhoaHocs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}