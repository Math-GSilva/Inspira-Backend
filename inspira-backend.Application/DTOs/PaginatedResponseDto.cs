namespace inspira_backend.Application.DTOs
{
    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();

        public string? NextCursor { get; set; }

        public bool HasMoreItems { get; set; }
    }
}
