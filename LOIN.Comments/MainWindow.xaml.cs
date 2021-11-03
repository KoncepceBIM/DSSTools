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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        private bool isClosing = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            Focus();

            base.OnClosing(e);
            if (!Comments.Any())
                return;

            SaveComments_Click(null, null);
            isClosing = true;
        }

        public string CurrentFile { get; set; }

        public string CurrentCommentsFile { get; set; }

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





        public SingleContext SingleContext
        {
            get { return (SingleContext)GetValue(SingleContextProperty); }
            set { SetValue(SingleContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SingleContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleContextProperty =
            DependencyProperty.Register("SingleContext", typeof(SingleContext), typeof(MainWindow), new PropertyMetadata(null));




        public RequirementView CurrentRequirement
        {
            get { return (RequirementView)GetValue(CurrentRequirementProperty); }
            set { SetValue(CurrentRequirementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentRequirement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRequirementProperty =
            DependencyProperty.Register("CurrentRequirement", typeof(RequirementView), typeof(MainWindow), new PropertyMetadata(null, (d, e) =>
            {
                if (!(d is MainWindow self))
                    return;

                if (!(e.NewValue is RequirementView rv))
                {
                    self.CurrentComment = null;
                    return;
                }

                var actor = self.Actors.FirstOrDefault(a => a.IsSelected);
                var reason = self.Reasons.FirstOrDefault(r => r.IsSelected);
                var milestone = self.Milestones.FirstOrDefault(m => m.IsSelected);
                var breakdown = self.BreakedownItems.SelectMany(i => i.GetDeepSelected()).FirstOrDefault();

                // get existing comment
                self.CurrentComment = self.Comments.FirstOrDefault(c =>
                    c.RequirementId == rv.Id &&
                    c.ActorId == actor?.Entity.Id &&
                    c.MilestoneId == milestone?.Entity.Id &&
                    c.ReasonId == reason?.Entity.Id &&
                    c.BreakDownId == breakdown?.Entity.Id
                    );

                // if the context is fully defined, create new one
                if (self.CurrentComment == null &&
                actor != null &&
                reason != null &&
                milestone != null &&
                breakdown != null
                )
                {
                    self.CurrentComment = new Comment
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
                        RequirementSetName = rv.Parent?.Name,
                        Type = CommentType.Comment
                    };
                }
            }));



        public Comment CurrentComment
        {
            get { return (Comment)GetValue(CurrentCommentProperty); }
            set { SetValue(CurrentCommentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentCommentProperty =
            DependencyProperty.Register("CurrentComment", typeof(Comment), typeof(MainWindow), new PropertyMetadata(null, (s, a) =>
            {
                if (!(s is MainWindow self))
                    return;
            }));



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

        private void ChangeUser_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UserDialog { User = User, Owner = this };
            dlg.ShowDialog();
            if (!dlg.IsValid())
                ChangeUser_Click(sender, e);
            else
            {
                User = dlg.User;
                App.Settings.User = User;
            }
        }

        private void SaveComments_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CurrentCommentsFile) && File.Exists(CurrentCommentsFile))
            {
                SaveComments(CurrentCommentsFile);
                SystemSounds.Beep.Play();
            }
            else
                SaveCommentsAs_Click(sender, e);
        }

        private void SaveCommentsAs_Click(object sender, RoutedEventArgs e)
        {

            var dlg = new SaveFileDialog
            {
                Filter = "CSV|*.csv",
                AddExtension = true,
                FilterIndex = 0,
                FileName = App.Settings.LastComments,
                InitialDirectory = Path.GetDirectoryName(App.Settings.LastComments),
                Title = "Uložit komentáře..."
            };
            if (dlg.ShowDialog() != true)
                return;

            CurrentCommentsFile = dlg.FileName;
            App.Settings.LastComments = dlg.FileName;

            if (CurrentComment != null)
                SaveComment(CurrentComment);

            SaveComments(CurrentCommentsFile);

            SystemSounds.Beep.Play();
        }

        private void SaveComments(string path)
        {
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Configuration.SanitizeForInjection = false;
                csv.WriteRecords(Comments.Where(c => !c.IsEmpty()));
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadComments_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "CSV|*.csv",
                FilterIndex = 0,
                Title = "Načíst komentáře...",
                FileName = App.Settings.LastComments,
                InitialDirectory = Path.GetDirectoryName(App.Settings.LastComments)
            };

            if (dlg.ShowDialog() != true)
                return;

            LoadComments(dlg.FileName);
        }

        private void LoadComments(string path)
        {
            CurrentCommentsFile = path;
            App.Settings.LastComments = path;

            try
            {
                using (var file = new StreamReader(path, Encoding.UTF8))
                using (var reader = new CsvReader(file, CultureInfo.InvariantCulture))
                {
                    reader.Configuration.HeaderValidated = null;
                    reader.Configuration.MissingFieldFound = null;

                    var comments = reader.GetRecords<Comment>().ToList();
                    var check = new HashSet<Guid>(comments.Select(c => c.Id));
                    var duplicities = new HashSet<Guid>(Comments.Where(c => check.Contains(c.Id)).Select(c => c.Id));
                    if (duplicities.Any())
                    {
                        var msg = MessageBox.Show($"{duplicities.Count} záznamů v souboru je již načteno. Chcete je nahradit záznamy ze souboru?", "Duplicity",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                        if (msg == MessageBoxResult.No)
                            comments = comments.Where(c => !duplicities.Contains(c.Id)).ToList();
                        else
                            Comments = new ObservableCollection<Comment>(Comments.Where(c => !duplicities.Contains(c.Id)));
                    }
                    foreach (var comment in comments)
                    {
                        Comments.Add(comment);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, $"Soubor s komentáři '{Path.GetFileName(path)}' se nepodařilo načíst: {e.Message}");
                App.Settings.LastComments = "";
            }

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

            var startFile = App.Settings.LastIFC;
            if (!string.IsNullOrWhiteSpace(startFile) && File.Exists(startFile))
                OpenFile(startFile);

            var commentsFile = App.Settings.LastComments;
            if (!string.IsNullOrWhiteSpace(commentsFile) && File.Exists(commentsFile))
                LoadComments(commentsFile);

            if (!string.IsNullOrWhiteSpace(App.Settings.User))
                User = App.Settings.User;

        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (string.IsNullOrWhiteSpace(User))
                ChangeUser_Click(null, null);
        }

        private void OpenFile(string path)
        {
            CurrentFile = path;
            Title = $"Připomínky k DSS: {path}";

            try
            {
                _model = Model.Open(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, $"Nepodařilo se načíst požadavky ze souboru {Path.GetFileName(path)}: {e.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentFile = "";
                Title = $"Připomínky k DSS";
                return;
            }

            ContextSelector = new ContextSelector(_model, false);
            SingleContext = new SingleContext(ContextSelector);

            ContextSelector.ContextUpdatedEvent += (s, a) =>
            {
                CurrentComment = null;
            };

            if (!_model.Requirements.Any())
            { 
                MessageBox.Show(this, "Tento soubor neobsahuje žádné požadavky", "Not a LOIN", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            DataContext = _model;

            // breakedown structure
            BreakedownItems = _model.BreakdownStructure.Where(bs => bs.Parent == null)
                .Select(i => new BreakdownItemView(i, null, ContextSelector, true))
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

            if (CurrentRequirement == null && comment.Type == CommentType.Comment)
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
            comment.CreatedOn = DateTime.UtcNow;

            if (CurrentRequirement != null && comment.Type == CommentType.Comment)
            {
                comment.RequirementId = CurrentRequirement.Id;
                comment.RequirementName = CurrentRequirement.Name2;
                comment.RequirementSetName = CurrentRequirement.Parent?.Name2;
            }

            return true;
        }

        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentComment == null)
            {
                MessageBox.Show("Žádný komentář");
                return;
            }

            SaveComment(CurrentComment);
        }

        private void SaveComment(Comment comment)
        {
            if (comment == null)
            {
                return;
            }

            if (!UpdateComment(comment))
            {
                return;
            }

            if (!Comments.Contains(comment))
            {
                if (!comment.IsEmpty())
                    Comments.Add(comment);
            }

            // there might be one last comment which hasn't been saved
            if (isClosing && !string.IsNullOrWhiteSpace(CurrentCommentsFile) && File.Exists(CurrentCommentsFile))
            {
                SaveComments(CurrentCommentsFile);
            }
        }

        private void dgComments_Selected(object sender, RoutedEventArgs e)
        {
            if (!(sender is DataGrid grid))
                return;

            if (!(grid.SelectedItem is Comment comment))
                return;

            var actor = Actors.FirstOrDefault(a => a.Entity.Id == comment.ActorId);
            var reason = Reasons.FirstOrDefault(a => a.Entity.Id == comment.ReasonId);
            var milestone = Milestones.FirstOrDefault(a => a.Entity.Id == comment.MilestoneId);
            var breakdown = BreakedownItems.Select(i => i.GetDeep(comment.BreakDownId)).Where(i => i != null).FirstOrDefault();

            if (actor != null)
                actor.IsSelected = true;
            if (reason != null)
                reason.IsSelected = true;
            if (milestone != null)
                milestone.IsSelected = true;
            if (breakdown != null)
            {
                breakdown.IsSelected = true;
                var parent = breakdown.Parent;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.Parent;
                }
            }

            var requirement = ContextSelector.Requirements.FirstOrDefault(r => r.Id == comment.RequirementId);
            if (requirement != null)
            {
                if (requirement.Parent != null)
                    requirement.Parent.IsSelected = true;

                requirement.IsSelected = true;
                CurrentRequirement = requirement;
            }
            else
            {
                CurrentComment = comment;
            }

        }

        private void NewRequirement_Click(object sender, RoutedEventArgs e)
        {
            if (SingleContext == null || !SingleContext.IsComplete)
            {
                MessageBox.Show(this, "Kontext není plně definován (kategorie, aktér, milník a účel)");
                return;
            }

            var comment = new Comment(SingleContext)
            {
                Author = User,
                Type = CommentType.NewRequirement,
            };

            CurrentRequirement = null;
            Comments.Add(comment);
            CurrentComment = comment;
        }
    }
}
