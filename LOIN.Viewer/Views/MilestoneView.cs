using System;
using System.Collections.Generic;
using System.Text;
using LOIN.Context;

namespace LOIN.Viewer.Views
{
    public class MilestoneView : ContextView<Milestone>
    {
        public MilestoneView(Milestone milestone, ContextSelector selector) : base(milestone, selector)
        {
        }
    }
}
