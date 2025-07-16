namespace Domain.Common;

public class NewsFeedFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? Hashtag { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeReposts { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
}