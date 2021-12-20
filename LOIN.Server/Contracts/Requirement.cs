using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LOIN;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class Requirement : LoinItem
    {
        private const string cs = "cs";
        private const string en = "en";

        public string Units { get; set; }
        public string ValueType { get; set; }

        public string DataType { get; set; }
        public string DataTypeCS { get; set; }
        public string DataTypeEN { get; set; }

        public string SetName { get; set; }
        public string SetNameCS { get; set; }
        public string SetNameEN { get; set; }

        public string SetDescription { get; set; }
        public string SetDescriptionCS { get; set; }
        public string SetDescriptionEN { get; set; }

        public string NoteCS { get; set; }
        public string NoteEN { get; set; }

        public List<string> Enumeration { get; set; }

        public List<string> Examples { get; set; }

        public List<RequirementContext> Contexts { get; set; }

        public Requirement(ContextMap contextMap, IIfcPropertyTemplate property, IIfcPropertySetTemplate set) : this(property, set)
        {
            var id = property.GlobalId;
            if (contextMap.TryGetValue(id, out var contexts))
                Contexts = contexts;
        }

        public Requirement(IIfcPropertyTemplate property, IIfcPropertySetTemplate set) : base(property)
        {
            // set might be null, it the requirement is assigned directly, not through a set
            SetName = set?.Name;
            SetNameCS = set?.GetName(cs) ?? SetName;
            SetNameEN = set?.GetName(en) ?? SetName;

            SetDescription = set?.Description;
            SetDescriptionCS = set?.GetDescription(cs) ?? SetDescription;
            SetDescriptionEN = set?.GetDescription(en) ?? SetDescription;

            if (property is IIfcSimplePropertyTemplate simple)
            {
                NoteCS = simple.GetNote(cs);
                NoteEN = simple.GetNote(en);

                ValueType = simple.TemplateType?.ToString();

                if (simple.PrimaryMeasureType.HasValue)
                {
                    DataType = simple.PrimaryMeasureType.Value.ToString();
                    DataTypeCS = simple.GetDataTypeName(cs);
                    DataTypeEN = simple.GetDataTypeName(en);
                }

                if (simple.PrimaryUnit != null)
                    Units = Unit.GetSymbol(simple.PrimaryUnit);
                else
                    Units = string.Empty;

                if (simple.Enumerators != null)
                {
                    Enumeration = simple.Enumerators.EnumerationValues.Select(v => v.Value.ToString()).ToList();
                }

                Examples = simple.GetExamples(set).Select(v => v.ToString()).ToList();
            }
            else
                throw new NotSupportedException("Only simple property templates are supported.");
        }
    }

    public class RequirementContext
    {
        public IEnumerable<LoinItem> Actors { get; set; }
        public IEnumerable<LoinItem> Milestones { get; set; }
        public IEnumerable<LoinItem> BreakdownItems { get; set; }
        public IEnumerable<LoinItem> Reasons { get; set; }
    }

    public class ContextMap : Dictionary<string, List<RequirementContext>>
    {
        public ContextMap(ILoinModel model)
        {
            foreach (var loin in model.Requirements)
            {
                var ctx = new RequirementContext
                {
                    Actors = loin.Actors.Select(a => new LoinItem(a)).Distinct(LoinItemEqualityComparer.Comparer).ToList(),
                    BreakdownItems = loin.BreakedownItems.Select(i => new LoinItem(i)).Distinct(LoinItemEqualityComparer.Comparer).ToList(),
                    Milestones = loin.Milestones.Select(m => new LoinItem(m)).Distinct(LoinItemEqualityComparer.Comparer).ToList(),
                    Reasons = loin.Reasons.Select(r => new LoinItem(r)).Distinct(LoinItemEqualityComparer.Comparer).ToList()
                };
                foreach (var requirement in loin.Requirements)
                {
                    var id = requirement.GlobalId;
                    if (TryGetValue(id, out var contexts))
                    {
                        contexts.Add(ctx);
                    }
                    else
                    {
                        Add(id, new List<RequirementContext> { ctx });
                    }

                }
            }
        }
    }

    internal class LoinItemEqualityComparer : IEqualityComparer<LoinItem>
    {
        public static readonly LoinItemEqualityComparer Comparer = new LoinItemEqualityComparer();

        public bool Equals([AllowNull] LoinItem x, [AllowNull] LoinItem y)
        {
            return string.Equals(x?.UUID, y?.UUID);
        }

        public int GetHashCode([DisallowNull] LoinItem obj)
        {
            return obj.UUID?.GetHashCode() ?? 0;
        }
    }
}
