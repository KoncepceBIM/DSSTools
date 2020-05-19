using LOIN.Comments.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LOIN.Comments
{
    /// <summary>
    /// Interaction logic for CommentEditor.xaml
    /// </summary>
    public partial class CommentEditor : UserControl
    {
        public CommentEditor()
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
            DependencyProperty.Register("Comment", typeof(Comment), typeof(CommentEditor), new PropertyMetadata(new Comment()));


    }
}
