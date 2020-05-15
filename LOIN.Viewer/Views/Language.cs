using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LOIN.Viewer.Views
{
    public static class Language
    {
        public static event PropertyChangedEventHandler PropertyChanged;
        private static void OnPropertyChanged(string name) => PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));

        private static string _lang = "en";
        public static string Lang {
            get => _lang;
            set
            {
                _lang = value;
                OnPropertyChanged(nameof(Lang));
            }
        }
    }
}
