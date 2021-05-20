using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.ConstraintResource;
using Xbim.Ifc4.ControlExtension;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.UtilityResource;
using System.Linq;

namespace LOIN
{
    public static class IfcExtensions
    {
        public static T SetId<T>(this T root, IfcGloballyUniqueId id) where T: IfcRoot
        {
            root.GlobalId = id;
            return root;
        }

        public static IfcSimplePropertyTemplate MakeEnumerated(this IfcSimplePropertyTemplate tmpl, string enumerationName, params string[] enumerationValues)
        {
            var i = tmpl.Model.Instances;
            tmpl.TemplateType = Xbim.Ifc4.Interfaces.IfcSimplePropertyTemplateTypeEnum.P_ENUMERATEDVALUE;
            tmpl.Enumerators = i.New<IfcPropertyEnumeration>(e => {
                e.Name = enumerationName;
                e.EnumerationValues.AddRange(enumerationValues.Select(v => new IfcIdentifier(v)).Cast<IfcValue>());
            });
            return tmpl;
        }

        public static IfcSimplePropertyTemplate SetConstraint(this IfcSimplePropertyTemplate tmpl, IfcValue value, IfcBenchmarkEnum @operator)
        {
            var i = tmpl.Model.Instances;
            i.New<IfcRelAssociatesConstraint>(c => {
                c.RelatedObjects.Add(tmpl);
                c.RelatingConstraint = i.New<IfcMetric>(m => {
                    m.Name = tmpl.Name + " constraint";
                    m.ConstraintGrade = Xbim.Ifc4.Interfaces.IfcConstraintEnum.NOTDEFINED;
                    m.Benchmark = @operator;
                    m.DataValue = value;
                });
            });
            return tmpl;
        }

    }
}
