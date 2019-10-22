using LOIN.Context;
using LOIN.Viewer.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using Xbim.Ifc4.Kernel;

namespace LOIN.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model _model;

        public MainWindow()
        {
            InitializeComponent();
        }



        public ContextSelector ContextSelector
        {
            get { return (ContextSelector)GetValue(ContextSelectorProperty); }
            set { SetValue(ContextSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextSelectorProperty =
            DependencyProperty.Register("ContextSelector", typeof(ContextSelector), typeof(MainWindow), new PropertyMetadata(null));




        public List<BreakedownItemView> BreakedownItems
        {
            get { return (List<BreakedownItemView>)GetValue(BreakedownItemsProperty); }
            set { SetValue(BreakedownItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BreakedownItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BreakedownItemsProperty =
            DependencyProperty.Register("BreakedownItems", typeof(List<BreakedownItemView>), typeof(MainWindow), new PropertyMetadata(null));




        public List<ActorView> Actors
        {
            get { return (List<ActorView>)GetValue(ActorsProperty); }
            set { SetValue(ActorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Actors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActorsProperty =
            DependencyProperty.Register("Actors", typeof(List<ActorView>), typeof(MainWindow), new PropertyMetadata(null));



        public List<MilestoneView> Milestones
            {
            get { return (List<MilestoneView>)GetValue(MilestonesProperty); }
            set { SetValue(MilestonesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Milestones.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MilestonesProperty =
            DependencyProperty.Register("Milestones", typeof(List<MilestoneView>), typeof(MainWindow), new PropertyMetadata(null));




        public List<ReasonView> Reasons
        {
            get { return (List<ReasonView>)GetValue(ReasonsProperty); }
            set { SetValue(ReasonsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Reasons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReasonsProperty =
            DependencyProperty.Register("Reasons", typeof(List<ReasonView>), typeof(MainWindow), new PropertyMetadata(null));



        private void SelectionToMVD_Click(object sender, RoutedEventArgs e)
        {
            var breakedown = new HashSet<IContextEntity>(ContextSelector.Context.OfType<BreakedownItem>());
            var milestones = new HashSet<IContextEntity>(ContextSelector.Context.OfType<Milestone>());
            var reasons = new HashSet<IContextEntity>(ContextSelector.Context.OfType<Reason>());
            var actors = new HashSet<IContextEntity>(ContextSelector.Context.OfType<Actor>());

            var psets = ContextSelector.Requirements.Where(r => r.IsSelected).Select(r => r.PsetTemplate);
            if (!psets.Any())
                psets = ContextSelector.Requirements.Select(r => r.PsetTemplate);

            var requirements = new HashSet<IfcPropertySetTemplate>(psets);

            // filtered export
            ExportToMVD(c => {
                if (breakedown.Count > 0 && c is BreakedownItem i)
                    return breakedown.Contains(c);
                if (milestones.Count > 0 && c is Milestone m)
                    return milestones.Contains(m);
                if (reasons.Count > 0 && c is Reason r)
                    return reasons.Contains(r);
                if (actors.Count > 0 && c is Actor a)
                    return actors.Contains(a);
                return true;

            }, p => requirements.Contains(p));
        }

        private void ExportToMVD_Click(object sender, RoutedEventArgs e)
        {
            ExportToMVD();
        }

        private void ExportToMVD(Func<IContextEntity, bool> contextFilter = null,
            Func<IfcPropertySetTemplate, bool> requirementsFilter = null)
        {
            if (_model == null)
            {
                MessageBox.Show(this, "No model opened", "No model", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var mvd = _model.GetMvd("cs", "LOIN", "LOIN requirements stored as MVD", "LOIN", contextFilter, requirementsFilter);

            var dlg = new SaveFileDialog
            {
                Filter = "MVD XML|*.mvdXML",
                AddExtension = true,
                FilterIndex = 0,
                FileName = App.Settings.LastMVD,
                InitialDirectory = System.IO.Path.GetDirectoryName(App.Settings.LastMVD),
                Title = "Create MVD XML..."
            };
            if (dlg.ShowDialog() != true)
                return;


            var path = dlg.FileName;
            App.Settings.LastMVD = path;
            if (string.IsNullOrWhiteSpace(path))
                return;

            using var stream = File.Create(path);
            var w = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "  " });
            mvd.Serialize(w);
            stream.Close();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "IFC File|*.ifc|IFC XML File|*.ifcxml",
                FilterIndex = 0,
                Title = "Select LOIN IFC File",
                FileName = App.Settings.LastIFC,
                InitialDirectory = System.IO.Path.GetDirectoryName(App.Settings.LastIFC)
            };
            if (dlg.ShowDialog() != true)
                return;

            var path = dlg.FileName;
            App.Settings.LastIFC = path;
            OpenFile(path);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var startFile = (Application.Current as App).StartupFile;
            if (!string.IsNullOrWhiteSpace(startFile))
                OpenFile(startFile);
        }

        private void OpenFile(string path)
        {
            _model = LOIN.Model.Open(path);

            ContextSelector = new ContextSelector(_model);

            if (!_model.Requirements.Any())
                MessageBox.Show(this, "This file doesn't contain any requirements.", "Not a LOIN", MessageBoxButton.OK, MessageBoxImage.Warning);

            DataContext = _model;

            // breakedown structure
            BreakedownItems = _model.BreakdownStructure.Where(bs => bs.Parent == null)
                .Select(i => new BreakedownItemView(i, ContextSelector))
                .ToList();

            Actors = _model.Actors.Select(a => new ActorView(a, ContextSelector)).ToList();
            Milestones = _model.Milestones.Select(a => new MilestoneView(a, ContextSelector)).ToList();
            Reasons = _model.Reasons.Select(a => new ReasonView(a, ContextSelector)).ToList();

        }
    }
}
