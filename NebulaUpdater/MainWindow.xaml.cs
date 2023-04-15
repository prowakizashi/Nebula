using NebulaDownloader;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NebulaUpdater
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string downloadPercent;
        public string DownloadPercent { get { return downloadPercent; } set { downloadPercent = value; OnPropertyChanged(); } }

        private string downloadSize;
        public string DownloadSize { get { return downloadSize; } set { downloadSize = value; OnPropertyChanged(); } }

        public MainWindow()
        {
            DataContext = this;

            downloadPercent = "";
            downloadSize = "";

            InitializeComponent();

            Task.Delay(3000).ContinueWith((t) =>
            {
                Task.Run(() =>
                {
                    StartLauncherUpdate();
                });
            });
        }

        private void StartLauncherUpdate()
        {
            Process[] processus = Process.GetProcessesByName("NebulaLauncher.exe");
            if (processus.Length > 0)
            {
                Thread.Sleep(500);
                StartLauncherUpdate();
                return;
            }

            var downloader = new AppDownloader("NebulaLauncher", EAppType.LAUNCHER);
            var InstallPath = Environment.CurrentDirectory;
            var TempPath = Path.Combine(InstallPath + "Temp/NebulaLauncher/");
            Application.Current.Dispatcher.Invoke(() => UpdateDowloadInfo(new ProgressData(EAppDownloadPhase.INIT, 0, 0)));
            downloader.DownloadLatestVersion(TempPath, InstallPath, (progressData) =>
            {
                Application.Current.Dispatcher.Invoke(() => UpdateDowloadInfo(progressData));
            });

            Application.Current.Dispatcher.Invoke(() => UpdateComplete());
        }

        private void UpdateComplete()
        {
            Process.Start("NebulaLauncher.exe");
            App.Current.Shutdown();
        }

        private void UpdateDowloadInfo(ProgressData progressData)
        {
            DownloadPercent = progressData.Percent.ToString("0.0") + "%";
            DownloadSize = (progressData.RemainingSize / 1000000.0f).ToString("0.0") + " MB";
        }

        protected void OnPropertyChanged([CallerMemberName] string? property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
