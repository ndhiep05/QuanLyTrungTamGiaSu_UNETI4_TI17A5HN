
namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models
{
    public class PhanTrangViewModel
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages =>
            TotalItems == 0
                ? 0
                : (int)Math.Ceiling((double)TotalItems / PageSize);

        public int TuMuc =>
            TotalItems == 0
                ? 0
                : (Page - 1) * PageSize + 1;

        public int DenMuc =>
            Math.Min(Page * PageSize, TotalItems);
    }
}