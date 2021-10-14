using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer.Views
{
    public class RequirementView : INotifyPropertyChanged
    {
        private string lang;

        private static RequirementSetView GetOrCreate(IfcSimplePropertyTemplate property, Dictionary<int, RequirementSetView> map)
        {
            // enumerate inverse property. This is potentially expensive
            var pset = property.PartOfPsetTemplate.FirstOrDefault();
            if (pset == null)
                return null;

            if (map.TryGetValue(pset.EntityLabel, out RequirementSetView view))
            {
                return view;
            }

            view = new RequirementSetView(pset);
            view.Requirements.Clear();
            map.Add(pset.EntityLabel, view);
            return view;
        }

        public RequirementView(IfcSimplePropertyTemplate property, Dictionary<int, RequirementSetView> map) :
            this(property, GetOrCreate(property, map), true)
        { }

        public RequirementView(IfcSimplePropertyTemplate property, RequirementSetView requirementSet, bool addSelf = false)
        {
            PropertyTemplate = property;
            Parent = requirementSet;

            if (addSelf)
                Parent.Requirements.Add(this);

            lang = Language.Lang;
            Language.PropertyChanged += (_, p) =>
            {
                if (p.PropertyName != nameof(Language.Lang))
                    return;
                lang = Language.Lang;
            };
        }

        public string Id => PropertyTemplate.GlobalId;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                if (value && Parent != null)
                    Parent.ShallowSelect(true);
            }
        }

        public string Name => PropertyTemplate.Name;

        public string Description => PropertyTemplate.Description;

        public string Name2 => PropertyTemplate.GetName(lang) ?? Name;
        public string Description2 => PropertyTemplate.GetDescription(lang) ?? Description;

        public string NameCS => PropertyTemplate.GetName("cs") ?? Name;
        public string NameEN => PropertyTemplate.GetName("en") ?? Name;

        public string DescriptionCS => PropertyTemplate.GetDescription("cs") ?? Name;
        public string DescriptionEN => PropertyTemplate.GetDescription("en") ?? Name;


        public string Example => string.Join("\r\n", PropertyTemplate.GetExamples(Parent.PsetTemplate));

        public string ValueType => PropertyTemplate.PrimaryMeasureType;

        public IReadOnlyList<string> Enumeration => PropertyTemplate.Enumerators?.EnumerationValues.Select(e => e.ToString()).ToArray() ?? Array.Empty<string>();

        public bool HasEnumeration => Enumeration.Any();

        public RequirementSetView Parent { get; }

        public IfcSimplePropertyTemplate PropertyTemplate { get; }
    }
}
