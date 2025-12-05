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
    public class VanBanController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VanBanController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/VanBan - Danh sách văn bản
        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] string? theLoai,
            [FromQuery] string? namHoc,
            [FromQuery] string? maKhoa,
            [FromQuery] string? trangThai,
            [FromQuery] string? tuKhoa)
        {
            var query = _context.VanBans
                .Include(v => v.Khoa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(theLoai))
                query = query.Where(v => v.TheLoai == theLoai);

            if (!string.IsNullOrEmpty(namHoc))
                query = query.Where(v => v.NamHoc == namHoc);

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(v => v.MaKhoa == maKhoa.ToUpper());

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(v => v.TrangThai == trangThai);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(v => v.TenVanBan.Contains(tuKhoa) || v.SoVanBan.Contains(tuKhoa));

            var result = await query
                .OrderByDescending(v => v.NgayBanHanh)
                .ToListAsync();

            return Ok(new
            {
                tongSo = result.Count,
                danhSach = result.Select(v => new
                {
                    v.Id,
                    v.SoVanBan,
                    v.TenVanBan,
                    v.TheLoai,
                    v.NgayBanHanh,
                    v.NamHoc,
                    v.CoQuanBanHanh,
                    v.NguoiKy,
                    v.TrichYeu,
                    v.MaKhoa,
                    tenKhoa = v.Khoa?.TenKhoa,
                    v.TrangThai,
                    coFile = !string.IsNullOrEmpty(v.FilePath),
                    v.FileName,
                    fileSize = v.FileSize.HasValue ? FormatFileSize(v.FileSize.Value) : null
                })
            });
        }

        // GET: api/VanBan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var item = await _context.VanBans
                .Include(v => v.Khoa)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy văn bản" });

            return Ok(item);
        }

        // GET: api/VanBan/TheLoai - Lấy danh sách thể loại
        [HttpGet("TheLoai")]
        public ActionResult GetTheLoai()
        {
            return Ok(new[]
            {
                "Thông báo",
                "Quyết định",
                "Công văn",
                "Kế hoạch",
                "Báo cáo",
                "Hướng dẫn",
                "Biên bản",
                "Khác"
            });
        }

        // POST: api/VanBan - Tạo văn bản mới (không upload file)
        [HttpPost]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult> Create(VanBan model)
        {
            model.MaKhoa = model.MaKhoa?.ToUpper();
            model.NgayTao = DateTime.Now;
            model.NguoiTao = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra số văn bản trùng
            if (await _context.VanBans.AnyAsync(v => v.SoVanBan == model.SoVanBan))
                return BadRequest(new { message = "Số văn bản đã tồn tại" });

            // Kiểm tra khoa tồn tại
            if (!string.IsNullOrEmpty(model.MaKhoa))
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == model.MaKhoa))
                    return BadRequest(new { message = "Mã khoa không tồn tại" });
            }

            model.Khoa = null;

            _context.VanBans.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // POST: api/VanBan/Upload/{id} - Upload file cho văn bản
        [HttpPost("Upload/{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<ActionResult> UploadFile(int id, IFormFile file)
        {
            var vanBan = await _context.VanBans.FindAsync(id);
            if (vanBan == null)
                return NotFound(new { message = "Không tìm thấy văn bản" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file" });

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận file PDF, DOC, DOCX, XLS, XLSX" });

            // Kiểm tra kích thước (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File không được vượt quá 10MB" });

            // Tạo thư mục lưu file
            var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "VanBan");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Tạo tên file unique
            var fileName = $"{vanBan.Id}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(vanBan.FilePath) && System.IO.File.Exists(vanBan.FilePath))
                System.IO.File.Delete(vanBan.FilePath);

            // Lưu file mới
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cập nhật database
            vanBan.FilePath = filePath;
            vanBan.FileName = file.FileName;
            vanBan.FileSize = file.Length;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Upload file thành công",
                fileName = file.FileName,
                fileSize = FormatFileSize(file.Length)
            });
        }

        // GET: api/VanBan/Download/{id} - Download file văn bản
        [HttpGet("Download/{id}")]
        public async Task<ActionResult> Download(int id)
        {
            var vanBan = await _context.VanBans.FindAsync(id);
            if (vanBan == null)
                return NotFound(new { message = "Không tìm thấy văn bản" });

            if (string.IsNullOrEmpty(vanBan.FilePath) || !System.IO.File.Exists(vanBan.FilePath))
                return NotFound(new { message = "File không tồn tại" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(vanBan.FilePath);
            var contentType = GetContentType(vanBan.FilePath);

            return File(fileBytes, contentType, vanBan.FileName ?? "download");
        }

        // PUT: api/VanBan/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CQC,TK")]
        public async Task<IActionResult> Update(int id, VanBan model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "Id không khớp" });

            // Giữ lại thông tin file
            var existing = await _context.VanBans.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (existing == null)
                return NotFound();

            model.FilePath = existing.FilePath;
            model.FileName = existing.FileName;
            model.FileSize = existing.FileSize;
            model.MaKhoa = model.MaKhoa?.ToUpper();
            model.Khoa = null;

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.VanBans.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/VanBan/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CQC")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.VanBans.FindAsync(id);
            if (item == null)
                return NotFound();

            // Xóa file nếu có
            if (!string.IsNullOrEmpty(item.FilePath) && System.IO.File.Exists(item.FilePath))
                System.IO.File.Delete(item.FilePath);

            _context.VanBans.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper: Format file size
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }

        // Helper: Get content type
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
                _ => "application/octet-stream"
            };
        }
    }
}
