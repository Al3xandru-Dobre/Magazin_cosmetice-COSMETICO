namespace GlowUp.API.DTOs.Common;

/// <summary>
/// Wrapper pentru rezultate paginate.
/// Clientul are nevoie si de metadate (cate pagini exista), nu doar de lista,
/// altfel nu poate randa controalele de paginare.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
