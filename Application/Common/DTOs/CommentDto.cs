namespace Application.Common.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Guid? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string Content { get; set; } = string.Empty;
    }
}