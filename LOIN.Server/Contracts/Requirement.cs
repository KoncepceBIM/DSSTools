using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LOIN;
using LOIN.Context;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;

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

        public RequirementContext Context { get; set; }

        internal Requirement(ContextMap contextMap, IIfcPropertyTemplate property, IIfcPropertySetTemplate set) : this(property, set)
        {
            var id = property.GlobalId;
            if (contextMap != null && contextMap.TryGetValue(id, out var context))
            {
                Context = context;
            }
        }

        internal  Requirement(IIfcPropertyTemplate property, IIfcPropertySetTemplate set) : base(property)
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
        public IEnumerable<LoinItem> Reasons { get; set; }
    }

    internal class ContextMap : Dictionary<string, RequirementContext>
    {
        public static ContextMap ForModel(ILoinModel model)
        {
            return model.Internal.GetCache("ContextMap", () => new ContextMap(model));
        }


        private ContextMap(ILoinModel model)
        {
            foreach (var loin in model.Requirements)
            {
                var ctx = new RequirementContext
                {
                    Actors = new HashSet<LoinItem>(loin.Actors.Select(a => new LoinItem(a)), LoinItemEqualityComparer.Comparer),
                    Milestones = new HashSet<LoinItem>(loin.Milestones.Select(m => new LoinItem(m)), LoinItemEqualityComparer.Comparer),
                    Reasons = new HashSet<LoinItem>(loin.Reasons.Select(r => new LoinItem(r)), LoinItemEqualityComparer.Comparer)
                };
                foreach (var requirement in loin.Requirements)
                {
                    var id = requirement.GlobalId;
                    if (TryGetValue(id, out var context))
                    {
                        AddContextItems(context.Actors, ctx.Actors);
                        AddContextItems(context.Milestones, ctx.Milestones);
                        AddContextItems(context.Reasons, ctx.Reasons);
                    }
                    else
                    {
                        Add(id, ctx );
                    }

                }
            }
        }

        private static void AddContextItems(IEnumerable<LoinItem> target, IEnumerable<LoinItem> source)
        {
            var inner = (HashSet<LoinItem>)target;
            foreach (var item in source)
            {
                inner.Add(item);
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
