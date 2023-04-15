using NebulaLauncher.ViewModels.Windows;
using NebulaLauncher.Views.Windows;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Net;
using System;
using System.Windows;
using NebulaUtils;
using System.Threading;
using System.Threading.Tasks;

namespace NebulaLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if RELEASE
            if (NeedsUpdate())
            {
                Task.Delay(1000).ContinueWith((t) =>
                {
                    Current.Dispatcher.Invoke(() => UpdateLauncher());
                });
            }
            else
            {
                MainWindow = new NebulaWindow();
                MainWindow.Show();
            }
#else
            MainWindow = new NebulaWindow();
            MainWindow.Show();
#endif
        }

        private bool NeedsUpdate()
        {
            string currentVersion = "";
            string latestVersion = "";
            if (File.Exists("version.txt"))
                currentVersion = File.ReadAllText("version.txt");
            latestVersion = WebRequests.DownloadStringSync("http://192.168.0.3/NebulaLauncher/latest_version.txt");

            return AreVersionDifferents(currentVersion, latestVersion);
        }

        private bool AreVersionDifferents(string CurrentVersion, string LatestVersion)
        {
            if (LatestVersion.Length == 0)
                return false;

            if (CurrentVersion.Length == 0)
                return true;

            string[] currentVersionArr = CurrentVersion.Split(new[] { '.' });
            string[] latestVersionArr = LatestVersion.Split(new[] { '.' });

            for (int i = 0; i < currentVersionArr.Length && i < latestVersionArr.Length; ++i)
            {
                int curr = 0;
                try { curr = int.Parse(currentVersionArr[i]); }
                catch (Exception) { return true; }

                int latst = 0;
                try { latst = int.Parse(latestVersionArr[i]); }
                catch (Exception) { return false; }

                if (curr != latst)
                    return curr < latst;
            }

            if (currentVersionArr.Length != latestVersionArr.Length)
                return currentVersionArr.Length < latestVersionArr.Length;

            return false;
        }

        private void UpdateLauncher()
        {
            if (Directory.Exists("Installer"))
                Directory.Delete("Installer", true);
            Directory.CreateDirectory("Installer");

            WebRequests.DownloadFileSync(Url.Combine("http://192.168.0.3/", "NebulaUpdater/NebulaUpdater.zip"), "Installer/NebulaUpdater.zip");
            ZipFile.ExtractToDirectory("Installer/NebulaUpdater.zip", "Installer/");
            var updaterPath = Path.Combine(Environment.CurrentDirectory, "Installer/NebulaUpdater.exe");

            Process.Start(updaterPath);
            App.Current.Shutdown();
        }
    }
}
