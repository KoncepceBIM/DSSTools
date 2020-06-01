using LOIN.Viewer.Views;
using System.IO;
using System.Windows;

namespace LOIN.Comments
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static Settings Settings { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // make Czech fixed secondary language
            Language.Lang = "cs";

            Settings = Settings.Open();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Save();
            base.OnExit(e);
        }
    }
}
