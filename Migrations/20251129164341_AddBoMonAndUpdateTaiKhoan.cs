using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherScheduleAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBoMonAndUpdateTaiKhoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                table: "tb_TaiKhoan");

            migrationBuilder.AlterColumn<string>(
                name: "ChucVu",
                table: "tb_TaiKhoan",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<string>(
                name: "MaBM",
                table: "tb_TaiKhoan",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaKhoa",
                table: "tb_TaiKhoan",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaBM",
                table: "tb_GiangVien",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_tb_TaiKhoan_MaBM",
                table: "tb_TaiKhoan",
                column: "MaBM");

            migrationBuilder.CreateIndex(
                name: "IX_tb_TaiKhoan_MaKhoa",
                table: "tb_TaiKhoan",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_GiangVien_MaBM",
                table: "tb_GiangVien",
                column: "MaBM");

            migrationBuilder.CreateIndex(
                name: "IX_tb_BoMon_MaKhoa",
                table: "tb_BoMon",
                column: "MaKhoa");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_GiangVien_tb_BoMon_MaBM",
                table: "tb_GiangVien",
                column: "MaBM",
                principalTable: "tb_BoMon",
                principalColumn: "MaBM");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_TaiKhoan_tb_BoMon_MaBM",
                table: "tb_TaiKhoan",
                column: "MaBM",
                principalTable: "tb_BoMon",
                principalColumn: "MaBM",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                table: "tb_TaiKhoan",
                column: "MaGV",
                principalTable: "tb_GiangVien",
                principalColumn: "MaGV",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_TaiKhoan_tb_Khoa_MaKhoa",
                table: "tb_TaiKhoan",
                column: "MaKhoa",
                principalTable: "tb_Khoa",
                principalColumn: "MaKhoa",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_GiangVien_tb_BoMon_MaBM",
                table: "tb_GiangVien");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_TaiKhoan_tb_BoMon_MaBM",
                table: "tb_TaiKhoan");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                table: "tb_TaiKhoan");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_TaiKhoan_tb_Khoa_MaKhoa",
                table: "tb_TaiKhoan");

            migrationBuilder.DropTable(
                name: "tb_BoMon");

            migrationBuilder.DropIndex(
                name: "IX_tb_TaiKhoan_MaBM",
                table: "tb_TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_tb_TaiKhoan_MaKhoa",
                table: "tb_TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_tb_GiangVien_MaBM",
                table: "tb_GiangVien");

            migrationBuilder.DropColumn(
                name: "MaBM",
                table: "tb_TaiKhoan");

            migrationBuilder.DropColumn(
                name: "MaKhoa",
                table: "tb_TaiKhoan");

            migrationBuilder.DropColumn(
                name: "MaBM",
                table: "tb_GiangVien");

            migrationBuilder.AlterColumn<string>(
                name: "ChucVu",
                table: "tb_TaiKhoan",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_TaiKhoan_tb_GiangVien_MaGV",
                table: "tb_TaiKhoan",
                column: "MaGV",
                principalTable: "tb_GiangVien",
                principalColumn: "MaGV");
        }
    }
}
