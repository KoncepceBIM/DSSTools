using CsvHelper;
using LOIN.BCF;
using LOIN.Comments.Data;
using LOIN.Context;
using LOIN.Requirements;
using LOIN.Validation;
using LOIN.Viewer.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using Xbim.Common;
using Xbim.Common.Model;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.IO.Memory;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Comment = LOIN.Comments.Data.Comment;

namespace LOIN.Comments
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

        public string CurrentFile { get; set; }

        public ContextSelector ContextSelector
        {
            get { return (ContextSelector)GetValue(ContextSelectorProperty); }
            set { SetValue(ContextSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextSelectorProperty =
            DependencyProperty.Register("ContextSelector", typeof(ContextSelector), typeof(MainWindow), new PropertyMetadata(null));




        public List<BreakdownItemView> BreakedownItems
        {
            get { return (List<BreakdownItemView>)GetValue(BreakedownItemsProperty); }
            set { SetValue(BreakedownItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BreakedownItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BreakedownItemsProperty =
            DependencyProperty.Register("BreakedownItems", typeof(List<BreakdownItemView>), typeof(MainWindow), new PropertyMetadata(null));




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




        public RequirementView CurrentRequirement
        {
            get { return (RequirementView)GetValue(CurrentRequirementProperty); }
            set { SetValue(CurrentRequirementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentRequirement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRequirementProperty =
            DependencyProperty.Register("CurrentRequirement", typeof(RequirementView), typeof(MainWindow), new PropertyMetadata(null, (d, e) =>
            {
                if (!(d is MainWindow self) || !(e.NewValue is RequirementView rv))
                    return;



                var actor = self.Actors.FirstOrDefault(a => a.IsSelected);
                var reason = self.Reasons.FirstOrDefault(r => r.IsSelected);
                var milestone = self.Milestones.FirstOrDefault(m => m.IsSelected);
                var breakdown = self.BreakedownItems.SelectMany(i => i.GetDeepSelected()).FirstOrDefault();

                self.CurrentComment = self.Comments.FirstOrDefault(c =>
                    c.RequirementId == rv.Id &&
                    c.ActorId == actor?.Entity.Id &&
                    c.MilestoneId == milestone?.Entity.Id &&
                    c.ReasonId == reason?.Entity.Id &&
                    c.BreakDownId == breakdown?.Entity.Id
                    ) ?? new Comment
                    {
                        Author = self.User,
                        ActorId = actor?.Entity.Id,
                        ActorName = actor?.Name,
                        MilestoneId = milestone?.Entity.Id,
                        MilestoneName = milestone?.Name,
                        ReasonId = reason?.Entity.Id,
                        ReasonName = reason?.Name,
                        BreakDownId = breakdown?.Entity.Id,
                        BreakDownName = breakdown?.Name,
                        RequirementId = rv.Id,
                        RequirementName = rv.Name,
                        State = CommentState.Open
                    };
            }));



        public Comment CurrentComment
        {
            get { return (Comment)GetValue(CurrentCommentProperty); }
            set { SetValue(CurrentCommentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentCommentProperty =
            DependencyProperty.Register("CurrentComment", typeof(Comment), typeof(MainWindow), new PropertyMetadata(null));



        public ObservableCollection<Comment> Comments
        {
            get { return (ObservableCollection<Comment>)GetValue(CommentsProperty); }
            set { SetValue(CommentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentsProperty =
            DependencyProperty.Register("Comments", typeof(ObservableCollection<Comment>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<Comment>()));




        public string User
        {
            get { return (string)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for User.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(string), typeof(MainWindow), new PropertyMetadata(null, (d, e) =>
            {
                if (!(d is MainWindow self))
                    return;

                var user = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(user))
                    App.Settings.User = user;
            }));



        private void SaveComments_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LoadComments_Click(object sender, RoutedEventArgs e)
        {
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
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

            CurrentFile = dlg.FileName;
            App.Settings.LastIFC = CurrentFile;
            OpenFile(CurrentFile);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var startFile = (Application.Current as App).StartupFile;
            if (!string.IsNullOrWhiteSpace(startFile))
                OpenFile(startFile);

            if (!string.IsNullOrWhiteSpace(App.Settings.User))
                User = App.Settings.User;
        }

        private void OpenFile(string path)
        {
            CurrentFile = path;
            Title = $"Připomínky k DSS: {path}";
            _model = LOIN.Model.Open(path);

            ContextSelector = new ContextSelector(_model, true);

            if (!_model.Requirements.Any())
                MessageBox.Show(this, "This file doesn't contain any requirements.", "Not a LOIN", MessageBoxButton.OK, MessageBoxImage.Warning);

            DataContext = _model;

            // breakedown structure
            BreakedownItems = _model.BreakdownStructure.Where(bs => bs.Parent == null)
                .Select(i => new BreakdownItemView(i, ContextSelector, false))
                .ToList();
            if (BreakedownItems.Any())
                BreakedownItems[0].IsSelected = true;

            Actors = _model.Actors.Select(a => new ActorView(a, ContextSelector)).ToList();
            if (Actors.Any())
                Actors[0].IsSelected = true;

            Milestones = _model.Milestones.Select(a => new MilestoneView(a, ContextSelector)).ToList();
            if (Milestones.Any())
                Milestones[0].IsSelected = true;

            Reasons = _model.Reasons.Select(a => new ReasonView(a, ContextSelector)).ToList();
            if (Reasons.Any())
                Reasons[0].IsSelected = true;
        }

        private bool UpdateComment(Comment comment)
        {
            if (string.IsNullOrWhiteSpace(User))
            {
                MessageBox.Show("Nejprve musíte vyplnit Váš email vpravo nahoře");
                return false;
            }

            var actor = Actors.FirstOrDefault(a => a.IsSelected);
            var reason = Reasons.FirstOrDefault(r => r.IsSelected);
            var milestone = Milestones.FirstOrDefault(m => m.IsSelected);
            var breakdown = BreakedownItems.SelectMany(i => i.GetDeepSelected()).FirstOrDefault();

            if (actor == null)
            {
                MessageBox.Show("Není vybraný žádný aktér");
                return false;
            }

            if (reason == null)
            {
                MessageBox.Show("Není vybraný žádný účel");
                return false;
            }

            if (milestone == null)
            {
                MessageBox.Show("Není vybraný žádný milník");
                return false;
            }

            if (breakdown == null)
            {
                MessageBox.Show("Není vybraná žádná kategorie DSS");
                return false;
            }

            if (CurrentRequirement == null)
            {
                MessageBox.Show("Není vybraný žádný požadavek");
                return false;
            }

            comment.Author = User;
            comment.ActorId = actor?.Entity.Id;
            comment.ActorName = actor?.Name;
            comment.MilestoneId = milestone?.Entity.Id;
            comment.MilestoneName = milestone?.Name;
            comment.ReasonId = reason?.Entity.Id;
            comment.ReasonName = reason?.Name;
            comment.BreakDownId = breakdown?.Entity.Id;
            comment.BreakDownName = breakdown?.Name;
            comment.RequirementId = CurrentRequirement.Id;
            comment.RequirementName = CurrentRequirement.Name;
            comment.CreatedOn = DateTime.UtcNow;

            return true;
        }

        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentComment == null)
            {
                MessageBox.Show("Žádný komentář");
                return;
            }

            if (!UpdateComment(CurrentComment))
            {
                return;
            }

            if (!Comments.Contains(CurrentComment))
            {
                Comments.Add(CurrentComment);
            }

            SaveComments_Click(sender, null);
        }
    }
}
