namespace Application.Common.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public required string AuthorUserName { get; set; }
        public Guid? ParentCommentId { get; set; }
        public int NestingLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public required string Content { get; set; }
        public Guid PostId { get; set; }
        public Guid? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public required string Content { get; set; }
    }
}