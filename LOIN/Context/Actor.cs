using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Context
{
    public class Actor : AbstractLoinEntity<IIfcActor>
    {
        public static readonly Actor Any = new Actor(null, null);

        internal Actor(IIfcActor actor, Model model) : base(actor, model)
        {
        }

        // public string Name { get; set; }
    }
}
