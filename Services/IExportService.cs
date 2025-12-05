using TeacherScheduleAPI.Controllers;
using TeacherScheduleAPI.Models;

namespace TeacherScheduleAPI.Services
{
    public interface IExportService
    {
        // Excel exports
        byte[] ExportGiangVienToExcel(List<GiangVien> data);
        byte[] ExportPhanCongToExcel(List<PhanCong> data);
        byte[] ExportThongKeGioGiangToExcel(List<ThongKeGioGiangDto> data, string? thoiGianHoc);
        byte[] ExportKeKhaiNhiemVuToExcel(KeKhaiNhiemVuDto data);

        // PDF exports
        byte[] ExportKeKhaiNhiemVuToPdf(KeKhaiNhiemVuDto data);
        byte[] ExportThongKeToanTruongToPdf(ThongKeToanTruongDto data);
        byte[] ExportLichCongTacToPdf(List<LichCongTac> data, DateTime tuNgay, DateTime denNgay);
    }
}
