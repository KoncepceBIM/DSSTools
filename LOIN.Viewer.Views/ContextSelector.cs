using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xbim.Common.ExpressValidation;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer.Views
{
    public class ContextSelector : INotifyPropertyChanged
    {
        public ContextSelector(Model model, bool oneOfType) : this(model)
        {
            OneOfType = oneOfType;
        }

        public ContextSelector(Model model)
        {
            this.model = model;
            Update();
        }

        private readonly HashSet<IContextEntity> _context = new HashSet<IContextEntity>();
        private readonly Dictionary<IContextEntity, List<ContextView>> _views = new Dictionary<IContextEntity, List<Views.ContextView>>();
        private readonly Model model;

        public IEnumerable<IContextEntity> Context
        {
            get => _context;
        }

        public void Add(ContextView view)
        {
            if (OneOfType)
            {
                var contextType = view.Entity.GetType();
                var existing = _context.Where(c => c.GetType() == contextType).ToList();
                if (existing.Any())
                {
                    foreach (var item in existing)
                    {
                        _context.Remove(item);
                        if (_views.TryGetValue(item, out List<ContextView> views))
                        { 
                            views.ForEach(v => v.Unselect());
                            views.Clear();
                        }
                    }
                }
            }

            _context.Add(view.Entity);
            if (_views.TryGetValue(view.Entity, out List<ContextView> cache))
            {
                cache.Add(view);
            }
            else
            {
                _views.Add(view.Entity, new List<ContextView> { view });
            }
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
                if (IncludeUpperBreakdown && contextType.Key == typeof(BreakdownItem))
                {
                    foreach (var item in contextType.OfType<BreakdownItem>().Where(i => i.Parent != null))
                        foreach (var parent in item.Parents)
                            _context.Add(parent);
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

        public bool OneOfType { get; } = false;
    }
}
