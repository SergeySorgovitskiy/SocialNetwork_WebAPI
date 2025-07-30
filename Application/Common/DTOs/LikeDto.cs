using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DTOs
{
    public class LikeDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class LikeCreateDto
    {
        public Guid PostId { get; set; }
    }
    public class LikeDeleteDto
    {
        public Guid PostId { get; set; }
    }
}
