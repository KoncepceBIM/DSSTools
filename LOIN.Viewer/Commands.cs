﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LOIN.Viewer
{
    public static class Commands
    {
        public static RoutedCommand OpenFile = new RoutedCommand();
        public static RoutedCommand SaveFile = new RoutedCommand();
        public static RoutedCommand ExportFile = new RoutedCommand();
        public static RoutedCommand Model = new RoutedCommand();

        static Commands()
        {
            OpenFile.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            SaveFile.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            ExportFile.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            Model.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }
    }
}
