using Domain.Entities;

namespace Domain.Common;

public class NewsFeedResult
{
    public IEnumerable<Post> Posts { get; set; } = new List<Post>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}