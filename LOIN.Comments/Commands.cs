using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LOIN.Comments
{
    public static class Commands
    {
        public static RoutedCommand OpenFile = new RoutedCommand();
        public static RoutedCommand SaveFile = new RoutedCommand();
        public static RoutedCommand SaveFileAs = new RoutedCommand();
        public static RoutedCommand ExportFile = new RoutedCommand();
        public static RoutedCommand Model = new RoutedCommand();
        public static RoutedCommand Load = new RoutedCommand();

        static Commands()
        {
            OpenFile.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            SaveFile.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            SaveFileAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            ExportFile.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            Model.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
            Load.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
        }
    }
}
