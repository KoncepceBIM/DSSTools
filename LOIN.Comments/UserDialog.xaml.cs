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

        private static readonly Regex parse = new Regex("(?<name>.*)<(?<email>.*)>");
        public string User
        {
            get => $"{UserName} <{Email}>";
            set
            {
                if (value == null)
                {
                    Email = null;
                    UserName = null;
                    return;
                }

                var match = parse.Match(value);
                if (!match.Success)
                {
                    Email = null;
                    UserName = null;
                    return;
                }

                Email = match.Groups["email"]?.Value?.Trim();
                UserName = match.Groups["name"]?.Value?.Trim();
            }
        }


        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Email.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(UserDialog), new PropertyMetadata(null));






        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(string), typeof(UserDialog), new PropertyMetadata(null));





        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid())
            {
                MessageBox.Show("Musíte zadat Vaše jméno a email.");
                return;
            }

            Close();
        }

        public bool IsValid()
        {

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(UserName))
                return false;

            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(Email);
        }
    }
}
