using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;
using TeacherScheduleAPI.Services;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IExportService _exportService;

        public ExportController(AppDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // ==================== XUẤT EXCEL ====================

        /// <summary>
        /// Xuất danh sách giảng viên ra Excel
        /// </summary>
        [HttpGet("Excel/GiangVien")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> ExportGiangVienExcel([FromQuery] string? maKhoa)
        {
            var query = _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(gv => gv.MaKhoa == maKhoa.ToUpper());

            var data = await query.OrderBy(gv => gv.MaGV).ToListAsync();

            var fileBytes = _exportService.ExportGiangVienToExcel(data);
            var fileName = $"DanhSachGiangVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Xuất bảng phân công giảng dạy ra Excel
        /// </summary>
        [HttpGet("Excel/PhanCong")]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<IActionResult> ExportPhanCongExcel(
            [FromQuery] string? maKhoa,
            [FromQuery] string? maGV,
            [FromQuery] string? thoiGianHoc)
        {
            var query = _context.PhanCongs
                .Include(pc => pc.GiangVien).ThenInclude(gv => gv.Khoa)
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(pc => pc.GiangVien.MaKhoa == maKhoa.ToUpper());

            if (!string.IsNullOrEmpty(maGV))
                query = query.Where(pc => pc.MaGV == maGV.ToUpper());

            if (!string.IsNullOrEmpty(thoiGianHoc))
                query = query.Where(pc => pc.ThoiGianHoc == thoiGianHoc);

            var data = await query.OrderBy(pc => pc.MaGV).ThenBy(pc => pc.Thu).ToListAsync();

            var fileBytes = _exportService.ExportPhanCongToExcel(data);
            var fileName = $"PhanCongGiangDay_{thoiGianHoc ?? "All"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Xuất thống kê giờ giảng ra Excel
        /// </summary>
        [HttpGet("Excel/ThongKeGioGiang")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> ExportThongKeGioGiangExcel(
            [FromQuery] string? maKhoa,
            [FromQuery] string? thoiGianHoc)
        {
            var query = _context.PhanCongs
                .Include(pc => pc.GiangVien).ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(pc => pc.GiangVien.MaKhoa == maKhoa.ToUpper());

            if (!string.IsNullOrEmpty(thoiGianHoc))
                query = query.Where(pc => pc.ThoiGianHoc == thoiGianHoc);

            var phanCongs = await query.ToListAsync();

            // Tính tổng giờ giảng theo từng giảng viên
            var thongKe = phanCongs
                .GroupBy(pc => new { pc.MaGV, pc.GiangVien?.TenGV, pc.GiangVien?.MaKhoa, TenKhoa = pc.GiangVien?.Khoa?.TenKhoa })
                .Select(g => new ThongKeGioGiangDto
                {
                    MaGV = g.Key.MaGV,
                    TenGV = g.Key.TenGV ?? "",
                    MaKhoa = g.Key.MaKhoa ?? "",
                    TenKhoa = g.Key.TenKhoa ?? "",
                    TongSoTiet = g.Sum(pc => pc.SoTiet),
                    SoMonHoc = g.Select(pc => pc.MaMH).Distinct().Count(),
                    SoLopHoc = g.Select(pc => pc.MaLop).Distinct().Count()
                })
                .OrderBy(x => x.MaKhoa)
                .ThenBy(x => x.MaGV)
                .ToList();

            var fileBytes = _exportService.ExportThongKeGioGiangToExcel(thongKe, thoiGianHoc);
            var fileName = $"ThongKeGioGiang_{thoiGianHoc ?? "All"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Xuất bảng kê khai nhiệm vụ ra Excel
        /// </summary>
        [HttpGet("Excel/KeKhaiNhiemVu/{maGV}")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<IActionResult> ExportKeKhaiNhiemVuExcel(string maGV, [FromQuery] string? thoiGianHoc)
        {
            maGV = maGV.ToUpper();
            thoiGianHoc ??= $"HK1-{DateTime.Now.Year}";

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // Lấy năm học từ thoiGianHoc (VD: "HK1-2024" -> "2024-2025")
            var namHoc = ExtractNamHoc(thoiGianHoc);

            // Lấy dữ liệu
            var phanCongs = await _context.PhanCongs
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .Where(pc => pc.MaGV == maGV && pc.ThoiGianHoc == thoiGianHoc)
                .ToListAsync();

            var nckhs = await _context.NghienCuuKhoaHocs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var boiDuongs = await _context.BoiDuongs
                .Where(b => b.MaGV == maGV && b.NamHoc == namHoc)
                .ToListAsync();

            var nhiemVuKhacs = await _context.NhiemVuKhacs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var dinhMuc = await _context.DinhMucs
                .FirstOrDefaultAsync(d => d.NamHoc == namHoc);

            var keKhaiData = new KeKhaiNhiemVuDto
            {
                GiangVien = giangVien,
                ThoiGianHoc = thoiGianHoc,
                NamHoc = namHoc,
                PhanCongs = phanCongs,
                NCKHs = nckhs,
                BoiDuongs = boiDuongs,
                NhiemVuKhacs = nhiemVuKhacs,
                DinhMuc = dinhMuc
            };

            var fileBytes = _exportService.ExportKeKhaiNhiemVuToExcel(keKhaiData);
            var fileName = $"KeKhaiNhiemVu_{maGV}_{thoiGianHoc.Replace("-", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ==================== XUẤT PDF ====================

        /// <summary>
        /// Xuất bảng kê khai nhiệm vụ ra PDF
        /// </summary>
        [HttpGet("PDF/KeKhaiNhiemVu/{maGV}")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<IActionResult> ExportKeKhaiNhiemVuPdf(string maGV, [FromQuery] string? thoiGianHoc)
        {
            maGV = maGV.ToUpper();
            thoiGianHoc ??= $"HK1-{DateTime.Now.Year}";

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var namHoc = ExtractNamHoc(thoiGianHoc);

            // Lấy dữ liệu
            var phanCongs = await _context.PhanCongs
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .Where(pc => pc.MaGV == maGV && pc.ThoiGianHoc == thoiGianHoc)
                .ToListAsync();

            var nckhs = await _context.NghienCuuKhoaHocs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var boiDuongs = await _context.BoiDuongs
                .Where(b => b.MaGV == maGV && b.NamHoc == namHoc)
                .ToListAsync();

            var nhiemVuKhacs = await _context.NhiemVuKhacs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var dinhMuc = await _context.DinhMucs
                .FirstOrDefaultAsync(d => d.NamHoc == namHoc);

            var keKhaiData = new KeKhaiNhiemVuDto
            {
                GiangVien = giangVien,
                ThoiGianHoc = thoiGianHoc,
                NamHoc = namHoc,
                PhanCongs = phanCongs,
                NCKHs = nckhs,
                BoiDuongs = boiDuongs,
                NhiemVuKhacs = nhiemVuKhacs,
                DinhMuc = dinhMuc
            };

            var fileBytes = _exportService.ExportKeKhaiNhiemVuToPdf(keKhaiData);
            var fileName = $"KeKhaiNhiemVu_{maGV}_{thoiGianHoc.Replace("-", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Xuất thống kê toàn trường ra PDF
        /// </summary>
        [HttpGet("PDF/ThongKeToanTruong")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> ExportThongKeToanTruongPdf([FromQuery] string? thoiGianHoc)
        {
            thoiGianHoc ??= $"HK1-{DateTime.Now.Year}";
            var namHoc = ExtractNamHoc(thoiGianHoc);

            var thongKeData = new ThongKeToanTruongDto
            {
                ThoiGianHoc = thoiGianHoc,
                NamHoc = namHoc,
                TongGiangVien = await _context.GiangViens.CountAsync(),
                TongKhoa = await _context.Khoas.CountAsync(),
                TongMonHoc = await _context.MonHocs.CountAsync(),
                TongLop = await _context.Lops.CountAsync(),
                TongPhanCong = await _context.PhanCongs.Where(pc => pc.ThoiGianHoc == thoiGianHoc).CountAsync(),
                TongNCKH = await _context.NghienCuuKhoaHocs.Where(n => n.NamHoc == namHoc).CountAsync(),
                TongBoiDuong = await _context.BoiDuongs.Where(b => b.NamHoc == namHoc).CountAsync(),
                ThongKeTheoKhoa = await _context.Khoas
                    .Select(k => new ThongKeKhoaDto
                    {
                        MaKhoa = k.MaKhoa,
                        TenKhoa = k.TenKhoa,
                        SoGiangVien = _context.GiangViens.Count(gv => gv.MaKhoa == k.MaKhoa),
                        SoBoMon = _context.BoMons.Count(bm => bm.MaKhoa == k.MaKhoa),
                        TongSoTiet = _context.PhanCongs
                            .Where(pc => pc.GiangVien.MaKhoa == k.MaKhoa && pc.ThoiGianHoc == thoiGianHoc)
                            .Sum(pc => pc.SoTiet)
                    })
                    .ToListAsync()
            };

            var fileBytes = _exportService.ExportThongKeToanTruongToPdf(thongKeData);
            var fileName = $"ThongKeToanTruong_{thoiGianHoc.Replace("-", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Xuất lịch công tác tuần ra PDF
        /// </summary>
        [HttpGet("PDF/LichCongTac")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> ExportLichCongTacPdf([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] string? maKhoa)
        {
            // Mặc định lấy tuần hiện tại
            var ngayHienTai = tuNgay ?? DateTime.Today;
            int diff = (7 + (ngayHienTai.DayOfWeek - DayOfWeek.Monday)) % 7;
            var ngayDauTuan = tuNgay ?? ngayHienTai.AddDays(-diff).Date;
            var ngayCuoiTuan = denNgay ?? ngayDauTuan.AddDays(6);

            var query = _context.LichCongTacs
                .Where(l => l.NgayThang >= ngayDauTuan && l.NgayThang <= ngayCuoiTuan)
                .Include(l => l.GiangVien)
                .Include(l => l.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(l => l.MaKhoa == maKhoa.ToUpper() || l.MaKhoa == null);

            var lichCongTacs = await query
                .OrderBy(l => l.NgayThang)
                .ThenBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            var fileBytes = _exportService.ExportLichCongTacToPdf(lichCongTacs, ngayDauTuan, ngayCuoiTuan);
            var fileName = $"LichCongTac_{ngayDauTuan:yyyyMMdd}_{ngayCuoiTuan:yyyyMMdd}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        // Helper: Trích xuất năm học từ ThoiGianHoc
        private string ExtractNamHoc(string thoiGianHoc)
        {
            // "HK1-2024" -> "2024-2025"
            // "HK2-2024" -> "2023-2024"
            try
            {
                var parts = thoiGianHoc.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int year))
                {
                    if (parts[0].ToUpper() == "HK1")
                        return $"{year}-{year + 1}";
                    else
                        return $"{year - 1}-{year}";
                }
            }
            catch { }
            return $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}";
        }
    }

    // ==================== DTOs ====================

    public class ThongKeGioGiangDto
    {
        public string MaGV { get; set; }
        public string TenGV { get; set; }
        public string MaKhoa { get; set; }
        public string TenKhoa { get; set; }
        public int TongSoTiet { get; set; }
        public int SoMonHoc { get; set; }
        public int SoLopHoc { get; set; }
    }

    public class KeKhaiNhiemVuDto
    {
        public GiangVien GiangVien { get; set; }
        public string ThoiGianHoc { get; set; }
        public string NamHoc { get; set; }
        public List<PhanCong> PhanCongs { get; set; }
        public List<NghienCuuKhoaHoc> NCKHs { get; set; }
        public List<BoiDuong> BoiDuongs { get; set; }
        public List<NhiemVuKhac> NhiemVuKhacs { get; set; }
        public DinhMuc? DinhMuc { get; set; }
    }

    public class ThongKeToanTruongDto
    {
        public string ThoiGianHoc { get; set; }
        public string NamHoc { get; set; }
        public int TongGiangVien { get; set; }
        public int TongKhoa { get; set; }
        public int TongMonHoc { get; set; }
        public int TongLop { get; set; }
        public int TongPhanCong { get; set; }
        public int TongNCKH { get; set; }
        public int TongBoiDuong { get; set; }
        public List<ThongKeKhoaDto> ThongKeTheoKhoa { get; set; }
    }

    public class ThongKeKhoaDto
    {
        public string MaKhoa { get; set; }
        public string TenKhoa { get; set; }
        public int SoGiangVien { get; set; }
        public int SoBoMon { get; set; }
        public int TongSoTiet { get; set; }
    }
}
