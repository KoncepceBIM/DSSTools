using System;
using System.Collections.Generic;
using System.Text;
using LOIN.Context;

namespace LOIN.Viewer.Views
{
    public class ReasonView : ContextView<Reason>
    {
        public ReasonView(Reason reason, ContextSelector selector) : base(reason, selector)
        {
        }
    }
}
