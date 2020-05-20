using System.Text.RegularExpressions;
using System.Windows;

namespace LOIN.Comments
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class UserDialog : Window
    {
        public UserDialog()
        {
            InitializeComponent();
        }



        public string User
        {
            get { return (string)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for User.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(string), typeof(UserDialog), new PropertyMetadata(null));

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid())
            {
                MessageBox.Show("Musíte zadat Váš email");
                return;
            }

            Close();
        }

        public bool IsValid()
        {

            if (string.IsNullOrWhiteSpace(User))
                return false;

            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(User);
        }
    }
}
