using LOIN.Viewer.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LOIN.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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



        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { 
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "IFC File|*.ifc|IFC XML File|*.ifcxml",
                FilterIndex = 0,
                Title = "Select LOIN IFC File"
            };
            if (dlg.ShowDialog() != true)
                return;

            var path = dlg.FileName;
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
            var model = LOIN.Model.Open(path);

            ContextSelector = new ContextSelector(model);

            if (!model.Requirements.Any())
                MessageBox.Show(this, "This file doesn't contain any requirements.", "Not a LOIN", MessageBoxButton.OK, MessageBoxImage.Warning);

            DataContext = model;

            // breakedown structure
            BreakedownItems = model.BreakdownStructure.Where(bs => bs.Parent == null)
                .Select(i => new BreakedownItemView(i, ContextSelector))
                .ToList();

            Actors = model.Actors.Select(a => new ActorView(a, ContextSelector)).ToList();
            Milestones = model.Milestones.Select(a => new MilestoneView(a, ContextSelector)).ToList();
            Reasons = model.Reasons.Select(a => new ReasonView(a, ContextSelector)).ToList();

        }
    }
}
