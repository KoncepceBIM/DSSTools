using System.IO;
using System.Windows;

namespace LOIN.Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string StartupFile { get; private set; }

        internal static Settings Settings { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Settings = Settings.Open();

            if (Settings.LastIFC != null && File.Exists(Settings.LastIFC))
                StartupFile = Settings.LastIFC;

            if (e.Args.Length > 0)
                StartupFile = e.Args[0];

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Save();
            base.OnExit(e);
        }
    }
}
