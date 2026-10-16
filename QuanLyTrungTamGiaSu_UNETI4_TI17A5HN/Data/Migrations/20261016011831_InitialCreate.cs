using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    MaMonHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMonHoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CapDo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.MaMonHoc);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "LopHoc",
                columns: table => new
                {
                    MaLop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    TrinhDoLop = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    HocPhi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHoc", x => x.MaLop);
                    table.ForeignKey(
                        name: "FK_LopHoc_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiaSu",
                columns: table => new
                {
                    MaGiaSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrinhDo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KinhNghiem = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaSu", x => x.MaGiaSu);
                    table.ForeignKey(
                        name: "FK_GiaSu_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocVien",
                columns: table => new
                {
                    MaHocVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrinhDoHienTai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocVien", x => x.MaHocVien);
                    table.ForeignKey(
                        name: "FK_HocVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenMonGiaSu",
                columns: table => new
                {
                    ChuyenMonGiaSuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaSu = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenMonGiaSu", x => x.ChuyenMonGiaSuId);
                    table.ForeignKey(
                        name: "FK_ChuyenMonGiaSu_GiaSu_MaGiaSu",
                        column: x => x.MaGiaSu,
                        principalTable: "GiaSu",
                        principalColumn: "MaGiaSu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuyenMonGiaSu_MonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalTable: "MonHoc",
                        principalColumn: "MaMonHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichHoc",
                columns: table => new
                {
                    MaLichHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    MaGiaSu = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhongHocHoacHinhThuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHoc", x => x.MaLichHoc);
                    table.ForeignKey(
                        name: "FK_LichHoc_GiaSu_MaGiaSu",
                        column: x => x.MaGiaSu,
                        principalTable: "GiaSu",
                        principalColumn: "MaGiaSu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHoc_LopHoc_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHoc",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongGiaSu",
                columns: table => new
                {
                    MaPhanCong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    MaGiaSu = table.Column<int>(type: "int", nullable: false),
                    NgayPhanCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiaSu", x => x.MaPhanCong);
                    table.ForeignKey(
                        name: "FK_PhanCongGiaSu_GiaSu_MaGiaSu",
                        column: x => x.MaGiaSu,
                        principalTable: "GiaSu",
                        principalColumn: "MaGiaSu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiaSu_LopHoc_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHoc",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyLop",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHocVien = table.Column<int>(type: "int", nullable: false),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyLop", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyLop_HocVien_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocVien",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyLop_LopHoc_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHoc",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    MaDiemDanh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLichHoc = table.Column<int>(type: "int", nullable: false),
                    MaHocVien = table.Column<int>(type: "int", nullable: false),
                    TrangThaiDiemDanh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThoiGianGhiNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.MaDiemDanh);
                    table.ForeignKey(
                        name: "FK_DiemDanh_HocVien_MaHocVien",
                        column: x => x.MaHocVien,
                        principalTable: "HocVien",
                        principalColumn: "MaHocVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemDanh_LichHoc_MaLichHoc",
                        column: x => x.MaLichHoc,
                        principalTable: "LichHoc",
                        principalColumn: "MaLichHoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToanHocPhi",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDangKy = table.Column<int>(type: "int", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NguoiGhiNhan = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToanHocPhi", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToanHocPhi_DangKyLop_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyLop",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenMonGiaSu_MaGiaSu",
                table: "ChuyenMonGiaSu",
                column: "MaGiaSu");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenMonGiaSu_MaMonHoc",
                table: "ChuyenMonGiaSu",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyLop_MaHocVien",
                table: "DangKyLop",
                column: "MaHocVien");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyLop_MaLop",
                table: "DangKyLop",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_MaHocVien",
                table: "DiemDanh",
                column: "MaHocVien");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_MaLichHoc_MaHocVien",
                table: "DiemDanh",
                columns: new[] { "MaLichHoc", "MaHocVien" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaSu_MaTaiKhoan",
                table: "GiaSu",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HocVien_MaTaiKhoan",
                table: "HocVien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MaGiaSu",
                table: "LichHoc",
                column: "MaGiaSu");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_MaLop",
                table: "LichHoc",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_MaMonHoc",
                table: "LopHoc",
                column: "MaMonHoc");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_TenMonHoc",
                table: "MonHoc",
                column: "TenMonHoc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiaSu_MaGiaSu",
                table: "PhanCongGiaSu",
                column: "MaGiaSu");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiaSu_MaLop",
                table: "PhanCongGiaSu",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanHocPhi_MaDangKy",
                table: "ThanhToanHocPhi",
                column: "MaDangKy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuyenMonGiaSu");

            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "PhanCongGiaSu");

            migrationBuilder.DropTable(
                name: "ThanhToanHocPhi");

            migrationBuilder.DropTable(
                name: "LichHoc");

            migrationBuilder.DropTable(
                name: "DangKyLop");

            migrationBuilder.DropTable(
                name: "GiaSu");

            migrationBuilder.DropTable(
                name: "HocVien");

            migrationBuilder.DropTable(
                name: "LopHoc");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "MonHoc");
        }
    }
}
