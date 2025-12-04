using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanCongController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhanCongController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== CRUD CƠ BẢN ====================

        // GET: api/PhanCong
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhanCong>>> GetPhanCongs()
        {
            return await _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .ToListAsync();
        }

        // GET: api/PhanCong/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhanCong>> GetPhanCong(int id)
        {
            var phanCong = await _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .FirstOrDefaultAsync(pc => pc.Id == id);

            if (phanCong == null)
                return NotFound(new { message = "Không tìm thấy phân công" });

            return phanCong;
        }

        // POST: api/PhanCong
        [HttpPost]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<ActionResult<PhanCong>> PostPhanCong(PhanCong phanCong)
        {
            phanCong.MaGV = phanCong.MaGV?.ToUpper();
            phanCong.MaMH = phanCong.MaMH?.ToUpper();
            phanCong.MaLop = phanCong.MaLop?.ToUpper();

            // 1. Kiểm tra các mã tồn tại
            if (!await _context.GiangViens.AnyAsync(g => g.MaGV == phanCong.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            if (!await _context.MonHocs.AnyAsync(m => m.MaMH == phanCong.MaMH))
                return BadRequest(new { message = "Mã môn học không tồn tại" });

            if (!await _context.Lops.AnyAsync(l => l.MaLop == phanCong.MaLop))
                return BadRequest(new { message = "Mã lớp không tồn tại" });

            // 2. Kiểm tra xung đột lịch GIẢNG VIÊN
            var teacherConflict = await CheckTeacherConflict(phanCong, null);
            if (teacherConflict != null)
                return BadRequest(new { message = teacherConflict });

            // 3. Kiểm tra xung đột PHÒNG HỌC
            if (!string.IsNullOrEmpty(phanCong.PhongHoc))
            {
                var roomConflict = await CheckRoomConflict(phanCong, null);
                if (roomConflict != null)
                    return BadRequest(new { message = roomConflict });
            }

            _context.PhanCongs.Add(phanCong);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhanCong", new { id = phanCong.Id }, phanCong);
        }

        // PUT: api/PhanCong/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<IActionResult> PutPhanCong(int id, PhanCong phanCong)
        {
            if (id != phanCong.Id)
                return BadRequest(new { message = "Id không khớp" });

            phanCong.MaGV = phanCong.MaGV?.ToUpper();
            phanCong.MaMH = phanCong.MaMH?.ToUpper();
            phanCong.MaLop = phanCong.MaLop?.ToUpper();

            // 1. Kiểm tra các mã tồn tại
            if (!await _context.GiangViens.AnyAsync(g => g.MaGV == phanCong.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            if (!await _context.MonHocs.AnyAsync(m => m.MaMH == phanCong.MaMH))
                return BadRequest(new { message = "Mã môn học không tồn tại" });

            if (!await _context.Lops.AnyAsync(l => l.MaLop == phanCong.MaLop))
                return BadRequest(new { message = "Mã lớp không tồn tại" });

            // 2. Kiểm tra xung đột lịch GIẢNG VIÊN (loại trừ chính nó)
            var teacherConflict = await CheckTeacherConflict(phanCong, id);
            if (teacherConflict != null)
                return BadRequest(new { message = teacherConflict });

            // 3. Kiểm tra xung đột PHÒNG HỌC (loại trừ chính nó)
            if (!string.IsNullOrEmpty(phanCong.PhongHoc))
            {
                var roomConflict = await CheckRoomConflict(phanCong, id);
                if (roomConflict != null)
                    return BadRequest(new { message = roomConflict });
            }

            _context.Entry(phanCong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PhanCongs.AnyAsync(e => e.Id == id))
                    return NotFound(new { message = "Không tìm thấy phân công" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/PhanCong/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> DeletePhanCong(int id)
        {
            var phanCong = await _context.PhanCongs.FindAsync(id);
            if (phanCong == null)
                return NotFound(new { message = "Không tìm thấy phân công" });

            _context.PhanCongs.Remove(phanCong);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==================== API NÂNG CAO ====================

        // GET: api/PhanCong/GiangVien/{maGV} - Lịch dạy của 1 giảng viên
        [HttpGet("GiangVien/{maGV}")]
        public async Task<ActionResult> GetByGiangVien(string maGV, [FromQuery] string? hocKy = null)
        {
            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var query = _context.PhanCongs
                .Where(pc => pc.MaGV == maGV)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .AsQueryable();

            // Lọc theo học kỳ nếu có
            if (!string.IsNullOrEmpty(hocKy))
                query = query.Where(pc => pc.ThoiGianHoc == hocKy);

            var phanCongs = await query.OrderBy(pc => pc.Thu).ThenBy(pc => pc.TietBatDau).ToListAsync();

            // Tính tổng số tiết
            var tongSoTiet = phanCongs.Sum(pc => pc.SoTiet);

            return Ok(new
            {
                giangVien = new
                {
                    giangVien.MaGV,
                    giangVien.TenGV,
                    giangVien.Email,
                    giangVien.SDT,
                    giangVien.MaKhoa,
                    TenKhoa = giangVien.Khoa?.TenKhoa,
                    giangVien.MaBM,
                    TenBoMon = giangVien.BoMon?.TenBM
                },
                tongSoTiet,
                soHocPhan = phanCongs.Select(pc => pc.MaMH).Distinct().Count(),
                soLop = phanCongs.Select(pc => pc.MaLop).Distinct().Count(),
                danhSachPhanCong = phanCongs.Select(pc => new
                {
                    pc.Id,
                    pc.MaMH,
                    TenMH = pc.MonHoc?.TenMH,
                    SoTinChi = pc.MonHoc?.SoTinChi,
                    pc.MaLop,
                    SiSo = pc.Lop?.SiSo,
                    pc.Thu,
                    pc.TietBatDau,
                    pc.SoTiet,
                    TietKetThuc = pc.TietBatDau + pc.SoTiet - 1,
                    pc.PhongHoc,
                    pc.ThoiGianHoc,
                    pc.GhiChu
                })
            });
        }

        // GET: api/PhanCong/Lop/{maLop} - Thời khóa biểu của 1 lớp
        [HttpGet("Lop/{maLop}")]
        public async Task<ActionResult> GetByLop(string maLop, [FromQuery] string? hocKy = null)
        {
            maLop = maLop.ToUpper();

            var lop = await _context.Lops
                .Include(l => l.Khoa)
                .FirstOrDefaultAsync(l => l.MaLop == maLop);

            if (lop == null)
                return NotFound(new { message = "Không tìm thấy lớp" });

            var query = _context.PhanCongs
                .Where(pc => pc.MaLop == maLop)
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .AsQueryable();

            if (!string.IsNullOrEmpty(hocKy))
                query = query.Where(pc => pc.ThoiGianHoc == hocKy);

            var phanCongs = await query.OrderBy(pc => pc.Thu).ThenBy(pc => pc.TietBatDau).ToListAsync();

            return Ok(new
            {
                lop = new
                {
                    lop.MaLop,
                    lop.SiSo,
                    lop.Nganh,
                    lop.NamHoc,
                    lop.MaKhoa,
                    TenKhoa = lop.Khoa?.TenKhoa
                },
                tongSoTiet = phanCongs.Sum(pc => pc.SoTiet),
                soMonHoc = phanCongs.Select(pc => pc.MaMH).Distinct().Count(),
                thoiKhoaBieu = phanCongs.Select(pc => new
                {
                    pc.Id,
                    pc.MaMH,
                    TenMH = pc.MonHoc?.TenMH,
                    SoTinChi = pc.MonHoc?.SoTinChi,
                    pc.MaGV,
                    TenGV = pc.GiangVien?.TenGV,
                    pc.Thu,
                    pc.TietBatDau,
                    pc.SoTiet,
                    TietKetThuc = pc.TietBatDau + pc.SoTiet - 1,
                    pc.PhongHoc,
                    pc.ThoiGianHoc
                })
            });
        }

        // GET: api/PhanCong/Today - Các tiết đang diễn ra hôm nay
        [HttpGet("Today")]
        public async Task<ActionResult> GetToday()
        {
            // Thứ trong tuần (2 = Thứ 2, ..., 8 = Chủ nhật)
            var today = DateTime.Now.DayOfWeek;
            int thu = today == DayOfWeek.Sunday ? 8 : (int)today + 1;

            var phanCongs = await _context.PhanCongs
                .Where(pc => pc.Thu == thu)
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .OrderBy(pc => pc.TietBatDau)
                .ToListAsync();

            return Ok(new
            {
                ngay = DateTime.Now.ToString("dd/MM/yyyy"),
                thu = thu == 8 ? "Chủ nhật" : $"Thứ {thu}",
                tongSoBuoiHoc = phanCongs.Count,
                danhSach = phanCongs.Select(pc => new
                {
                    pc.Id,
                    pc.MaGV,
                    TenGV = pc.GiangVien?.TenGV,
                    pc.MaMH,
                    TenMH = pc.MonHoc?.TenMH,
                    pc.MaLop,
                    pc.TietBatDau,
                    pc.SoTiet,
                    TietKetThuc = pc.TietBatDau + pc.SoTiet - 1,
                    pc.PhongHoc
                })
            });
        }

        // GET: api/PhanCong/Week?maGV=xxx&week=1 - Lịch theo tuần
        [HttpGet("Week")]
        public async Task<ActionResult> GetWeekSchedule([FromQuery] string? maGV, [FromQuery] string? maLop)
        {
            var query = _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maGV))
                query = query.Where(pc => pc.MaGV == maGV.ToUpper());

            if (!string.IsNullOrEmpty(maLop))
                query = query.Where(pc => pc.MaLop == maLop.ToUpper());

            var phanCongs = await query.ToListAsync();

            // Nhóm theo thứ
            var lichTheoThu = phanCongs
                .GroupBy(pc => pc.Thu)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    thu = g.Key == 8 ? "Chủ nhật" : $"Thứ {g.Key}",
                    thuSo = g.Key,
                    danhSach = g.OrderBy(pc => pc.TietBatDau).Select(pc => new
                    {
                        pc.Id,
                        pc.MaGV,
                        TenGV = pc.GiangVien?.TenGV,
                        pc.MaMH,
                        TenMH = pc.MonHoc?.TenMH,
                        pc.MaLop,
                        pc.TietBatDau,
                        pc.SoTiet,
                        TietKetThuc = pc.TietBatDau + pc.SoTiet - 1,
                        pc.PhongHoc
                    })
                });

            return Ok(new
            {
                maGV,
                maLop,
                tongSoBuoiHoc = phanCongs.Count,
                tongSoTiet = phanCongs.Sum(pc => pc.SoTiet),
                lichTheoThu
            });
        }

        // GET: api/PhanCong/Filter - Lọc nâng cao
        [HttpGet("Filter")]
        public async Task<ActionResult> Filter(
            [FromQuery] string? maGV,
            [FromQuery] string? maLop,
            [FromQuery] string? maMH,
            [FromQuery] int? thu,
            [FromQuery] string? hocKy,
            [FromQuery] string? phongHoc)
        {
            var query = _context.PhanCongs
                .Include(pc => pc.GiangVien)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maGV))
                query = query.Where(pc => pc.MaGV == maGV.ToUpper());

            if (!string.IsNullOrEmpty(maLop))
                query = query.Where(pc => pc.MaLop == maLop.ToUpper());

            if (!string.IsNullOrEmpty(maMH))
                query = query.Where(pc => pc.MaMH == maMH.ToUpper());

            if (thu.HasValue)
                query = query.Where(pc => pc.Thu == thu.Value);

            if (!string.IsNullOrEmpty(hocKy))
                query = query.Where(pc => pc.ThoiGianHoc == hocKy);

            if (!string.IsNullOrEmpty(phongHoc))
                query = query.Where(pc => pc.PhongHoc == phongHoc);

            var result = await query
                .OrderBy(pc => pc.Thu)
                .ThenBy(pc => pc.TietBatDau)
                .ToListAsync();

            return Ok(new
            {
                tongKetQua = result.Count,
                tongSoTiet = result.Sum(pc => pc.SoTiet),
                danhSach = result.Select(pc => new
                {
                    pc.Id,
                    pc.MaGV,
                    TenGV = pc.GiangVien?.TenGV,
                    pc.MaMH,
                    TenMH = pc.MonHoc?.TenMH,
                    SoTinChi = pc.MonHoc?.SoTinChi,
                    pc.MaLop,
                    SiSo = pc.Lop?.SiSo,
                    pc.Thu,
                    pc.TietBatDau,
                    pc.SoTiet,
                    TietKetThuc = pc.TietBatDau + pc.SoTiet - 1,
                    pc.PhongHoc,
                    pc.ThoiGianHoc,
                    pc.GhiChu
                })
            });
        }

        // GET: api/PhanCong/ThongKeGioGiang - Thống kê giờ giảng theo giảng viên
        [HttpGet("ThongKeGioGiang")]
        public async Task<ActionResult> ThongKeGioGiang([FromQuery] string? maKhoa, [FromQuery] string? hocKy)
        {
            var query = _context.PhanCongs
                .Include(pc => pc.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(hocKy))
                query = query.Where(pc => pc.ThoiGianHoc == hocKy);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(pc => pc.GiangVien.MaKhoa == maKhoa.ToUpper());

            var result = await query.ToListAsync();

            var thongKe = result
                .GroupBy(pc => new { pc.MaGV, pc.GiangVien?.TenGV, pc.GiangVien?.MaKhoa })
                .Select(g => new
                {
                    maGV = g.Key.MaGV,
                    tenGV = g.Key.TenGV,
                    maKhoa = g.Key.MaKhoa,
                    tongSoTiet = g.Sum(pc => pc.SoTiet),
                    soHocPhan = g.Select(pc => pc.MaMH).Distinct().Count(),
                    soLop = g.Select(pc => pc.MaLop).Distinct().Count()
                })
                .OrderByDescending(x => x.tongSoTiet)
                .ToList();

            return Ok(new
            {
                hocKy,
                maKhoa,
                tongGiangVien = thongKe.Count,
                tongSoTiet = thongKe.Sum(x => x.tongSoTiet),
                danhSach = thongKe
            });
        }

        // ==================== HELPER METHODS ====================

        private async Task<string?> CheckTeacherConflict(PhanCong phanCong, int? excludeId)
        {
            var query = _context.PhanCongs.Where(pc =>
                pc.MaGV == phanCong.MaGV &&
                pc.Thu == phanCong.Thu &&
                pc.ThoiGianHoc == phanCong.ThoiGianHoc &&
                (
                    (phanCong.TietBatDau >= pc.TietBatDau && phanCong.TietBatDau < (pc.TietBatDau + pc.SoTiet)) ||
                    ((phanCong.TietBatDau + phanCong.SoTiet) > pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) <= (pc.TietBatDau + pc.SoTiet)) ||
                    (phanCong.TietBatDau <= pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) >= (pc.TietBatDau + pc.SoTiet))
                )
            );

            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            var conflict = await query.Include(pc => pc.MonHoc).Include(pc => pc.Lop).FirstOrDefaultAsync();

            if (conflict != null)
                return $"Giảng viên đã có lịch dạy trùng: {conflict.MonHoc?.TenMH} - Lớp {conflict.MaLop} (Thứ {conflict.Thu}, Tiết {conflict.TietBatDau}-{conflict.TietBatDau + conflict.SoTiet - 1})";

            return null;
        }

        private async Task<string?> CheckRoomConflict(PhanCong phanCong, int? excludeId)
        {
            var query = _context.PhanCongs.Where(pc =>
                pc.PhongHoc == phanCong.PhongHoc &&
                pc.Thu == phanCong.Thu &&
                pc.ThoiGianHoc == phanCong.ThoiGianHoc &&
                (
                    (phanCong.TietBatDau >= pc.TietBatDau && phanCong.TietBatDau < (pc.TietBatDau + pc.SoTiet)) ||
                    ((phanCong.TietBatDau + phanCong.SoTiet) > pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) <= (pc.TietBatDau + pc.SoTiet)) ||
                    (phanCong.TietBatDau <= pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) >= (pc.TietBatDau + pc.SoTiet))
                )
            );

            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            var conflict = await query.Include(pc => pc.GiangVien).Include(pc => pc.MonHoc).FirstOrDefaultAsync();

            if (conflict != null)
                return $"Phòng {phanCong.PhongHoc} đã có lịch sử dụng: {conflict.GiangVien?.TenGV} dạy {conflict.MonHoc?.TenMH} (Thứ {conflict.Thu}, Tiết {conflict.TietBatDau}-{conflict.TietBatDau + conflict.SoTiet - 1})";

            return null;
        }
    }
}