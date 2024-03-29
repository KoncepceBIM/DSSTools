﻿using System;
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
                .Select(p => new RequirementView(p, this))
                .ToList();


            lang = Language.Lang;
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

                // forward to all requirements in the set
                Requirements.ForEach(r => r.IsSelected = IsSelected);
            }
        }

        internal void ShallowSelect(bool value)
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }

        public string Name => PsetTemplate.Name;
        public string Description => PsetTemplate.Description;

        public string Name2 => PsetTemplate.GetName(lang) ?? Name;
        public string Description2 => PsetTemplate.GetDescription(lang) ?? Description;

        public string NameCS => PsetTemplate.GetName("cs") ?? Name;
        public string NameEN => PsetTemplate.GetName("en") ?? Name;

        public string DescriptionCS => PsetTemplate.GetDescription("cs") ?? Name;
        public string DescriptionEN => PsetTemplate.GetDescription("en") ?? Name;

        public List<RequirementView> Requirements { get; }

        public IfcPropertySetTemplate PsetTemplate { get; }
    }
}
