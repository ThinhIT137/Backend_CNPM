namespace backend.DTO
{
    public class SearchFilterRequest
    {
        public string? Type { get; set; }
        public string? Keyword { get; set; } // Dùng cho Smart Search (Tên, Title)
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Category { get; set; } // Phân loại Tour (Ghép đoàn, Riêng tư)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
