using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer.Views
{
    public class ContextSelector: INotifyPropertyChanged
    {
        public ContextSelector(Model model)
        {
            this.model = model;
            Update();
        }

        private readonly HashSet<IContextEntity> _context = new HashSet<IContextEntity>();
        private readonly Model model;

        public IEnumerable<IContextEntity> Context 
        {
            get => _context;
        }

        public void Add(IContextEntity entity)
        {
            _context.Add(entity);
            OnPropertyChanged(nameof(Context));
            Update();
        }

        public void Remove(IContextEntity entity)
        {
            _context.Remove(entity);
            OnPropertyChanged(nameof(Context));
            Update();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Update()
        {
            var contextTypes = _context.GroupBy(c => c.GetType());
            var requirements = model.Requirements;
            foreach (var contextType in contextTypes)
            {
                var context = new HashSet<IContextEntity>(contextType);
                if (IncludeUpperBreakdown && contextType.Key == typeof(BreakedownItem))
                {
                    foreach (var item in contextType.OfType<BreakedownItem>().Where(i => i.Parent != null))
                    {
                        context.Add(item.Parent);
                    }
                }

                // continuous filtering refinement
                requirements = requirements.Where(r => contextType.Any(c => c.IsContextFor(r)));
            }

            RequirementSets = requirements.SelectMany(rs => rs.RequirementSets).Distinct()
                .Select(rs => new RequirementSetView(rs))
                .ToList();

            // requirements from sets
            Requirements = RequirementSets.SelectMany(rs => rs.Requirements).ToList();

            // requirements used directly without property set
            var map = new Dictionary<int, RequirementSetView>();
            var check = new HashSet<int>(Requirements.Select(r => r.PropertyTemplate.EntityLabel));
            var directRequirements = requirements
                .SelectMany(rs => rs.Requirements)
                .Where(r => check.Add(r.EntityLabel) && r is IfcSimplePropertyTemplate)
                .Select(r => new RequirementView((IfcSimplePropertyTemplate)r, map));

            // add direct properties and their parent property sets (if any)
            Requirements.AddRange(directRequirements);
            RequirementSets.AddRange(map.Values);

            // keep information about the primary filter
            LevelsOfInformationNeeded = requirements.ToList();

            // notify about the change of both
            OnPropertyChanged(nameof(Requirements));
            OnPropertyChanged(nameof(RequirementSets));
            OnPropertyChanged(nameof(LevelsOfInformationNeeded));
        }

        public List<RequirementView> Requirements { get; private set; }
        public List<RequirementSetView> RequirementSets { get; private set; }

        public List<RequirementsSet> LevelsOfInformationNeeded { get; private set; }

        private bool _includeUpperBreakdown = true;
        public bool IncludeUpperBreakdown
        {
            get => _includeUpperBreakdown;
            set
            {
                _includeUpperBreakdown = value;
                OnPropertyChanged(nameof(IncludeUpperBreakdown));
            }
        }
    }
}
