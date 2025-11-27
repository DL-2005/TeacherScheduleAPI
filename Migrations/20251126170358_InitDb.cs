using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherScheduleAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "tb_GiangVien",
                columns: table => new
                {
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenGV = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaKhoa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_GiangVien", x => x.MaGV);
                    table.ForeignKey(
                        name: "FK_tb_GiangVien_tb_Khoa_MaKhoa",
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
                name: "tb_TaiKhoan",
                columns: table => new
                {
                    MaTK = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaGV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_TaiKhoan", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                        column: x => x.MaGV,
                        principalTable: "tb_GiangVien",
                        principalColumn: "MaGV");
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

            migrationBuilder.CreateIndex(
                name: "IX_tb_GiangVien_MaKhoa",
                table: "tb_GiangVien",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Lop_MaKhoa",
                table: "tb_Lop",
                column: "MaKhoa");

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
                name: "IX_tb_TaiKhoan_MaGV",
                table: "tb_TaiKhoan",
                column: "MaGV");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_PhanCong");

            migrationBuilder.DropTable(
                name: "tb_TaiKhoan");

            migrationBuilder.DropTable(
                name: "tb_Lop");

            migrationBuilder.DropTable(
                name: "tb_MonHoc");

            migrationBuilder.DropTable(
                name: "tb_GiangVien");

            migrationBuilder.DropTable(
                name: "tb_Khoa");
        }
    }
}
