using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer.Views
{
    public class RequirementSetView: INotifyPropertyChanged
    {
        private readonly IfcPropertySetTemplate psetTemplate;

        public RequirementSetView(IfcPropertySetTemplate psetTemplate)
        {
            this.psetTemplate = psetTemplate;
            Requirements = psetTemplate.HasPropertyTemplates
                .OfType<IfcSimplePropertyTemplate>()
                .Select(p => new RequirementView(p))
                .ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isSelected;

        public bool IsSelected {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string Name => PsetTemplate.Name;
        public string Description => PsetTemplate.Description;

        public List<RequirementView> Requirements { get; }

        public IfcPropertySetTemplate PsetTemplate => psetTemplate;
    }
}
