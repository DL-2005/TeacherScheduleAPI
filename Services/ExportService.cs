using ClosedXML.Excel;
using TeacherScheduleAPI.Controllers;
using TeacherScheduleAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TeacherScheduleAPI.Services
{
    public class ExportService : IExportService
    {
        public ExportService()
        {
            // Cấu hình QuestPDF license (Community license - miễn phí cho dự án nhỏ)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ==================== EXCEL EXPORTS ====================

        public byte[] ExportGiangVienToExcel(List<GiangVien> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách giảng viên");

            // Header
            var headers = new[] { "STT", "Mã GV", "Họ tên", "Ngày sinh", "Email", "SĐT", "Địa chỉ", "Khoa", "Bộ môn" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data
            for (int i = 0; i < data.Count; i++)
            {
                var gv = data[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = gv.MaGV;
                worksheet.Cell(i + 2, 3).Value = gv.TenGV;
                worksheet.Cell(i + 2, 4).Value = gv.NgaySinh?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(i + 2, 5).Value = gv.Email ?? "";
                worksheet.Cell(i + 2, 6).Value = gv.SDT ?? "";
                worksheet.Cell(i + 2, 7).Value = gv.DiaChi ?? "";
                worksheet.Cell(i + 2, 8).Value = gv.Khoa?.TenKhoa ?? "";
                worksheet.Cell(i + 2, 9).Value = gv.BoMon?.TenBM ?? "";
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add border
            var range = worksheet.Range(1, 1, data.Count + 1, headers.Length);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportPhanCongToExcel(List<PhanCong> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Phân công giảng dạy");

            // Header
            var headers = new[] { "STT", "Mã GV", "Họ tên GV", "Khoa", "Mã MH", "Tên môn học", "Lớp", "Thứ", "Tiết BĐ", "Số tiết", "Phòng", "Thời gian học" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data
            for (int i = 0; i < data.Count; i++)
            {
                var pc = data[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = pc.MaGV;
                worksheet.Cell(i + 2, 3).Value = pc.GiangVien?.TenGV ?? "";
                worksheet.Cell(i + 2, 4).Value = pc.GiangVien?.Khoa?.TenKhoa ?? "";
                worksheet.Cell(i + 2, 5).Value = pc.MaMH;
                worksheet.Cell(i + 2, 6).Value = pc.MonHoc?.TenMH ?? "";
                worksheet.Cell(i + 2, 7).Value = pc.Lop?.Nganh ?? pc.MaLop;
                worksheet.Cell(i + 2, 8).Value = GetThuText(pc.Thu);
                worksheet.Cell(i + 2, 9).Value = pc.TietBatDau;
                worksheet.Cell(i + 2, 10).Value = pc.SoTiet;
                worksheet.Cell(i + 2, 11).Value = pc.PhongHoc ?? "";
                worksheet.Cell(i + 2, 12).Value = pc.ThoiGianHoc ?? "";
            }

            // Auto-fit & border
            worksheet.Columns().AdjustToContents();
            var range = worksheet.Range(1, 1, data.Count + 1, headers.Length);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportThongKeGioGiangToExcel(List<ThongKeGioGiangDto> data, string? thoiGianHoc)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Thống kê giờ giảng");

            // Title
            worksheet.Cell(1, 1).Value = $"THỐNG KÊ GIỜ GIẢNG - {thoiGianHoc ?? "TẤT CẢ"}";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Header
            var headers = new[] { "STT", "Mã GV", "Họ tên GV", "Khoa", "Tổng số tiết", "Số môn học", "Số lớp" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(3, i + 1).Value = headers[i];
                worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
                worksheet.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                worksheet.Cell(i + 4, 1).Value = i + 1;
                worksheet.Cell(i + 4, 2).Value = item.MaGV;
                worksheet.Cell(i + 4, 3).Value = item.TenGV;
                worksheet.Cell(i + 4, 4).Value = item.TenKhoa;
                worksheet.Cell(i + 4, 5).Value = item.TongSoTiet;
                worksheet.Cell(i + 4, 6).Value = item.SoMonHoc;
                worksheet.Cell(i + 4, 7).Value = item.SoLopHoc;
            }

            // Summary row
            var summaryRow = data.Count + 4;
            worksheet.Cell(summaryRow, 1).Value = "TỔNG CỘNG";
            worksheet.Range(summaryRow, 1, summaryRow, 4).Merge();
            worksheet.Cell(summaryRow, 1).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 5).Value = data.Sum(x => x.TongSoTiet);
            worksheet.Cell(summaryRow, 5).Style.Font.Bold = true;

            // Auto-fit & border
            worksheet.Columns().AdjustToContents();
            var range = worksheet.Range(3, 1, data.Count + 4, headers.Length);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportKeKhaiNhiemVuToExcel(KeKhaiNhiemVuDto data)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Kê khai nhiệm vụ");

            int row = 1;

            // === HEADER ===
            ws.Cell(row, 1).Value = "BẢNG KÊ KHAI KHỐI LƯỢNG CÔNG TÁC";
            ws.Range(row, 1, row, 8).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Info GV
            ws.Cell(row, 1).Value = $"Họ tên: {data.GiangVien.TenGV}";
            ws.Cell(row, 4).Value = $"Mã GV: {data.GiangVien.MaGV}";
            row++;
            ws.Cell(row, 1).Value = $"Khoa: {data.GiangVien.Khoa?.TenKhoa}";
            ws.Cell(row, 4).Value = $"Bộ môn: {data.GiangVien.BoMon?.TenBM}";
            row++;
            ws.Cell(row, 1).Value = $"Thời gian học: {data.ThoiGianHoc}";
            ws.Cell(row, 4).Value = $"Năm học: {data.NamHoc}";
            row += 2;

            // === PHẦN 1: GIẢNG DẠY ===
            ws.Cell(row, 1).Value = "I. HOẠT ĐỘNG GIẢNG DẠY";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            ws.Range(row, 1, row, 8).Merge();
            row++;

            var hdGD = new[] { "STT", "Môn học", "Lớp", "Thứ", "Tiết BĐ", "Số tiết", "Phòng" };
            for (int i = 0; i < hdGD.Length; i++)
            {
                ws.Cell(row, i + 1).Value = hdGD[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
            }
            row++;

            for (int i = 0; i < data.PhanCongs.Count; i++)
            {
                var pc = data.PhanCongs[i];
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = pc.MonHoc?.TenMH ?? pc.MaMH;
                ws.Cell(row, 3).Value = pc.Lop?.Nganh ?? pc.MaLop;
                ws.Cell(row, 4).Value = GetThuText(pc.Thu);
                ws.Cell(row, 5).Value = pc.TietBatDau;
                ws.Cell(row, 6).Value = pc.SoTiet;
                ws.Cell(row, 7).Value = pc.PhongHoc ?? "";
                row++;
            }

            ws.Cell(row, 1).Value = "Tổng số tiết giảng dạy:";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 6).Value = data.PhanCongs.Sum(pc => pc.SoTiet);
            ws.Cell(row, 6).Style.Font.Bold = true;
            row += 2;

            // === PHẦN 2: NCKH ===
            ws.Cell(row, 1).Value = "II. NGHIÊN CỨU KHOA HỌC";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            ws.Range(row, 1, row, 8).Merge();
            row++;

            var hdNCKH = new[] { "STT", "Tên đề tài", "Thể loại", "Vai trò", "Giờ NCKH", "Trạng thái" };
            for (int i = 0; i < hdNCKH.Length; i++)
            {
                ws.Cell(row, i + 1).Value = hdNCKH[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
            }
            row++;

            for (int i = 0; i < data.NCKHs.Count; i++)
            {
                var nckh = data.NCKHs[i];
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = nckh.TenDeTai;
                ws.Cell(row, 3).Value = nckh.TheLoai;
                ws.Cell(row, 4).Value = nckh.VaiTro ?? "";
                ws.Cell(row, 5).Value = nckh.GioNCKH;  // int - không cần ??
                ws.Cell(row, 6).Value = nckh.TrangThai ?? "";
                row++;
            }

            ws.Cell(row, 1).Value = "Tổng giờ NCKH:";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 5).Value = data.NCKHs.Sum(n => n.GioNCKH);  // int - không cần ??
            ws.Cell(row, 5).Style.Font.Bold = true;
            row += 2;

            // === PHẦN 3: BỒI DƯỠNG ===
            ws.Cell(row, 1).Value = "III. BỒI DƯỠNG CHUYÊN MÔN";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            ws.Range(row, 1, row, 8).Merge();
            row++;

            var hdBD = new[] { "STT", "Nội dung", "Chi tiết", "Giờ bồi dưỡng", "Ngày thực hiện" };
            for (int i = 0; i < hdBD.Length; i++)
            {
                ws.Cell(row, i + 1).Value = hdBD[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
            }
            row++;

            for (int i = 0; i < data.BoiDuongs.Count; i++)
            {
                var bd = data.BoiDuongs[i];
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = bd.NoiDung;
                ws.Cell(row, 3).Value = bd.ChiTiet ?? "";
                ws.Cell(row, 4).Value = bd.GioBoiDuong;  // int - không cần ??
                ws.Cell(row, 5).Value = bd.NgayThucHien?.ToString("dd/MM/yyyy") ?? "";
                row++;
            }

            ws.Cell(row, 1).Value = "Tổng giờ bồi dưỡng:";
            ws.Range(row, 1, row, 3).Merge();
            ws.Cell(row, 4).Value = data.BoiDuongs.Sum(b => b.GioBoiDuong);  // int - không cần ??
            ws.Cell(row, 4).Style.Font.Bold = true;
            row += 2;

            // === PHẦN 4: NHIỆM VỤ KHÁC ===
            ws.Cell(row, 1).Value = "IV. NHIỆM VỤ KHÁC";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCoral;
            ws.Range(row, 1, row, 8).Merge();
            row++;

            var hdNVK = new[] { "STT", "Công việc", "Chi tiết", "Số giờ", "Ngày thực hiện" };
            for (int i = 0; i < hdNVK.Length; i++)
            {
                ws.Cell(row, i + 1).Value = hdNVK[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
            }
            row++;

            for (int i = 0; i < data.NhiemVuKhacs.Count; i++)
            {
                var nvk = data.NhiemVuKhacs[i];
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = nvk.CongViec;
                ws.Cell(row, 3).Value = nvk.ChiTiet ?? "";
                ws.Cell(row, 4).Value = nvk.SoGio;  // int - không cần ??
                ws.Cell(row, 5).Value = nvk.NgayThucHien?.ToString("dd/MM/yyyy") ?? "";
                row++;
            }

            ws.Cell(row, 1).Value = "Tổng giờ nhiệm vụ khác:";
            ws.Range(row, 1, row, 3).Merge();
            ws.Cell(row, 4).Value = data.NhiemVuKhacs.Sum(n => n.SoGio);  // int - không cần ??
            ws.Cell(row, 4).Style.Font.Bold = true;
            row += 2;

            // === PHẦN 5: TỔNG HỢP ===
            ws.Cell(row, 1).Value = "V. TỔNG HỢP";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Range(row, 1, row, 8).Merge();
            row++;

            var tongGiangDay = data.PhanCongs.Sum(pc => pc.SoTiet);
            var tongNCKH = data.NCKHs.Sum(n => n.GioNCKH);
            var tongBoiDuong = data.BoiDuongs.Sum(b => b.GioBoiDuong);
            var tongNhiemVuKhac = data.NhiemVuKhacs.Sum(n => n.SoGio);
            var tongCong = tongGiangDay + tongNCKH + tongBoiDuong + tongNhiemVuKhac;

            ws.Cell(row, 1).Value = "Tổng giờ giảng dạy:";
            ws.Cell(row, 3).Value = tongGiangDay;
            ws.Cell(row, 4).Value = $"Định mức: {data.DinhMuc?.DinhMucGiangDay ?? 0}";
            row++;
            ws.Cell(row, 1).Value = "Tổng giờ NCKH:";
            ws.Cell(row, 3).Value = tongNCKH;
            ws.Cell(row, 4).Value = $"Định mức: {data.DinhMuc?.DinhMucNCKH ?? 0}";
            row++;
            ws.Cell(row, 1).Value = "Tổng giờ bồi dưỡng:";
            ws.Cell(row, 3).Value = tongBoiDuong;
            ws.Cell(row, 4).Value = $"Định mức: {data.DinhMuc?.DinhMucBoiDuong ?? 0}";
            row++;
            ws.Cell(row, 1).Value = "Tổng giờ nhiệm vụ khác:";
            ws.Cell(row, 3).Value = tongNhiemVuKhac;
            row++;
            ws.Cell(row, 1).Value = "TỔNG CỘNG:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = tongCong;
            ws.Cell(row, 3).Style.Font.Bold = true;

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ==================== PDF EXPORTS ====================

        public byte[] ExportKeKhaiNhiemVuToPdf(KeKhaiNhiemVuDto data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, data));
                    page.Content().Element(c => ComposeKeKhaiContent(c, data));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, KeKhaiNhiemVuDto data)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("BẢNG KÊ KHAI KHỐI LƯỢNG CÔNG TÁC").Bold().FontSize(16);
                column.Item().AlignCenter().Text($"Thời gian học: {data.ThoiGianHoc} - Năm học: {data.NamHoc}").FontSize(12);
                column.Item().Height(10);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Họ tên: {data.GiangVien.TenGV}").Bold();
                    row.RelativeItem().Text($"Mã GV: {data.GiangVien.MaGV}");
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Khoa: {data.GiangVien.Khoa?.TenKhoa}");
                    row.RelativeItem().Text($"Bộ môn: {data.GiangVien.BoMon?.TenBM}");
                });
                column.Item().Height(15);
            });
        }

        private void ComposeKeKhaiContent(IContainer container, KeKhaiNhiemVuDto data)
        {
            container.Column(column =>
            {
                // I. Giảng dạy
                column.Item().Text("I. HOẠT ĐỘNG GIẢNG DẠY").Bold().FontSize(12);
                column.Item().Height(5);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(3).Text("STT").Bold();
                        header.Cell().Border(1).Padding(3).Text("Môn học").Bold();
                        header.Cell().Border(1).Padding(3).Text("Lớp").Bold();
                        header.Cell().Border(1).Padding(3).Text("Thứ").Bold();
                        header.Cell().Border(1).Padding(3).Text("Số tiết").Bold();
                    });

                    for (int i = 0; i < data.PhanCongs.Count; i++)
                    {
                        var pc = data.PhanCongs[i];
                        table.Cell().Border(1).Padding(3).Text((i + 1).ToString());
                        table.Cell().Border(1).Padding(3).Text(pc.MonHoc?.TenMH ?? pc.MaMH);
                        table.Cell().Border(1).Padding(3).Text(pc.Lop?.Nganh ?? pc.MaLop);
                        table.Cell().Border(1).Padding(3).Text(GetThuText(pc.Thu));
                        table.Cell().Border(1).Padding(3).Text(pc.SoTiet.ToString());
                    }
                });
                column.Item().Text($"Tổng số tiết: {data.PhanCongs.Sum(pc => pc.SoTiet)}").Bold();
                column.Item().Height(15);

                // II. NCKH
                column.Item().Text("II. NGHIÊN CỨU KHOA HỌC").Bold().FontSize(12);
                column.Item().Height(5);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(3).Text("STT").Bold();
                        header.Cell().Border(1).Padding(3).Text("Tên đề tài").Bold();
                        header.Cell().Border(1).Padding(3).Text("Vai trò").Bold();
                        header.Cell().Border(1).Padding(3).Text("Giờ NCKH").Bold();
                    });

                    for (int i = 0; i < data.NCKHs.Count; i++)
                    {
                        var nckh = data.NCKHs[i];
                        table.Cell().Border(1).Padding(3).Text((i + 1).ToString());
                        table.Cell().Border(1).Padding(3).Text(nckh.TenDeTai);
                        table.Cell().Border(1).Padding(3).Text(nckh.VaiTro ?? "");
                        table.Cell().Border(1).Padding(3).Text(nckh.GioNCKH.ToString());
                    }
                });
                column.Item().Text($"Tổng giờ NCKH: {data.NCKHs.Sum(n => n.GioNCKH)}").Bold();
                column.Item().Height(15);

                // III. Bồi dưỡng
                column.Item().Text("III. BỒI DƯỠNG CHUYÊN MÔN").Bold().FontSize(12);
                column.Item().Height(5);
                if (data.BoiDuongs.Any())
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(4);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(3).Text("STT").Bold();
                            header.Cell().Border(1).Padding(3).Text("Nội dung").Bold();
                            header.Cell().Border(1).Padding(3).Text("Số giờ").Bold();
                        });

                        for (int i = 0; i < data.BoiDuongs.Count; i++)
                        {
                            var bd = data.BoiDuongs[i];
                            table.Cell().Border(1).Padding(3).Text((i + 1).ToString());
                            table.Cell().Border(1).Padding(3).Text(bd.NoiDung);
                            table.Cell().Border(1).Padding(3).Text(bd.GioBoiDuong.ToString());
                        }
                    });
                }
                column.Item().Text($"Tổng giờ bồi dưỡng: {data.BoiDuongs.Sum(b => b.GioBoiDuong)}").Bold();
                column.Item().Height(15);

                // IV. Nhiệm vụ khác
                column.Item().Text("IV. NHIỆM VỤ KHÁC").Bold().FontSize(12);
                column.Item().Height(5);
                if (data.NhiemVuKhacs.Any())
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(4);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(3).Text("STT").Bold();
                            header.Cell().Border(1).Padding(3).Text("Công việc").Bold();
                            header.Cell().Border(1).Padding(3).Text("Số giờ").Bold();
                        });

                        for (int i = 0; i < data.NhiemVuKhacs.Count; i++)
                        {
                            var nvk = data.NhiemVuKhacs[i];
                            table.Cell().Border(1).Padding(3).Text((i + 1).ToString());
                            table.Cell().Border(1).Padding(3).Text(nvk.CongViec);
                            table.Cell().Border(1).Padding(3).Text(nvk.SoGio.ToString());
                        }
                    });
                }
                column.Item().Text($"Tổng giờ nhiệm vụ khác: {data.NhiemVuKhacs.Sum(n => n.SoGio)}").Bold();
                column.Item().Height(20);

                // V. Tổng hợp
                var tongGD = data.PhanCongs.Sum(pc => pc.SoTiet);
                var tongNCKH = data.NCKHs.Sum(n => n.GioNCKH);
                var tongBD = data.BoiDuongs.Sum(b => b.GioBoiDuong);
                var tongNVK = data.NhiemVuKhacs.Sum(n => n.SoGio);

                column.Item().Text("V. TỔNG HỢP").Bold().FontSize(12);
                column.Item().Height(5);
                column.Item().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Giảng dạy: {tongGD} tiết");
                        r.RelativeItem().Text($"(Định mức: {data.DinhMuc?.DinhMucGiangDay ?? 0})");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"NCKH: {tongNCKH} giờ");
                        r.RelativeItem().Text($"(Định mức: {data.DinhMuc?.DinhMucNCKH ?? 0})");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Bồi dưỡng: {tongBD} giờ");
                        r.RelativeItem().Text($"(Định mức: {data.DinhMuc?.DinhMucBoiDuong ?? 0})");
                    });
                    c.Item().Text($"Nhiệm vụ khác: {tongNVK} giờ");
                    c.Item().Height(5);
                    c.Item().Text($"TỔNG CỘNG: {tongGD + tongNCKH + tongBD + tongNVK} giờ").Bold().FontSize(12);
                });
            });
        }

        public byte[] ExportThongKeToanTruongToPdf(ThongKeToanTruongDto data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("BÁO CÁO THỐNG KÊ TOÀN TRƯỜNG").Bold().FontSize(18);
                        col.Item().AlignCenter().Text($"Thời gian học: {data.ThoiGianHoc} - Năm học: {data.NamHoc}").FontSize(12);
                        col.Item().Height(20);
                    });

                    page.Content().Column(col =>
                    {
                        // Thống kê tổng quan
                        col.Item().Text("I. TỔNG QUAN").Bold().FontSize(14);
                        col.Item().Height(10);
                        col.Item().Border(1).Padding(15).Column(c =>
                        {
                            c.Item().Text($"• Tổng số giảng viên: {data.TongGiangVien}");
                            c.Item().Text($"• Tổng số khoa: {data.TongKhoa}");
                            c.Item().Text($"• Tổng số môn học: {data.TongMonHoc}");
                            c.Item().Text($"• Tổng số lớp: {data.TongLop}");
                            c.Item().Text($"• Tổng số phân công: {data.TongPhanCong}");
                            c.Item().Text($"• Tổng đề tài NCKH: {data.TongNCKH}");
                            c.Item().Text($"• Tổng hoạt động bồi dưỡng: {data.TongBoiDuong}");
                        });
                        col.Item().Height(20);

                        // Thống kê theo khoa
                        col.Item().Text("II. THỐNG KÊ THEO KHOA").Bold().FontSize(14);
                        col.Item().Height(10);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("STT").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Khoa").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Số GV").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Số BM").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Tổng số tiết").Bold();
                            });

                            for (int i = 0; i < data.ThongKeTheoKhoa.Count; i++)
                            {
                                var khoa = data.ThongKeTheoKhoa[i];
                                table.Cell().Border(1).Padding(5).Text((i + 1).ToString());
                                table.Cell().Border(1).Padding(5).Text(khoa.TenKhoa);
                                table.Cell().Border(1).Padding(5).AlignCenter().Text(khoa.SoGiangVien.ToString());
                                table.Cell().Border(1).Padding(5).AlignCenter().Text(khoa.SoBoMon.ToString());
                                table.Cell().Border(1).Padding(5).AlignCenter().Text(khoa.TongSoTiet.ToString());
                            }

                            // Tổng cộng
                            table.Cell().Border(1).Padding(5).Text("");
                            table.Cell().Border(1).Padding(5).Text("TỔNG CỘNG").Bold();
                            table.Cell().Border(1).Padding(5).AlignCenter().Text(data.TongGiangVien.ToString()).Bold();
                            table.Cell().Border(1).Padding(5).AlignCenter().Text(data.ThongKeTheoKhoa.Sum(k => k.SoBoMon).ToString()).Bold();
                            table.Cell().Border(1).Padding(5).AlignCenter().Text(data.ThongKeTheoKhoa.Sum(k => k.TongSoTiet).ToString()).Bold();
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Ngày xuất: ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] ExportLichCongTacToPdf(List<LichCongTac> data, DateTime tuNgay, DateTime denNgay)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("LỊCH CÔNG TÁC").Bold().FontSize(16);
                        col.Item().AlignCenter().Text($"Từ ngày {tuNgay:dd/MM/yyyy} đến ngày {denNgay:dd/MM/yyyy}").FontSize(11);
                        col.Item().Height(15);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("STT").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Ngày").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Giờ").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Nội dung").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Địa điểm").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Chủ trì").Bold();
                            header.Cell().Border(1).Background(Colors.Blue.Lighten3).Padding(5).Text("Thành phần").Bold();
                        });

                        for (int i = 0; i < data.Count; i++)
                        {
                            var lich = data[i];
                            var thu = GetThuFromDate(lich.NgayThang);
                            table.Cell().Border(1).Padding(3).Text((i + 1).ToString());
                            table.Cell().Border(1).Padding(3).Text($"{thu}\n{lich.NgayThang:dd/MM}");
                            table.Cell().Border(1).Padding(3).Text($"{lich.ThoiGianBatDau:hh\\:mm}");
                            table.Cell().Border(1).Padding(3).Text(lich.NoiDung);
                            table.Cell().Border(1).Padding(3).Text(lich.DiaDiem ?? "");
                            table.Cell().Border(1).Padding(3).Text(lich.ChuTri ?? "");
                            table.Cell().Border(1).Padding(3).Text(lich.ThanhPhan ?? "");
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ==================== HELPER METHODS ====================

        private string GetThuText(int thu)
        {
            return thu switch
            {
                2 => "Thứ 2",
                3 => "Thứ 3",
                4 => "Thứ 4",
                5 => "Thứ 5",
                6 => "Thứ 6",
                7 => "Thứ 7",
                8 => "CN",
                _ => thu.ToString()
            };
        }

        private string GetThuFromDate(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }
    }
}
