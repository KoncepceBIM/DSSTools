using System;
using System.Collections.Generic;
using System.Text;

namespace LOIN.Comments.Data
{
    public class Comment
    {
        // identity
        public string Author { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // context
        public string BreakDownId { get; set; }
        public string BreakDownName { get; set; }

        public string ActorId { get; set; }
        public string ActorName { get; set; }

        public string ReasonId { get; set; }
        public string ReasonName { get; set; }

        public string MilestoneId { get; set; }
        public string MilestoneName { get; set; }

        // requirement identity
        public string RequirementId { get; set; }
        public string RequirementName { get; set; }

        // free text content
        public string Content { get; set; }
        public string Suggestion { get; set; }
        public bool BadEnumeration { get; set; }

        // wrong context report
        public bool WrongBreakDown { get; set; }
        public bool WrongActor { get; set; }
        public bool WrongReason { get; set; }
        public bool WrongMilestone { get; set; }

        // state
        public CommentState State { get; set; }
    }

    public enum CommentState
    {
        Open,
        Closed
    }
}
