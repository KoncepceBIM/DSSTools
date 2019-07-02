using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class Milestone : RelatedLoinEntity<IIfcRelAssignsToProcess, IIfcTask>
    {
        public Milestone(IIfcRelAssignsToProcess relationship) : base(relationship)
        {
        }

        protected override Func<IIfcRelAssignsToProcess, IIfcTask> Accessor =>
            r => r.RelatingProcess as IIfcTask;

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
