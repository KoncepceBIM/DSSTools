using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace LOIN.Viewer.Views
{
    public class ActorView : ContextView<Actor>
    {
        public ActorView(Actor actor, ContextSelector selector) : base(actor, selector)
        {
        }


    }
}
