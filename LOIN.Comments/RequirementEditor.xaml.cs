using LOIN.Comments.Data;
using System.Windows;
using System.Windows.Controls;

namespace LOIN.Comments
{
    /// <summary>
    /// Interaction logic for RequirementEditor.xaml
    /// </summary>
    public partial class RequirementEditor : UserControl
    {
        public RequirementEditor()
        {
            InitializeComponent();
        }

        public Comment Comment
        {
            get { return (Comment)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(Comment), typeof(RequirementEditor), new PropertyMetadata(new Comment(), (s, a) => {
                if (!(s is RequirementEditor c))
                    return;

                if (a.NewValue == null)
                    c.Visibility = Visibility.Collapsed;
                else
                {
                    c.Visibility = Visibility.Visible;
                    c.DataContext = a.NewValue;
                }
            }));
    }
}
