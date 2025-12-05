using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherScheduleAPI.Models
{
    [Table("tb_MinhChung")]
    public class MinhChung
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại Giảng viên
        [Required]
        [StringLength(10)]
        public string MaGV { get; set; }
        [ForeignKey("MaGV")]
        public GiangVien? GiangVien { get; set; }

        [Required]
        [StringLength(50)]
        public string LoaiMinhChung { get; set; } // "Phiếu báo giảng", "Minh chứng NCKH", "Minh chứng bồi dưỡng", "Khác"

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; }

        public string? MoTa { get; set; }

        [StringLength(20)]
        public string? NamHoc { get; set; }

        // Liên kết với các bảng khác (nếu cần)
        public int? IdNCKH { get; set; } // Liên kết với NghienCuuKhoaHoc
        public int? IdBoiDuong { get; set; } // Liên kết với BoiDuong
        public int? IdNhiemVuKhac { get; set; } // Liên kết với NhiemVuKhac

        // File đính kèm
        [Required]
        public string FilePath { get; set; } // Đường dẫn file
        public string? FileName { get; set; } // Tên file gốc
        public long? FileSize { get; set; } // Kích thước (bytes)
        public string? FileType { get; set; } // "pdf", "docx", "jpg"...

        public DateTime NgayNop { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ duyệt"; // "Chờ duyệt", "Đã duyệt", "Từ chối", "Cần bổ sung"

        // Thông tin duyệt
        public string? NguoiDuyet { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public string? GhiChuDuyet { get; set; }
    }
}
