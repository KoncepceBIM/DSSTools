using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
                requirements = requirements.Where(r => contextType.Any(c => c.IsContextFor(r)));
            }

            RequirementSets = requirements
                .Distinct()
                .ToList();

            Requirements = RequirementSets
                .SelectMany(rs => rs.RequirementSets).Distinct()
                .Select(rs => new RequirementSetView(rs))
                .ToList();

            OnPropertyChanged(nameof(Requirements));
        }

        public List<RequirementSetView> Requirements { get; private set; }
        public List<RequirementsSet> RequirementSets { get; private set; }

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
