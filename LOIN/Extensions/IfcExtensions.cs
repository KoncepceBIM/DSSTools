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
        public static T SetId<T>(this T root, IfcGloballyUniqueId id) where T : IfcRoot
        {
            root.GlobalId = id;
            return root;
        }

        public static IfcSimplePropertyTemplate MakeEnumerated(this IfcSimplePropertyTemplate tmpl, string enumerationName, params string[] enumerationValues)
        {
            var i = tmpl.Model.Instances;
            tmpl.TemplateType = Xbim.Ifc4.Interfaces.IfcSimplePropertyTemplateTypeEnum.P_ENUMERATEDVALUE;
            tmpl.Enumerators = i.New<IfcPropertyEnumeration>(e =>
            {
                e.Name = enumerationName;
                e.EnumerationValues.AddRange(enumerationValues.Select(v => new IfcIdentifier(v)).Cast<IfcValue>());
            });
            return tmpl;
        }

        public static IfcSimplePropertyTemplate SetConstraint(this IfcSimplePropertyTemplate tmpl, IfcValue value, IfcBenchmarkEnum @operator)
        {
            var i = tmpl.Model.Instances;
            i.New<IfcRelAssociatesConstraint>(c =>
            {
                c.RelatedObjects.Add(tmpl);
                c.RelatingConstraint = i.New<IfcMetric>(m =>
                {
                    m.Name = tmpl.Name + " constraint";
                    m.ConstraintGrade = Xbim.Ifc4.Interfaces.IfcConstraintEnum.NOTDEFINED;
                    m.Benchmark = @operator;
                    m.DataValue = value;
                });
            });
            return tmpl;
        }

        public static void SetExample(this IfcSimplePropertyTemplate propTmpl, IfcPropertySetTemplate propSetTmpl, IfcValue example)
        {
            ArgsCheck(propTmpl, propSetTmpl);

            if (example is null) { throw new ArgumentNullException(nameof(example), "Example must be defined"); }

            var i = propTmpl.Model.Instances;
            var defines = propSetTmpl.Defines.FirstOrDefault();
            if (defines == null)
            {
                defines = i.New<IfcRelDefinesByTemplate>(rel =>
                {
                    rel.RelatingTemplate = propSetTmpl;
                    rel.GlobalId = Guid.NewGuid();
                });
            }

            defines.RelatedPropertySets.Add(i.New<IfcPropertySet>(ps =>
            {
                ps.Name = propSetTmpl.Name;
                ps.HasProperties.Add(i.New<IfcPropertySingleValue>(p =>
                {
                    p.Name = propTmpl.Name?.ToString();
                    p.NominalValue = example;
                }));
            }));
        }

        public static IEnumerable<IfcValue> GetExamples(this IIfcSimplePropertyTemplate propTmpl, IIfcPropertySetTemplate propSetTmpl)
        {
            ArgsCheck(propTmpl, propSetTmpl);

            return propSetTmpl.Defines
                .SelectMany(rel => rel.RelatedPropertySets)
                .OfType<IfcPropertySet>()
                .SelectMany(ps => ps.HasProperties.Where(p => string.Equals(p.Name, propTmpl.Name, StringComparison.OrdinalIgnoreCase)))
                .OfType<IfcPropertySingleValue>()
                .Select(p => p.NominalValue);
        }

        private static void ArgsCheck(IIfcSimplePropertyTemplate propertyTemplate, IIfcPropertySetTemplate propertySetTemplate)
        {
            if (propertyTemplate is null) { throw new ArgumentNullException(nameof(propertyTemplate), "Property template must be defined"); }
            if (string.IsNullOrWhiteSpace(propertyTemplate.Name)) { throw new ArgumentNullException(nameof(propertyTemplate), "Property template must have a name"); }

            if (propertySetTemplate is null) { throw new ArgumentNullException(nameof(propertySetTemplate), "Property set template must be defined"); }
            if (string.IsNullOrWhiteSpace(propertySetTemplate.Name)) { throw new ArgumentNullException(nameof(propertyTemplate), "Property set template must have a name"); }

            if (!propertySetTemplate.HasPropertyTemplates.Contains(propertyTemplate)) { throw new ArgumentNullException(nameof(propertyTemplate), "Property set template must contain the property template"); }

        }
    }
}
