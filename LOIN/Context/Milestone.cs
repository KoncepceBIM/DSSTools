using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Context
{
    public class Milestone : AbstractLoinEntity<IIfcTask>
    {
        public static readonly Milestone Any = new Milestone(null, null);

        internal Milestone(IIfcTask task, Model model) : base(task, model)
        {
        }

        public string Name
        {
            get => Entity.Name;
            set => Entity.Name = value;
        }

        public string Description
        {
            get => Entity.Description;
            set => Entity.Description = value;
        }
    }
}
