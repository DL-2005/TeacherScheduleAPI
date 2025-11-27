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

        // GET: api/PhanCong
        // Lấy danh sách lịch dạy (kèm tên GV, tên Môn, tên Lớp để hiển thị cho đẹp)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhanCong>>> GetPhanCongs()
        {
            return await _context.PhanCongs
                .Include(pc => pc.GiangVien) // Join bảng GV
                .Include(pc => pc.MonHoc)    // Join bảng Môn
                .Include(pc => pc.Lop)       // Join bảng Lớp
                .ToListAsync();
        }

        // GET: api/PhanCong/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PhanCong>> GetPhanCong(int id)
        {
            var phanCong = await _context.PhanCongs.FindAsync(id);

            if (phanCong == null)
            {
                return NotFound();
            }

            return phanCong;
        }

        // POST: api/PhanCong
        // Thêm lịch dạy mới (Có kiểm tra trùng lịch)
        [HttpPost]
        public async Task<ActionResult<PhanCong>> PostPhanCong(PhanCong phanCong)
        {
            phanCong.MaGV = phanCong.MaGV?.ToUpper();
            phanCong.MaMH = phanCong.MaMH?.ToUpper();
            phanCong.MaLop = phanCong.MaLop?.ToUpper();
            // 1. Kiểm tra các mã có tồn tại không
            if (!_context.GiangViens.Any(g => g.MaGV == phanCong.MaGV))
                return BadRequest("Mã giảng viên không tồn tại.");
            if (!_context.MonHocs.Any(m => m.MaMH == phanCong.MaMH))
                return BadRequest("Mã môn học không tồn tại.");
            if (!_context.Lops.Any(l => l.MaLop == phanCong.MaLop))
                return BadRequest("Mã lớp không tồn tại.");

            // 2. LOGIC QUAN TRỌNG: Kiểm tra trùng lịch giảng viên
            // Logic: Cùng GV, Cùng Thứ, và Thời gian chồng lấn nhau
            var isConflict = await _context.PhanCongs.AnyAsync(pc =>
                pc.MaGV == phanCong.MaGV && // Cùng GV
                pc.Thu == phanCong.Thu &&   // Cùng thứ
                (
                    // Tiết bắt đầu của lịch mới nằm trong khoảng lịch cũ
                    (phanCong.TietBatDau >= pc.TietBatDau && phanCong.TietBatDau < (pc.TietBatDau + pc.SoTiet)) ||
                    // Hoặc tiết kết thúc của lịch mới chồng lấn lên lịch cũ
                    ((phanCong.TietBatDau + phanCong.SoTiet) > pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) <= (pc.TietBatDau + pc.SoTiet)) ||
                    // Hoặc lịch mới bao trùm lịch cũ
                    (phanCong.TietBatDau <= pc.TietBatDau && (phanCong.TietBatDau + phanCong.SoTiet) >= (pc.TietBatDau + pc.SoTiet))
                )
            );

            if (isConflict)
            {
                return BadRequest("Giảng viên đã có lịch dạy trùng vào thời gian này!");
            }

            // 3. Nếu không trùng thì lưu
            _context.PhanCongs.Add(phanCong);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhanCong", new { id = phanCong.Id }, phanCong);
        }

        // DELETE: api/PhanCong/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhanCong(int id)
        {
            var phanCong = await _context.PhanCongs.FindAsync(id);
            if (phanCong == null)
            {
                return NotFound();
            }

            _context.PhanCongs.Remove(phanCong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}