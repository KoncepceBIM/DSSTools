using LOIN.Viewer.Views;
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
    /// Interaction logic for RequirementDetails.xaml
    /// </summary>
    public partial class RequirementDetails : UserControl
    {
        public RequirementDetails()
        {
            InitializeComponent();
        }



        public RequirementView Requirement
        {
            get { return (RequirementView)GetValue(RequirementProperty); }
            set { SetValue(RequirementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequirementProperty =
            DependencyProperty.Register("Requirement", typeof(RequirementView), typeof(RequirementDetails), new PropertyMetadata(null, (s, a) =>
            {
                if (!(s is RequirementDetails self))
                    return;

                if (a.NewValue is RequirementView requirement)
                    self.propertyInformation.Visibility = Visibility.Visible;
                else
                    self.propertyInformation.Visibility = Visibility.Collapsed;
            }));



        public SingleContext Context
        {
            get { return (SingleContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Context.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(SingleContext), typeof(RequirementDetails), new PropertyMetadata(null, (s, a) =>
            {
                if (!(s is RequirementDetails self))
                    return;

                if (a.NewValue is SingleContext context)
                { 
                    if (context.IsComplete)
                        self.contextInformation.Visibility = Visibility.Visible;
                    context.ContextUpdatedEvent += (s, a) => {
                        if (context.IsComplete)
                            self.contextInformation.Visibility = Visibility.Visible;
                        else
                            self.contextInformation.Visibility = Visibility.Collapsed;
                    };
                }
                else
                    self.contextInformation.Visibility = Visibility.Collapsed;
            }));
    }
}
