﻿using LOIN.Comments.Data;
using System.Windows;
using System.Windows.Controls;

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
            DependencyProperty.Register("Comment", typeof(Comment), typeof(CommentEditor), new PropertyMetadata(new Comment(), (s, a) => {
                if (!(s is CommentEditor c))
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
