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
        private string lang;


        public RequirementSetView(IfcPropertySetTemplate psetTemplate)
        {
            this.PsetTemplate = psetTemplate;
            Requirements = psetTemplate.HasPropertyTemplates
                .OfType<IfcSimplePropertyTemplate>()
                .Select(p => new RequirementView(p))
                .ToList();


            Language.PropertyChanged += (_, p) => {
                if (p.PropertyName != nameof(Language.Lang))
                    return;
                lang = Language.Lang;
            };
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

        public string Name2 => PsetTemplate.GetName(lang) ?? Name;
        public string Description2 => PsetTemplate.GetDescription(lang) ?? Description;

        public List<RequirementView> Requirements { get; }

        public IfcPropertySetTemplate PsetTemplate { get; }
    }
}
