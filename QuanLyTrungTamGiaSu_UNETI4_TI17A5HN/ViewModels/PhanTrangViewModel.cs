// Họ và tên: <Nguyễn Đình Hiệp>
// Mã sinh viên: 23103100230
// Nội dung thực hiện: ViewModel dùng chung cho giao diện (SV1).

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.ViewModels;

public class PhanTrangViewModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
    public int TuMuc => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int DenMuc => Math.Min(Page * PageSize, TotalItems);

    public int CurrentPage { get; internal set; }
    public string ActionName { get; internal set; }
}
