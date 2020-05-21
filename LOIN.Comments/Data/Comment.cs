using LOIN.Viewer.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LOIN.Comments.Data
{
    public class Comment
    {
        public Comment()
        {

        }

        public Comment(SingleContext ctx): this()
        {
            ActorId = ctx.Actor?.Id;
            ActorName = ctx.Actor?.Name;

            BreakDownId = ctx.BreakdownItem?.Id;
            BreakDownName = ctx.BreakdownItem?.Name;

            ReasonId =   ctx.Reason?.Id;
            ReasonName = ctx.Reason?.Name;

            MilestoneId =   ctx.Milestone?.Id;
            MilestoneName = ctx.Milestone?.Name;
        }

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
        public string RequirementSetName { get; set; }

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
        public CommentType Type { get; set; }

        // resolution
        public string Resolution { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public bool IsEmpty() => 
            BadEnumeration == false && 
            WrongActor == false && 
            WrongBreakDown == false && 
            WrongMilestone == false && 
            WrongReason == false && 
            string.IsNullOrWhiteSpace(Content) && 
            string.IsNullOrWhiteSpace(Suggestion);
        
    }

    public enum ResolutionType
    {
        Unresolved,
        Accepted
    }

    public enum CommentType
    {
        Comment,
        NewRequirement
    }
}
