using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer.Views
{
    public class RequirementView : INotifyPropertyChanged
    {
        private readonly IfcSimplePropertyTemplate property;
        private string lang;


        public RequirementView(IfcSimplePropertyTemplate property)
        {
            this.property = property;


            Language.PropertyChanged += (_, p) => {
                if (p.PropertyName != nameof(Language.Lang))
                    return;
                lang = Language.Lang;
            };
        }

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
            }
        }

        public string Name => property.Name;

        public string Description => property.Description;

        public string Name2 => property.GetName(lang) ?? Name;
        public string Description2 => property.GetDescription(lang) ?? Description;

        public string ValueType => property.PrimaryMeasureType;
    }
}
