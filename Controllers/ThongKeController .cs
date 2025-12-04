using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherScheduleAPI.Data;

namespace TeacherScheduleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== THỐNG KÊ CÁ NHÂN ====================

        // GET: api/ThongKe/CaNhan/{maGV} - Dashboard cá nhân của giảng viên
        [HttpGet("CaNhan/{maGV}")]
        public async Task<ActionResult> ThongKeCaNhan(string maGV, [FromQuery] string? namHoc)
        {
            maGV = maGV.ToUpper();

            // 1. Lấy thông tin giảng viên
            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // 2. Lấy định mức (nếu có)
            var dinhMuc = !string.IsNullOrEmpty(namHoc)
                ? await _context.DinhMucs.FirstOrDefaultAsync(d => d.NamHoc == namHoc)
                : await _context.DinhMucs.OrderByDescending(d => d.NamHoc).FirstOrDefaultAsync();

            // 3. Lấy dữ liệu phân công giảng dạy
            var phanCongQuery = _context.PhanCongs.Where(pc => pc.MaGV == maGV);

            // Xử lý lọc theo năm học - TÁCH BIẾN RA NGOÀI LINQ
            if (!string.IsNullOrEmpty(namHoc))
            {
                var namHocFilter = namHoc.Split('-')[0]; // Lấy năm đầu, ví dụ: "2024" từ "2024-2025"
                phanCongQuery = phanCongQuery.Where(pc => pc.ThoiGianHoc != null && pc.ThoiGianHoc.Contains(namHocFilter));
            }

            var phanCongs = await phanCongQuery
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .ToListAsync();

            var tongSoTietGiangDay = phanCongs.Sum(pc => pc.SoTiet);

            // 4. Lấy dữ liệu NCKH
            var nckhQuery = _context.NghienCuuKhoaHocs.Where(n => n.MaGV == maGV);
            if (!string.IsNullOrEmpty(namHoc))
                nckhQuery = nckhQuery.Where(n => n.NamHoc == namHoc);

            var nckhs = await nckhQuery.ToListAsync();
            var tongGioNCKH = nckhs.Sum(n => n.GioNCKH);

            // 5. Lấy dữ liệu bồi dưỡng
            var boiDuongQuery = _context.BoiDuongs.Where(b => b.MaGV == maGV);
            if (!string.IsNullOrEmpty(namHoc))
                boiDuongQuery = boiDuongQuery.Where(b => b.NamHoc == namHoc);

            var boiDuongs = await boiDuongQuery.ToListAsync();
            var tongGioBoiDuong = boiDuongs.Sum(b => b.GioBoiDuong);

            // 6. Lấy dữ liệu nhiệm vụ khác
            var nhiemVuQuery = _context.NhiemVuKhacs.Where(n => n.MaGV == maGV);
            if (!string.IsNullOrEmpty(namHoc))
                nhiemVuQuery = nhiemVuQuery.Where(n => n.NamHoc == namHoc);

            var nhiemVus = await nhiemVuQuery.ToListAsync();
            var tongGioNhiemVuKhac = nhiemVus.Sum(n => n.SoGio);

            // 7. Tính thừa/thiếu
            decimal dinhMucGiangDay = dinhMuc?.DinhMucGiangDay ?? 270;
            decimal dinhMucNCKH = dinhMuc?.DinhMucNCKH ?? 150;
            decimal dinhMucBoiDuong = dinhMuc?.DinhMucBoiDuong ?? 80;

            var thuaThieuGiangDay = tongSoTietGiangDay - dinhMucGiangDay;
            var thuaThieuNCKH = tongGioNCKH - dinhMucNCKH;
            var thuaThieuBoiDuong = tongGioBoiDuong - dinhMucBoiDuong;

            return Ok(new
            {
                // Thông tin giảng viên
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

                namHoc = namHoc ?? dinhMuc?.NamHoc,

                // Tổng hợp số liệu
                tongHop = new
                {
                    // Giảng dạy
                    tongSoTietGiangDay,
                    dinhMucGiangDay,
                    thuaThieuGiangDay,
                    trangThaiGiangDay = thuaThieuGiangDay >= 0 ? "Đủ" : "Thiếu",

                    // NCKH
                    tongGioNCKH,
                    dinhMucNCKH,
                    thuaThieuNCKH,
                    trangThaiNCKH = thuaThieuNCKH >= 0 ? "Đủ" : "Thiếu",

                    // Bồi dưỡng
                    tongGioBoiDuong,
                    dinhMucBoiDuong,
                    thuaThieuBoiDuong,
                    trangThaiBoiDuong = thuaThieuBoiDuong >= 0 ? "Đủ" : "Thiếu",

                    // Nhiệm vụ khác
                    tongGioNhiemVuKhac
                },

                // Chi tiết giảng dạy
                giangDay = new
                {
                    soHocPhan = phanCongs.Select(pc => pc.MaMH).Distinct().Count(),
                    soLop = phanCongs.Select(pc => pc.MaLop).Distinct().Count(),
                    danhSach = phanCongs.Select(pc => new
                    {
                        pc.MaMH,
                        TenMH = pc.MonHoc?.TenMH,
                        SoTinChi = pc.MonHoc?.SoTinChi,
                        pc.MaLop,
                        SiSo = pc.Lop?.SiSo,
                        pc.SoTiet,
                        pc.Thu,
                        pc.TietBatDau,
                        pc.PhongHoc
                    })
                },

                // Chi tiết NCKH
                nghienCuuKhoaHoc = new
                {
                    soDeTai = nckhs.Count,
                    theoTheLoai = nckhs.GroupBy(n => n.TheLoai).Select(g => new
                    {
                        theLoai = g.Key,
                        soLuong = g.Count(),
                        tongGio = g.Sum(x => x.GioNCKH)
                    }),
                    danhSach = nckhs.Select(n => new
                    {
                        n.TenDeTai,
                        n.TheLoai,
                        n.VaiTro,
                        n.GioNCKH,
                        n.TrangThai
                    })
                },

                // Chi tiết bồi dưỡng
                boiDuong = new
                {
                    soHoatDong = boiDuongs.Count,
                    danhSach = boiDuongs.Select(b => new
                    {
                        b.NoiDung,
                        b.ChiTiet,
                        b.GioBoiDuong
                    })
                },

                // Chi tiết nhiệm vụ khác
                nhiemVuKhac = new
                {
                    soNhiemVu = nhiemVus.Count,
                    danhSach = nhiemVus.Select(n => new
                    {
                        n.CongViec,
                        n.ChiTiet,
                        n.SoGio
                    })
                }
            });
        }

        // ==================== THỐNG KÊ THEO KHOA ====================

        // GET: api/ThongKe/Khoa/{maKhoa}
        [HttpGet("Khoa/{maKhoa}")]
        public async Task<ActionResult> ThongKeKhoa(string maKhoa, [FromQuery] string? namHoc)
        {
            maKhoa = maKhoa.ToUpper();

            var khoa = await _context.Khoas.FindAsync(maKhoa);
            if (khoa == null)
                return NotFound(new { message = "Không tìm thấy khoa" });

            // Lấy danh sách giảng viên của khoa
            var giangViens = await _context.GiangViens
                .Where(gv => gv.MaKhoa == maKhoa)
                .ToListAsync();

            var maGVs = giangViens.Select(gv => gv.MaGV).ToList();

            // Thống kê giảng dạy - TÁCH BIẾN RA NGOÀI
            var phanCongQuery = _context.PhanCongs.Where(pc => maGVs.Contains(pc.MaGV));
            if (!string.IsNullOrEmpty(namHoc))
            {
                var namHocFilter = namHoc.Split('-')[0];
                phanCongQuery = phanCongQuery.Where(pc => pc.ThoiGianHoc != null && pc.ThoiGianHoc.Contains(namHocFilter));
            }

            var phanCongs = await phanCongQuery.ToListAsync();

            // Thống kê NCKH
            var nckhQuery = _context.NghienCuuKhoaHocs.Where(n => maGVs.Contains(n.MaGV));
            if (!string.IsNullOrEmpty(namHoc))
                nckhQuery = nckhQuery.Where(n => n.NamHoc == namHoc);

            var nckhs = await nckhQuery.ToListAsync();

            // Thống kê theo từng giảng viên
            var thongKeTheoGV = giangViens.Select(gv => new
            {
                gv.MaGV,
                gv.TenGV,
                tongSoTietGiangDay = phanCongs.Where(pc => pc.MaGV == gv.MaGV).Sum(pc => pc.SoTiet),
                tongGioNCKH = nckhs.Where(n => n.MaGV == gv.MaGV).Sum(n => n.GioNCKH),
                soHocPhan = phanCongs.Where(pc => pc.MaGV == gv.MaGV).Select(pc => pc.MaMH).Distinct().Count(),
                soDeTaiNCKH = nckhs.Count(n => n.MaGV == gv.MaGV)
            }).OrderByDescending(x => x.tongSoTietGiangDay).ToList();

            return Ok(new
            {
                khoa = new
                {
                    khoa.MaKhoa,
                    khoa.TenKhoa
                },
                namHoc,
                tongSoGiangVien = giangViens.Count,
                tongHop = new
                {
                    tongSoTietGiangDay = phanCongs.Sum(pc => pc.SoTiet),
                    tongGioNCKH = nckhs.Sum(n => n.GioNCKH),
                    tongSoHocPhan = phanCongs.Select(pc => pc.MaMH).Distinct().Count(),
                    tongSoDeTaiNCKH = nckhs.Count
                },
                thongKeTheoGV
            });
        }

        // ==================== THỐNG KÊ TOÀN TRƯỜNG ====================

        // GET: api/ThongKe/ToanTruong
        [HttpGet("ToanTruong")]
        [Authorize(Roles = "CQC")]
        public async Task<ActionResult> ThongKeToanTruong([FromQuery] string? namHoc)
        {
            // Tổng số liệu
            var tongGiangVien = await _context.GiangViens.CountAsync();
            var tongKhoa = await _context.Khoas.CountAsync();
            var tongBoMon = await _context.BoMons.CountAsync();
            var tongLop = await _context.Lops.CountAsync();
            var tongMonHoc = await _context.MonHocs.CountAsync();

            // Phân công - TÁCH BIẾN RA NGOÀI
            var phanCongQuery = _context.PhanCongs.AsQueryable();
            if (!string.IsNullOrEmpty(namHoc))
            {
                var namHocFilter = namHoc.Split('-')[0];
                phanCongQuery = phanCongQuery.Where(pc => pc.ThoiGianHoc != null && pc.ThoiGianHoc.Contains(namHocFilter));
            }

            var tongPhanCong = await phanCongQuery.CountAsync();
            var tongSoTiet = await phanCongQuery.SumAsync(pc => pc.SoTiet);

            // NCKH
            var nckhQuery = _context.NghienCuuKhoaHocs.AsQueryable();
            if (!string.IsNullOrEmpty(namHoc))
                nckhQuery = nckhQuery.Where(n => n.NamHoc == namHoc);

            var tongDeTaiNCKH = await nckhQuery.CountAsync();
            var tongGioNCKH = await nckhQuery.SumAsync(n => n.GioNCKH);

            // Thống kê NCKH theo thể loại
            var nckhTheoLoai = await nckhQuery
                .GroupBy(n => n.TheLoai)
                .Select(g => new
                {
                    theLoai = g.Key,
                    soLuong = g.Count(),
                    tongGio = g.Sum(x => x.GioNCKH)
                })
                .ToListAsync();

            // Thống kê theo khoa
            var thongKeTheoKhoa = await _context.Khoas
                .Select(k => new
                {
                    k.MaKhoa,
                    k.TenKhoa,
                    soGiangVien = _context.GiangViens.Count(gv => gv.MaKhoa == k.MaKhoa),
                    soBoMon = _context.BoMons.Count(bm => bm.MaKhoa == k.MaKhoa)
                })
                .ToListAsync();

            return Ok(new
            {
                namHoc,
                tongQuan = new
                {
                    tongGiangVien,
                    tongKhoa,
                    tongBoMon,
                    tongLop,
                    tongMonHoc,
                    tongPhanCong,
                    tongSoTiet,
                    tongDeTaiNCKH,
                    tongGioNCKH
                },
                nckhTheoLoai,
                thongKeTheoKhoa
            });
        }

        // ==================== BÁO CÁO KÊ KHAI NHIỆM VỤ ====================

        // GET: api/ThongKe/KeKhaiNhiemVu/{maGV} - Bảng kê khai nhiệm vụ cuối năm
        [HttpGet("KeKhaiNhiemVu/{maGV}")]
        public async Task<ActionResult> KeKhaiNhiemVu(string maGV, [FromQuery] string namHoc)
        {
            if (string.IsNullOrEmpty(namHoc))
                return BadRequest(new { message = "Vui lòng cung cấp năm học" });

            maGV = maGV.ToUpper();

            var giangVien = await _context.GiangViens
                .Include(gv => gv.Khoa)
                .Include(gv => gv.BoMon)
                .FirstOrDefaultAsync(gv => gv.MaGV == maGV);

            if (giangVien == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // Lấy định mức
            var dinhMuc = await _context.DinhMucs.FirstOrDefaultAsync(d => d.NamHoc == namHoc);

            // Phần I: Định mức lao động được giao
            decimal dinhMucGiangDay = dinhMuc?.DinhMucGiangDay ?? 270;
            decimal dinhMucNCKH = dinhMuc?.DinhMucNCKH ?? 150;
            decimal dinhMucBoiDuong = dinhMuc?.DinhMucBoiDuong ?? 80;

            // Phần II: Công tác giảng dạy - TÁCH BIẾN RA NGOÀI
            var namHocFilter = namHoc.Split('-')[0];
            var phanCongs = await _context.PhanCongs
                .Where(pc => pc.MaGV == maGV && pc.ThoiGianHoc != null && pc.ThoiGianHoc.Contains(namHocFilter))
                .Include(pc => pc.MonHoc)
                .Include(pc => pc.Lop)
                .ToListAsync();

            var tongGiangDay = phanCongs.Sum(pc => pc.SoTiet);

            // Phần III: Nghiên cứu khoa học
            var nckhs = await _context.NghienCuuKhoaHocs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var tongNCKH = nckhs.Sum(n => n.GioNCKH);

            // Phần IV: Hoạt động chuyên môn khác (Bồi dưỡng + Nhiệm vụ khác)
            var boiDuongs = await _context.BoiDuongs
                .Where(b => b.MaGV == maGV && b.NamHoc == namHoc)
                .ToListAsync();

            var nhiemVuKhacs = await _context.NhiemVuKhacs
                .Where(n => n.MaGV == maGV && n.NamHoc == namHoc)
                .ToListAsync();

            var tongBoiDuong = boiDuongs.Sum(b => b.GioBoiDuong);
            var tongNhiemVuKhac = nhiemVuKhacs.Sum(n => n.SoGio);
            var tongHoatDongCMKhac = tongBoiDuong + tongNhiemVuKhac;

            // Phần V: Tổng hợp thực hiện định mức
            var thuaThieuGiangDay = tongGiangDay - dinhMucGiangDay;
            var thuaThieuNCKH = tongNCKH - dinhMucNCKH;
            var thuaThieuBoiDuong = tongHoatDongCMKhac - dinhMucBoiDuong;

            return Ok(new
            {
                tieuDe = "BẢNG KÊ KHAI CÁC NHIỆM VỤ ĐÃ THỰC HIỆN",
                namHoc,

                thongTinGiangVien = new
                {
                    giangVien.MaGV,
                    giangVien.TenGV,
                    donVi = giangVien.Khoa?.TenKhoa,
                    toChuyenMon = giangVien.BoMon?.TenBM
                },

                phanI_DinhMuc = new
                {
                    tieuDe = "I. Định mức lao động được giao",
                    danhSach = new[]
                    {
                        new { stt = 1, noiDung = "Giảng dạy", dinhMucChuan = dinhMucGiangDay, giamTru = 0m, dinhMucThucTe = dinhMucGiangDay },
                        new { stt = 2, noiDung = "Nghiên cứu khoa học", dinhMucChuan = dinhMucNCKH, giamTru = 0m, dinhMucThucTe = dinhMucNCKH },
                        new { stt = 3, noiDung = "Sinh hoạt chuyên môn", dinhMucChuan = dinhMucBoiDuong, giamTru = 0m, dinhMucThucTe = dinhMucBoiDuong }
                    }
                },

                phanII_GiangDay = new
                {
                    tieuDe = "II. Công tác giảng dạy; thực tập; thực hành; thực tế chuyên môn; kiểm tra, đánh giá, các nhiệm vụ khác",
                    tongSoGioChuanQuyDoi = tongGiangDay,
                    danhSach = phanCongs.Select((pc, index) => new
                    {
                        stt = index + 1,
                        noiDung = pc.MonHoc?.TenMH ?? pc.MaMH,
                        lop = pc.MaLop,
                        gioChuanQuyDoi = pc.SoTiet
                    })
                },

                phanIII_NCKH = new
                {
                    tieuDe = "III. Nghiên cứu khoa học",
                    tongSoGioNCKH = tongNCKH,
                    danhSach = nckhs.Select((n, index) => new
                    {
                        stt = index + 1,
                        noiDung = n.TheLoai,
                        tenDeTai = n.TenDeTai,
                        gioNCKHQuyDoi = n.GioNCKH
                    })
                },

                phanIV_HoatDongKhac = new
                {
                    tieuDe = "IV. Hoạt động chuyên môn khác",
                    tongSoGioQuyDoi = tongHoatDongCMKhac,
                    boiDuong = boiDuongs.Select((b, index) => new
                    {
                        stt = index + 1,
                        noiDung = b.NoiDung,
                        chiTiet = b.ChiTiet,
                        soGioDaThucHien = b.GioBoiDuong
                    }),
                    nhiemVuKhac = nhiemVuKhacs.Select((n, index) => new
                    {
                        stt = index + 1,
                        noiDung = n.CongViec,
                        chiTiet = n.ChiTiet,
                        soGioDaThucHien = n.SoGio
                    })
                },

                phanV_TongHop = new
                {
                    tieuDe = "V. Tổng hợp thực hiện định mức lao động",
                    danhSach = new[]
                    {
                        new { stt = 1, noiDung = "Giảng dạy", dinhMucThucTe = dinhMucGiangDay, tongSoGioDaThucHien = (decimal)tongGiangDay, thuaThieu = thuaThieuGiangDay },
                        new { stt = 2, noiDung = "Nghiên cứu khoa học", dinhMucThucTe = dinhMucNCKH, tongSoGioDaThucHien = (decimal)tongNCKH, thuaThieu = thuaThieuNCKH },
                        new { stt = 3, noiDung = "Sinh hoạt chuyên môn", dinhMucThucTe = dinhMucBoiDuong, tongSoGioDaThucHien = (decimal)tongHoatDongCMKhac, thuaThieu = thuaThieuBoiDuong }
                    }
                },

                ketLuan = new
                {
                    giangDay = thuaThieuGiangDay >= 0 ? $"Thừa {thuaThieuGiangDay} tiết" : $"Thiếu {Math.Abs(thuaThieuGiangDay)} tiết",
                    nckh = thuaThieuNCKH >= 0 ? $"Thừa {thuaThieuNCKH} giờ" : $"Thiếu {Math.Abs(thuaThieuNCKH)} giờ",
                    boiDuong = thuaThieuBoiDuong >= 0 ? $"Thừa {thuaThieuBoiDuong} giờ" : $"Thiếu {Math.Abs(thuaThieuBoiDuong)} giờ"
                }
            });
        }
    }
}