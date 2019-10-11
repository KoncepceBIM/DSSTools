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

        public RequirementView(IfcSimplePropertyTemplate property)
        {
            this.property = property;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Name => property.Name;

        public string Description => property.Description;

        public string ValueType => property.PrimaryMeasureType;
    }
}
