using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;
using TeacherScheduleAPI.Models;
using System.Security.Claims;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinhChungController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MinhChungController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/MinhChung - Danh sách minh chứng
        [HttpGet]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<ActionResult> GetAll(
            [FromQuery] string? loaiMinhChung,
            [FromQuery] string? namHoc,
            [FromQuery] string? maKhoa,
            [FromQuery] string? trangThai)
        {
            var query = _context.MinhChungs
                .Include(m => m.GiangVien)
                    .ThenInclude(gv => gv.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(loaiMinhChung))
                query = query.Where(m => m.LoaiMinhChung == loaiMinhChung);

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(m => m.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(m => m.GiangVien.MaKhoa == maKhoa.ToUpper());

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(m => m.TrangThai == trangThai);

            var result = await query
                .OrderByDescending(m => m.NgayNop)
                .ToListAsync();

            return Ok(new
            {
                tongSo = result.Count,
                thongKe = new
                {
                    choDuyet = result.Count(m => m.TrangThai == "Chờ duyệt"),
                    daDuyet = result.Count(m => m.TrangThai == "Đã duyệt"),
                    tuChoi = result.Count(m => m.TrangThai == "Từ chối"),
                    canBoSung = result.Count(m => m.TrangThai == "Cần bổ sung")
                },
                danhSach = result.Select(m => new
                {
                    m.Id,
                    m.MaGV,
                    tenGV = m.GiangVien?.TenGV,
                    maKhoa = m.GiangVien?.MaKhoa,
                    tenKhoa = m.GiangVien?.Khoa?.TenKhoa,
                    m.LoaiMinhChung,
                    m.TieuDe,
                    m.MoTa,
                    m.NamHoc,
                    m.NgayNop,
                    m.TrangThai,
                    m.FileName,
                    fileSize = m.FileSize.HasValue ? FormatFileSize(m.FileSize.Value) : null,
                    m.NguoiDuyet,
                    m.NgayDuyet,
                    m.GhiChuDuyet
                })
            });
        }

        // GET: api/MinhChung/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var item = await _context.MinhChungs
                .Include(m => m.GiangVien)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy minh chứng" });

            return Ok(item);
        }

        // GET: api/MinhChung/GiangVien/{maGV} - Minh chứng của 1 giảng viên
        [HttpGet("GiangVien/{maGV}")]
        public async Task<ActionResult> GetByGiangVien(string maGV, [FromQuery] string? namHoc)
        {
            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var query = _context.MinhChungs
                .Where(m => m.MaGV == maGV)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(m => m.NamHoc == namHoc);

            var result = await query
                .OrderByDescending(m => m.NgayNop)
                .ToListAsync();

            // Thống kê theo loại minh chứng
            var thongKeTheoLoai = result
                .GroupBy(m => m.LoaiMinhChung)
                .Select(g => new
                {
                    loaiMinhChung = g.Key,
                    soLuong = g.Count(),
                    daDuyet = g.Count(m => m.TrangThai == "Đã duyệt"),
                    choDuyet = g.Count(m => m.TrangThai == "Chờ duyệt")
                });

            return Ok(new
            {
                giangVien = new
                {
                    giangVien.MaGV,
                    giangVien.TenGV,
                    giangVien.MaKhoa,
                    tenKhoa = giangVien.Khoa?.TenKhoa
                },
                tongSo = result.Count,
                thongKe = new
                {
                    choDuyet = result.Count(m => m.TrangThai == "Chờ duyệt"),
                    daDuyet = result.Count(m => m.TrangThai == "Đã duyệt"),
                    tuChoi = result.Count(m => m.TrangThai == "Từ chối")
                },
                thongKeTheoLoai,
                danhSach = result.Select(m => new
                {
                    m.Id,
                    m.LoaiMinhChung,
                    m.TieuDe,
                    m.MoTa,
                    m.NamHoc,
                    m.NgayNop,
                    m.TrangThai,
                    m.FileName,
                    m.GhiChuDuyet
                })
            });
        }

        // POST: api/MinhChung/NopPhieuBaoGiang - Nộp phiếu báo giảng
        [HttpPost("NopPhieuBaoGiang")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult> NopPhieuBaoGiang([FromForm] string maGV, [FromForm] string tieuDe, [FromForm] string? moTa, [FromForm] string namHoc, IFormFile file)
        {
            return await NopMinhChung(maGV, "Phiếu báo giảng", tieuDe, moTa, namHoc, null, file);
        }

        // POST: api/MinhChung/NopMinhChungNCKH - Nộp minh chứng NCKH
        [HttpPost("NopMinhChungNCKH")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult> NopMinhChungNCKH([FromForm] string maGV, [FromForm] string tieuDe, [FromForm] string? moTa, [FromForm] string namHoc, [FromForm] int? idNCKH, IFormFile file)
        {
            return await NopMinhChung(maGV, "Minh chứng NCKH", tieuDe, moTa, namHoc, idNCKH, file);
        }

        // POST: api/MinhChung/NopMinhChungBoiDuong - Nộp minh chứng bồi dưỡng
        [HttpPost("NopMinhChungBoiDuong")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult> NopMinhChungBoiDuong([FromForm] string maGV, [FromForm] string tieuDe, [FromForm] string? moTa, [FromForm] string namHoc, [FromForm] int? idBoiDuong, IFormFile file)
        {
            var minhChung = new MinhChung
            {
                MaGV = maGV.ToUpper(),
                LoaiMinhChung = "Minh chứng bồi dưỡng",
                TieuDe = tieuDe,
                MoTa = moTa,
                NamHoc = namHoc,
                IdBoiDuong = idBoiDuong,
                TrangThai = "Chờ duyệt",
                NgayNop = DateTime.Now
            };

            return await SaveMinhChung(minhChung, file);
        }

        // POST: api/MinhChung/Nop - Nộp minh chứng tổng quát
        [HttpPost("Nop")]
        [Authorize(Roles = "CQC,TK,TBM,GV")]
        public async Task<ActionResult> NopMinhChungTongQuat([FromForm] string maGV, [FromForm] string loaiMinhChung, [FromForm] string tieuDe, [FromForm] string? moTa, [FromForm] string namHoc, IFormFile file)
        {
            return await NopMinhChung(maGV, loaiMinhChung, tieuDe, moTa, namHoc, null, file);
        }

        // PUT: api/MinhChung/Duyet/{id} - Duyệt minh chứng
        [HttpPut("Duyet/{id}")]
        [Authorize(Roles = "CQC,TK,TBM")]
        public async Task<ActionResult> DuyetMinhChung(int id, [FromBody] DuyetMinhChungRequest request)
        {
            var minhChung = await _context.MinhChungs.FindAsync(id);
            if (minhChung == null)
                return NotFound(new { message = "Không tìm thấy minh chứng" });

            minhChung.TrangThai = request.TrangThai; // "Đã duyệt", "Từ chối", "Cần bổ sung"
            minhChung.NguoiDuyet = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            minhChung.NgayDuyet = DateTime.Now;
            minhChung.GhiChuDuyet = request.GhiChu;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã cập nhật trạng thái: {request.TrangThai}" });
        }

        // GET: api/MinhChung/Download/{id} - Download file minh chứng
        [HttpGet("Download/{id}")]
        public async Task<ActionResult> Download(int id)
        {
            var minhChung = await _context.MinhChungs.FindAsync(id);
            if (minhChung == null)
                return NotFound(new { message = "Không tìm thấy minh chứng" });

            if (string.IsNullOrEmpty(minhChung.FilePath) || !System.IO.File.Exists(minhChung.FilePath))
                return NotFound(new { message = "File không tồn tại" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(minhChung.FilePath);
            var contentType = GetContentType(minhChung.FilePath);

            return File(fileBytes, contentType, minhChung.FileName ?? "download");
        }

        // DELETE: api/MinhChung/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.MinhChungs.FindAsync(id);
            if (item == null)
                return NotFound();

            // Xóa file nếu có
            if (!string.IsNullOrEmpty(item.FilePath) && System.IO.File.Exists(item.FilePath))
                System.IO.File.Delete(item.FilePath);

            _context.MinhChungs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/MinhChung/ThongKe - Thống kê minh chứng
        [HttpGet("ThongKe")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult> ThongKe([FromQuery] string? namHoc, [FromQuery] string? maKhoa)
        {
            var query = _context.MinhChungs
                .Include(m => m.GiangVien)
                .AsQueryable();

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(m => m.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(m => m.GiangVien.MaKhoa == maKhoa.ToUpper());

            var result = await query.ToListAsync();

            return Ok(new
            {
                namHoc,
                maKhoa,
                tongSo = result.Count,
                theoTrangThai = result
                    .GroupBy(m => m.TrangThai)
                    .Select(g => new { trangThai = g.Key, soLuong = g.Count() }),
                theoLoai = result
                    .GroupBy(m => m.LoaiMinhChung)
                    .Select(g => new { loai = g.Key, soLuong = g.Count() }),
                theoGiangVien = result
                    .GroupBy(m => new { m.MaGV, m.GiangVien.TenGV })
                    .Select(g => new
                    {
                        maGV = g.Key.MaGV,
                        tenGV = g.Key.TenGV,
                        tongSo = g.Count(),
                        daDuyet = g.Count(m => m.TrangThai == "Đã duyệt"),
                        choDuyet = g.Count(m => m.TrangThai == "Chờ duyệt")
                    })
                    .OrderByDescending(x => x.tongSo)
            });
        }

        // ========== HELPER METHODS ==========

        private async Task<ActionResult> NopMinhChung(string maGV, string loaiMinhChung, string tieuDe, string? moTa, string namHoc, int? idNCKH, IFormFile file)
        {
            var minhChung = new MinhChung
            {
                MaGV = maGV.ToUpper(),
                LoaiMinhChung = loaiMinhChung,
                TieuDe = tieuDe,
                MoTa = moTa,
                NamHoc = namHoc,
                IdNCKH = idNCKH,
                TrangThai = "Chờ duyệt",
                NgayNop = DateTime.Now
            };

            return await SaveMinhChung(minhChung, file);
        }

        private async Task<ActionResult> SaveMinhChung(MinhChung minhChung, IFormFile file)
        {
            // Validate
            if (!await _context.GiangViens.AnyAsync(gv => gv.MaGV == minhChung.MaGV))
                return BadRequest(new { message = "Mã giảng viên không tồn tại" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file" });

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Định dạng file không hợp lệ" });

            // Kiểm tra kích thước (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File không được vượt quá 10MB" });

            // Tạo thư mục lưu file
            var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "MinhChung", minhChung.MaGV);
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Tạo tên file unique
            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cập nhật thông tin file
            minhChung.FilePath = filePath;
            minhChung.FileName = file.FileName;
            minhChung.FileSize = file.Length;
            minhChung.FileType = extension.TrimStart('.');

            minhChung.GiangVien = null;

            _context.MinhChungs.Add(minhChung);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Nộp minh chứng thành công",
                id = minhChung.Id,
                trangThai = minhChung.TrangThai
            });
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }

    // Request model cho duyệt minh chứng
    public class DuyetMinhChungRequest
    {
        public string TrangThai { get; set; } // "Đã duyệt", "Từ chối", "Cần bổ sung"
        public string? GhiChu { get; set; }
    }
}
