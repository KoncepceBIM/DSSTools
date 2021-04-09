using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using Xbim.Common.Step21;
using Xbim.Ifc4.Kernel;
using Xbim.MvdXml;

namespace LOIN
{
    public interface ILoinModel
    {
        IEnumerable<Actor> Actors { get; }
        IEnumerable<BreakdownItem> BreakdownStructure { get; }
        Xbim.Common.IModel Internal { get; }
        IEnumerable<Milestone> Milestones { get; }
        IEnumerable<Reason> Reasons { get; }
        IEnumerable<RequirementsSet> Requirements { get; }

        void Dispose();
        mvdXML GetMvd(XbimSchemaVersion schema, string languageCode, string name, string definition, string code, string classificationProperty, Func<IContextEntity, bool> contextFilter = null, Func<IfcPropertySetTemplate, bool> requirementSetFilter = null, Func<IfcPropertyTemplate, bool> requirementsFilter = null);
        IEnumerable<RequirementsSet> GetRequirements(params IContextEntity[] context);
    }
}