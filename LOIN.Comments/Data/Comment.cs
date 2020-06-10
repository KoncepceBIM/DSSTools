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

        public static readonly Comment Empty = new Comment();

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

        // wrong context report
        public bool WrongBreakDown { get; set; }
        public bool WrongActor { get; set; }
        public bool WrongReason { get; set; }
        public bool WrongMilestone { get; set; }

        // other suggestions
        public bool BadEnumeration { get; set; }
        public bool ShouldBeOptional { get; set; }
        public bool WrongPropertySet { get; set; }

        // state
        public CommentType Type { get; set; }

        // resolution
        public string Resolution { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public bool IsEmpty() => 
            WrongActor == Empty.WrongActor&& 
            WrongBreakDown == Empty.WrongBreakDown && 
            WrongMilestone == Empty.WrongMilestone && 
            WrongReason == Empty.WrongReason && 

            BadEnumeration == Empty.BadEnumeration && 
            ShouldBeOptional == Empty.ShouldBeOptional &&
            WrongPropertySet == Empty.WrongPropertySet &&

            ResolutionType == Empty.ResolutionType &&
            string.IsNullOrWhiteSpace(Resolution) &&
            
            string.IsNullOrWhiteSpace(Content) && 
            string.IsNullOrWhiteSpace(Suggestion);

    }

    // zadáno / k projednání / odsouhlaseno OTO / odsouhlaseno TK / zamítnuto
    public enum ResolutionType
    {
        // default
        [Description("Zadáno")]
        Open = 0,

        [Description("K projednání")]
        ToBeDiscussed,

        [Description("Odsouhlaseno OTO")]
        AcceptedOTO,
         
        [Description("Odsouhlaseno TK")]
        AcceptedTK,

        [Description("Zamítnuto")]
        Rejected
    }

    public enum CommentType
    {
        // default
        [Description("Komentář")]
        Comment = 0,

        [Description("Nový požadavek")]
        NewRequirement
    }
}
