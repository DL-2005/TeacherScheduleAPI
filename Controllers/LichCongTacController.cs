using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LichCongTacController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LichCongTacController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LichCongTac - Lấy danh sách lịch công tác
        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay,
            [FromQuery] string? maKhoa,
            [FromQuery] string? loaiLich)
        {
            var query = _context.LichCongTacs
                .Include(l => l.GiangVien)
                .Include(l => l.Khoa)
                .AsQueryable();

            // Lọc theo khoảng ngày
            if (tuNgay.HasValue)
                query = query.Where(l => l.NgayThang >= tuNgay.Value);

            if (denNgay.HasValue)
                query = query.Where(l => l.NgayThang <= denNgay.Value);

            // Lọc theo khoa
            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(l => l.MaKhoa == maKhoa.ToUpper());

            // Lọc theo loại lịch
            if (!string.IsNullOrEmpty(loaiLich))
                query = query.Where(l => l.LoaiLich == loaiLich);

            var result = await query
                .OrderBy(l => l.NgayThang)
                .ThenBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            return Ok(new
            {
                tongSo = result.Count,
                danhSach = result.Select(l => new
                {
                    l.Id,
                    l.NgayThang,
                    thu = GetThu(l.NgayThang),
                    thoiGianBatDau = l.ThoiGianBatDau?.ToString(@"hh\:mm"),
                    thoiGianKetThuc = l.ThoiGianKetThuc?.ToString(@"hh\:mm"),
                    l.NoiDung,
                    l.DiaDiem,
                    l.ThanhPhan,
                    l.ChuTri,
                    l.MaGV,
                    tenGV = l.GiangVien?.TenGV,
                    l.MaKhoa,
                    tenKhoa = l.Khoa?.TenKhoa,
                    l.LoaiLich,
                    l.TrangThai,
                    l.GhiChu
                })
            });
        }

        // GET: api/LichCongTac/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var item = await _context.LichCongTacs
                .Include(l => l.GiangVien)
                .Include(l => l.Khoa)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy lịch công tác" });

            return Ok(item);
        }

        // GET: api/LichCongTac/Tuan - Lịch công tác theo tuần
        [HttpGet("Tuan")]
        public async Task<ActionResult> GetByWeek([FromQuery] DateTime? ngay, [FromQuery] string? maKhoa)
        {
            // Mặc định lấy tuần hiện tại
            var ngayHienTai = ngay ?? DateTime.Today;

            // Tính ngày đầu tuần (Thứ 2)
            int diff = (7 + (ngayHienTai.DayOfWeek - DayOfWeek.Monday)) % 7;
            var ngayDauTuan = ngayHienTai.AddDays(-diff).Date;
            var ngayCuoiTuan = ngayDauTuan.AddDays(6);

            var query = _context.LichCongTacs
                .Where(l => l.NgayThang >= ngayDauTuan && l.NgayThang <= ngayCuoiTuan)
                .Include(l => l.GiangVien)
                .Include(l => l.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(l => l.MaKhoa == maKhoa.ToUpper() || l.MaKhoa == null);

            var result = await query
                .OrderBy(l => l.NgayThang)
                .ThenBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            // Nhóm theo ngày
            var lichTheoNgay = result
                .GroupBy(l => l.NgayThang.Date)
                .Select(g => new
                {
                    ngay = g.Key,
                    thu = GetThu(g.Key),
                    danhSach = g.Select(l => new
                    {
                        l.Id,
                        thoiGianBatDau = l.ThoiGianBatDau?.ToString(@"hh\:mm"),
                        thoiGianKetThuc = l.ThoiGianKetThuc?.ToString(@"hh\:mm"),
                        l.NoiDung,
                        l.DiaDiem,
                        l.ThanhPhan,
                        l.ChuTri,
                        l.LoaiLich,
                        l.TrangThai
                    })
                })
                .OrderBy(x => x.ngay);

            return Ok(new
            {
                tuanTu = ngayDauTuan,
                tuanDen = ngayCuoiTuan,
                tongSoLich = result.Count,
                lichTheoNgay
            });
        }

        // GET: api/LichCongTac/HomNay - Lịch hôm nay
        [HttpGet("HomNay")]
        public async Task<ActionResult> GetToday([FromQuery] string? maKhoa)
        {
            var homNay = DateTime.Today;

            var query = _context.LichCongTacs
                .Where(l => l.NgayThang.Date == homNay)
                .Include(l => l.GiangVien)
                .Include(l => l.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(l => l.MaKhoa == maKhoa.ToUpper() || l.MaKhoa == null);

            var result = await query
                .OrderBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            return Ok(new
            {
                ngay = homNay,
                thu = GetThu(homNay),
                tongSoLich = result.Count,
                danhSach = result.Select(l => new
                {
                    l.Id,
                    thoiGianBatDau = l.ThoiGianBatDau?.ToString(@"hh\:mm"),
                    thoiGianKetThuc = l.ThoiGianKetThuc?.ToString(@"hh\:mm"),
                    l.NoiDung,
                    l.DiaDiem,
                    l.ThanhPhan,
                    l.ChuTri,
                    l.LoaiLich,
                    l.TrangThai
                })
            });
        }

        // POST: api/LichCongTac
        [HttpPost]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult> Create(LichCongTac model)
        {
            model.MaGV = model.MaGV?.ToUpper();
            model.MaKhoa = model.MaKhoa?.ToUpper();
            model.NgayTao = DateTime.Now;

            // Kiểm tra giảng viên tồn tại
            if (!string.IsNullOrEmpty(model.MaGV))
            {
                if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == model.MaGV))
                    return BadRequest(new { message = "Mã giảng viên không tồn tại" });
            }

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(model.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == model.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            model.GiangVien = null;
            model.Khoa = null;

            _context.LichCongTacs.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/LichCongTac/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Update(int id, LichCongTac model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "Id không khớp" });

            model.MaGV = model.MaGV?.ToUpper();
            model.MaKhoa = model.MaKhoa?.ToUpper();
            model.GiangVien = null;
            model.Khoa = null;

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.LichCongTacs.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/LichCongTac/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.LichCongTacs.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.LichCongTacs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper: Lấy thứ trong tuần
        private string GetThu(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }
    }
}
