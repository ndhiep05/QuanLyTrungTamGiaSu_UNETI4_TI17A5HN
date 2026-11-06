using ClosedXML.Excel;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Services
{
    public class ExcelExportService
    {
        private static readonly XLColor PrimaryHeaderBg = XLColor.FromHtml("#4F46E5"); // Brand indigo
        private static readonly XLColor TitleBg = XLColor.FromHtml("#1E1B4B");         // Dark indigo
        private static readonly XLColor ZebraBg = XLColor.FromHtml("#F9FAFB");         // Light gray
        private static readonly XLColor BorderColor = XLColor.FromHtml("#D1D5DB");     // Light border

        /// <summary>
        /// Xuất danh sách môn học ra định dạng Excel (.xlsx)
        /// </summary>
        public byte[] XuatDanhSachMonHoc(IEnumerable<MonHoc> danhSach, string? nguoiXuat = null, string? thongTinBoLoc = null)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachMonHoc");
            ws.ShowGridLines = true;

            // 1. Tiêu đề chính
            ws.Cell(1, 1).Value = "DANH SÁCH MÔN HỌC - TRUNG TÂM GIA SƯ";
            ws.Range("A1:F1").Merge();
            ws.Range("A1:F1").Style.Font.Bold = true;
            ws.Range("A1:F1").Style.Font.FontSize = 16;
            ws.Range("A1:F1").Style.Font.FontColor = XLColor.White;
            ws.Range("A1:F1").Style.Fill.BackgroundColor = TitleBg;
            ws.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range("A1:F1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(1).Height = 35;

            // 2. Thông tin phụ
            var thoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            ws.Cell(2, 1).Value = $"Thời gian xuất: {thoiGian}" + (!string.IsNullOrWhiteSpace(nguoiXuat) ? $" | Người xuất: {nguoiXuat}" : "");
            ws.Range("A2:F2").Merge();
            ws.Range("A2:F2").Style.Font.Italic = true;
            ws.Range("A2:F2").Style.Font.FontSize = 10;
            ws.Range("A2:F2").Style.Font.FontColor = XLColor.FromHtml("#4B5563");
            ws.Range("A2:F2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 20;

            ws.Cell(3, 1).Value = $"Bộ lọc: {thongTinBoLoc ?? "Tất cả"} | Tổng số môn học: {danhSach.Count()}";
            ws.Range("A3:F3").Merge();
            ws.Range("A3:F3").Style.Font.FontSize = 10;
            ws.Range("A3:F3").Style.Font.FontColor = XLColor.FromHtml("#4B5563");
            ws.Range("A3:F3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(3).Height = 20;

            // 3. Tiêu đề bảng (Header)
            string[] headers = { "STT", "Mã môn", "Tên môn học", "Cấp độ", "Trạng thái", "Mô tả" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(5, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = PrimaryHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            ws.Row(5).Height = 26;

            // 4. Dữ liệu
            int row = 6;
            int stt = 1;
            foreach (var item in danhSach)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 2).Value = item.MaMonHoc;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 3).Value = item.TenMonHoc;
                ws.Cell(row, 3).Style.Font.Bold = true;

                ws.Cell(row, 4).Value = item.CapDo;
                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var trangThaiStr = item.TrangThai.Equals("HoatDong", StringComparison.OrdinalIgnoreCase) || item.TrangThai == "Hoạt động"
                    ? "Hoạt động"
                    : "Khóa";
                ws.Cell(row, 5).Value = trangThaiStr;
                ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 6).Value = item.MoTa ?? "";

                // Định dạng viền và màu nền so le
                for (int col = 1; col <= headers.Length; col++)
                {
                    var c = ws.Cell(row, col);
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    c.Style.Border.OutsideBorderColor = BorderColor;
                    c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    if (stt % 2 == 1) // Hàng chẵn so le
                    {
                        c.Style.Fill.BackgroundColor = ZebraBg;
                    }
                }

                ws.Row(row).Height = 22;
                row++;
            }

            // Đóng khung viền cho header
            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(5, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(5, col).Style.Border.OutsideBorderColor = BorderColor;
            }

            // 5. Dòng tổng kết
            ws.Cell(row, 1).Value = "Tổng cộng";
            ws.Range(row, 1, row, 2).Merge();
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            ws.Range(row, 1, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(row, 3).Value = $"{danhSach.Count()} môn học";
            ws.Cell(row, 3).Style.Font.Bold = true;

            for (int col = 1; col <= headers.Length; col++)
            {
                var c = ws.Cell(row, col);
                c.Style.Border.TopBorder = XLBorderStyleValues.Double;
                c.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = BorderColor;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            }
            ws.Row(row).Height = 24;

            // Tự động căn chỉnh độ rộng cột
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 8);  // STT
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 12); // Mã môn
            ws.Column(3).Width = Math.Max(ws.Column(3).Width, 25); // Tên môn
            ws.Column(4).Width = Math.Max(ws.Column(4).Width, 15); // Cấp độ
            ws.Column(5).Width = Math.Max(ws.Column(5).Width, 15); // Trạng thái
            ws.Column(6).Width = Math.Max(ws.Column(6).Width, 35); // Mô tả

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Xuất danh sách tài khoản ra định dạng Excel (.xlsx)
        /// </summary>
        public byte[] XuatDanhSachTaiKhoan(IEnumerable<TaiKhoan> danhSach, string? nguoiXuat = null, string? thongTinBoLoc = null)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachTaiKhoan");
            ws.ShowGridLines = true;

            // 1. Tiêu đề chính
            ws.Cell(1, 1).Value = "DANH SÁCH TÀI KHOẢN - TRUNG TÂM GIA SƯ";
            ws.Range("A1:G1").Merge();
            ws.Range("A1:G1").Style.Font.Bold = true;
            ws.Range("A1:G1").Style.Font.FontSize = 16;
            ws.Range("A1:G1").Style.Font.FontColor = XLColor.White;
            ws.Range("A1:G1").Style.Fill.BackgroundColor = TitleBg;
            ws.Range("A1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range("A1:G1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(1).Height = 35;

            // 2. Thông tin phụ
            var thoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            ws.Cell(2, 1).Value = $"Thời gian xuất: {thoiGian}" + (!string.IsNullOrWhiteSpace(nguoiXuat) ? $" | Người xuất: {nguoiXuat}" : "");
            ws.Range("A2:G2").Merge();
            ws.Range("A2:G2").Style.Font.Italic = true;
            ws.Range("A2:G2").Style.Font.FontSize = 10;
            ws.Range("A2:G2").Style.Font.FontColor = XLColor.FromHtml("#4B5563");
            ws.Range("A2:G2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 20;

            ws.Cell(3, 1).Value = $"Bộ lọc: {thongTinBoLoc ?? "Tất cả"} | Tổng số tài khoản: {danhSach.Count()}";
            ws.Range("A3:G3").Merge();
            ws.Range("A3:G3").Style.Font.FontSize = 10;
            ws.Range("A3:G3").Style.Font.FontColor = XLColor.FromHtml("#4B5563");
            ws.Range("A3:G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(3).Height = 20;

            // 3. Tiêu đề bảng (Header)
            string[] headers = { "STT", "Mã TK", "Tên đăng nhập", "Họ và tên", "Email", "Vai trò", "Trạng thái" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(5, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = PrimaryHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            ws.Row(5).Height = 26;

            // 4. Dữ liệu
            int row = 6;
            int stt = 1;
            foreach (var item in danhSach)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 2).Value = item.MaTaiKhoan;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 3).Value = item.TenDangNhap;
                ws.Cell(row, 3).Style.Font.Bold = true;

                ws.Cell(row, 4).Value = item.HoTen;
                ws.Cell(row, 5).Value = item.Email;

                var vaiTroText = item.VaiTro switch
                {
                    "Admin" => "Quản trị viên",
                    "GiaSu" => "Gia sư",
                    "HocVien" => "Học viên",
                    _ => item.VaiTro
                };
                ws.Cell(row, 6).Value = vaiTroText;
                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var trangThaiStr = item.TrangThai.Equals("HoatDong", StringComparison.OrdinalIgnoreCase) || item.TrangThai == "Hoạt động"
                    ? "Hoạt động"
                    : "Khóa";
                ws.Cell(row, 7).Value = trangThaiStr;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Định dạng viền và màu nền so le
                for (int col = 1; col <= headers.Length; col++)
                {
                    var c = ws.Cell(row, col);
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    c.Style.Border.OutsideBorderColor = BorderColor;
                    c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    if (stt % 2 == 1)
                    {
                        c.Style.Fill.BackgroundColor = ZebraBg;
                    }
                }

                ws.Row(row).Height = 22;
                row++;
            }

            // Đóng khung viền cho header
            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(5, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(5, col).Style.Border.OutsideBorderColor = BorderColor;
            }

            // 5. Dòng tổng kết
            ws.Cell(row, 1).Value = "Tổng cộng";
            ws.Range(row, 1, row, 2).Merge();
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            ws.Range(row, 1, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(row, 3).Value = $"{danhSach.Count()} tài khoản";
            ws.Cell(row, 3).Style.Font.Bold = true;

            for (int col = 1; col <= headers.Length; col++)
            {
                var c = ws.Cell(row, col);
                c.Style.Border.TopBorder = XLBorderStyleValues.Double;
                c.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = BorderColor;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            }
            ws.Row(row).Height = 24;

            // Tự động căn chỉnh độ rộng cột
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 8);  // STT
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 12); // Mã TK
            ws.Column(3).Width = Math.Max(ws.Column(3).Width, 20); // Tên đăng nhập
            ws.Column(4).Width = Math.Max(ws.Column(4).Width, 25); // Họ tên
            ws.Column(5).Width = Math.Max(ws.Column(5).Width, 28); // Email
            ws.Column(6).Width = Math.Max(ws.Column(6).Width, 18); // Vai trò
            ws.Column(7).Width = Math.Max(ws.Column(7).Width, 15); // Trạng thái

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
