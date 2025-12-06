using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherScheduleAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQLSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_DinhMuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DinhMucGiangDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DinhMucNCKH = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DinhMucBoiDuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HeSoLopDong = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeSoThucHanh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_DinhMuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Khoa",
                columns: table => new
                {
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Khoa", x => x.MaKhoa);
                });

            migrationBuilder.CreateTable(
                name: "tb_MonHoc",
                columns: table => new
                {
                    MaMH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenMH = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoTinChi = table.Column<int>(type: "int", nullable: false),
                    HeDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_MonHoc", x => x.MaMH);
                });

            migrationBuilder.CreateTable(
                name: "tb_BoMon",
                columns: table => new
                {
                    MaBM = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenBM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_BoMon", x => x.MaBM);
                    table.ForeignKey(
                        name: "FK_tb_BoMon_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa");
                });

            migrationBuilder.CreateTable(
                name: "tb_Lop",
                columns: table => new
                {
                    MaLop = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SiSo = table.Column<int>(type: "int", nullable: false),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Nganh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamHoc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Lop", x => x.MaLop);
                    table.ForeignKey(
                        name: "FK_tb_Lop_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa");
                });

            migrationBuilder.CreateTable(
                name: "tb_VanBan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoVanBan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenVanBan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TheLoai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayBanHanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CoQuanBanHanh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NguoiKy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiTao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_VanBan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_VanBan_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_GiangVien",
                columns: table => new
                {
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenGV = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaBM = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_GiangVien", x => x.MaGV);
                    table.ForeignKey(
                        name: "FK_tb_GiangVien_tb_BoMon_MaBM",
                        column: x => x.MaBM,
                        principalTable: "tb_BoMon",
                        principalColumn: "MaBM");
                    table.ForeignKey(
                        name: "FK_tb_GiangVien_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa");
                });

            migrationBuilder.CreateTable(
                name: "tb_BoiDuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChiTiet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GioBoiDuong = table.Column<int>(type: "int", nullable: false),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgayThucHien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_BoiDuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_BoiDuong_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_LichCongTac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayThang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianBatDau = table.Column<TimeSpan>(type: "time", nullable: true),
                    ThoiGianKetThuc = table.Column<TimeSpan>(type: "time", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThanhPhan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChuTri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LoaiLich = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_LichCongTac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_LichCongTac_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_LichCongTac_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_MinhChung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LoaiMinhChung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdNCKH = table.Column<int>(type: "int", nullable: true),
                    IdBoiDuong = table.Column<int>(type: "int", nullable: true),
                    IdNhiemVuKhac = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NguoiDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_MinhChung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_MinhChung_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_NghienCuuKhoaHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenDeTai = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TheLoai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GioNCKH = table.Column<int>(type: "int", nullable: false),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileMinhChung = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_NghienCuuKhoaHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_NghienCuuKhoaHoc_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_NhiemVuKhac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CongViec = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChiTiet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoGio = table.Column<int>(type: "int", nullable: false),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgayThucHien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_NhiemVuKhac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_NhiemVuKhac_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_PhanCong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaMH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaLop = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TietBatDau = table.Column<int>(type: "int", nullable: false),
                    SoTiet = table.Column<int>(type: "int", nullable: false),
                    Thu = table.Column<int>(type: "int", nullable: false),
                    ThoiGianHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhongHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_PhanCong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_PhanCong_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_PhanCong_tb_Lop_MaLop",
                        column: x => x.MaLop,
                        principalTable: "tb_Lop",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_PhanCong_tb_MonHoc_MaMH",
                        column: x => x.MaMH,
                        principalTable: "tb_MonHoc",
                        principalColumn: "MaMH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_TaiKhoan",
                columns: table => new
                {
                    MaTK = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaBM = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_TaiKhoan", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_tb_TaiKhoan_tb_BoMon_MaBM",
                        column: x => x.MaBM,
                        principalTable: "tb_BoMon",
                        principalColumn: "MaBM",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_TaiKhoan_tb_Khoa_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "tb_Khoa",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_BoiDuong_MaGV",
                table: "tb_BoiDuong",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_BoMon_MaKhoa",
                table: "tb_BoMon",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_GiangVien_MaBM",
                table: "tb_GiangVien",
                column: "MaBM");

            migrationBuilder.CreateIndex(
                name: "IX_tb_GiangVien_MaKhoa",
                table: "tb_GiangVien",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_LichCongTac_MaGV",
                table: "tb_LichCongTac",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_LichCongTac_MaKhoa",
                table: "tb_LichCongTac",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Lop_MaKhoa",
                table: "tb_Lop",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_MinhChung_MaGV",
                table: "tb_MinhChung",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_NghienCuuKhoaHoc_MaGV",
                table: "tb_NghienCuuKhoaHoc",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_NhiemVuKhac_MaGV",
                table: "tb_NhiemVuKhac",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_PhanCong_MaGV",
                table: "tb_PhanCong",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_PhanCong_MaLop",
                table: "tb_PhanCong",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_tb_PhanCong_MaMH",
                table: "tb_PhanCong",
                column: "MaMH");

            migrationBuilder.CreateIndex(
                name: "IX_tb_TaiKhoan_MaBM",
                table: "tb_TaiKhoan",
                column: "MaBM");

            migrationBuilder.CreateIndex(
                name: "IX_tb_TaiKhoan_MaGV",
                table: "tb_TaiKhoan",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_tb_TaiKhoan_MaKhoa",
                table: "tb_TaiKhoan",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_VanBan_MaKhoa",
                table: "tb_VanBan",
                column: "MaKhoa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_BoiDuong");

            migrationBuilder.DropTable(
                name: "tb_DinhMuc");

            migrationBuilder.DropTable(
                name: "tb_LichCongTac");

            migrationBuilder.DropTable(
                name: "tb_MinhChung");

            migrationBuilder.DropTable(
                name: "tb_NghienCuuKhoaHoc");

            migrationBuilder.DropTable(
                name: "tb_NhiemVuKhac");

            migrationBuilder.DropTable(
                name: "tb_PhanCong");

            migrationBuilder.DropTable(
                name: "tb_TaiKhoan");

            migrationBuilder.DropTable(
                name: "tb_VanBan");

            migrationBuilder.DropTable(
                name: "tb_Lop");

            migrationBuilder.DropTable(
                name: "tb_MonHoc");

            migrationBuilder.DropTable(
                name: "tb_GiangVien");

            migrationBuilder.DropTable(
                name: "tb_BoMon");

            migrationBuilder.DropTable(
                name: "tb_Khoa");
        }
    }
}
