using ManifestCommon;
using NebulaDownloader;
using NebulaLauncher.Managers;
using NebulaLauncher.Models;
using NebulaLauncher.Views.Windows;
using NebulaUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace NebulaLauncher.ViewModels.Widgets
{
    public enum EInstallStatus
    {
        NOT_INSTALLED,
        INSTALLED,
        PENDING,
        DOWNLOADING,
        COPYING
    }

    public class GameAppViewModel : ViewModelBase
    {
        private GameData gameData;
        public GameData GameData { get { return gameData; } set { gameData = value; OnPropertyChanged(); } }

        private Visibility installVisibility;
        public Visibility InstallVisibility { get { return installVisibility; } set { installVisibility = value; OnPropertyChanged(); } }

        private Visibility playVisibility;
        public Visibility PlayVisibility { get { return playVisibility; } set { playVisibility = value; OnPropertyChanged(); } }

        private Visibility phaseVisibility;
        public Visibility PhaseVisibility { get { return phaseVisibility; } set { phaseVisibility = value; OnPropertyChanged(); } }

        private bool actionAvailable;
        public bool ActionAvailable { get { return actionAvailable; } set { actionAvailable = value; OnPropertyChanged(); } }

        private string? installStep;
        public string? InstallStep { get { return installStep; } set { installStep = value; OnPropertyChanged(); } }

        private string? downloadPercent;
        public string? DownloadPercent { get { return downloadPercent; } set { downloadPercent = value; OnPropertyChanged(); } }

        private string? downloadSize;
        public string? DownloadSize { get { return downloadSize; } set { downloadSize = value; OnPropertyChanged(); } }

        private Visibility downloadVisibility;
        public Visibility DownloadVisibility { get { return downloadVisibility; } set { downloadVisibility = value; OnPropertyChanged(); } }

        public bool updateAvailable { get; set; }
        public bool UpdateAvailable { get { return updateAvailable; } set { updateAvailable = value; OnPropertyChanged(); } }

        private string? iconPath;
        public string? IconPath { get { return iconPath; } set { iconPath = value; OnPropertyChanged(); } }

        public ICommand? InstallCommand { get; }
        public ICommand? UninstallCommand { get; }
        public ICommand? PlayCommand { get; }

        public GameAppViewModel(GameData Data)
        {
            this.gameData = Data;
            InstallCommand = new RelayCommand(InstallOrUpdateApp);
            UninstallCommand = new RelayCommand(UninstallApp);
            PlayCommand = new RelayCommand(PlayApp);

            InstallVisibility = Data.Installed ? Visibility.Collapsed : Visibility.Visible;
            PlayVisibility = Data.Installed ? Visibility.Visible : Visibility.Collapsed;
            DownloadVisibility = Visibility.Collapsed;
            PhaseVisibility = Visibility.Collapsed;

            var dirPath = Path.Combine("Data/Games/", GameData.GameID);
            WebRequests.DownloadFileAsync(Url.Combine("http://192.168.0.3/", "Games", GameData.GameID, "icon.png"), Path.Combine(dirPath, "icon.png"), () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IconPath = Path.Combine(Environment.CurrentDirectory, dirPath, "icon.png");
                });
            });
        }

        private void InstallOrUpdateApp()
        {
            if (string.IsNullOrEmpty(gameData.InstallFolder) || !gameData.Installed)
            {
                ActionAvailable = false;

                var window = new InstallFolderSelector();
                var model = window.DataContext as InstallFolderSelectorViewModel;
                window.ShowDialog();

                GameData.InstallFolder = model != null && model.SelectedFolder != null ? Path.Combine(model.SelectedFolder.Path, GameData.GameID) : null;
                OnPropertyChanged("GameData");
            }

            PhaseVisibility = Visibility.Visible;
            InstallStep = "En Attente...";
            DownloadManager.Instance.RequestToDownloadGame(this, OnInstallUpdate, OnInstallCompleted);
        }

        private void UninstallApp()
        {
            if (gameData.Installed && !string.IsNullOrEmpty(gameData.InstallFolder))
            {
                try
                {
                    Directory.Delete(gameData.InstallFolder, true);
                } catch { }

                PlayVisibility = Visibility.Collapsed;
                InstallVisibility = Visibility.Visible;

                gameData.InstallFolder = "";
                gameData.Installed = false;

                AppManager.Instance.SaveGamesLocally();
            }
        }

        private void PlayApp()
        {
            if (gameData.Installed && !string.IsNullOrEmpty(gameData.ExecFile) && !string.IsNullOrEmpty(gameData.InstallFolder))
            {
                try
                {
                    Process.Start(Path.Combine(gameData.InstallFolder, gameData.ExecFile));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du lancement de l'exécutable : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnInstallUpdate(ProgressData Progress)
        {
            InstallStep = GetProgressPhaseText(Progress.Phase);
            DownloadPercent = Progress.Percent.ToString("00.0") + "%";
            DownloadSize = (Progress.RemainingSize / 1000000.0f).ToString("00.0") + " MB";
            DownloadVisibility = Progress.Phase == EAppDownloadPhase.DOWNLOAD ? Visibility.Visible : Visibility.Collapsed;
            PhaseVisibility = Visibility.Visible;
        }

        private string GetProgressPhaseText(EAppDownloadPhase Phase)
        {
            switch (Phase)
            {
                case EAppDownloadPhase.DOWNLOAD:
                    return "Téléchargement...";
                case EAppDownloadPhase.INSTALL:
                    return "Installation...";
                case EAppDownloadPhase.INIT:
                    return "Préparation...";
            }
            return "";
        }

        private void OnInstallCompleted(bool Success)
        {
            ActionAvailable = true;
            PhaseVisibility = Visibility.Collapsed;
            PlayVisibility = Success ? Visibility.Visible : Visibility.Collapsed;
            InstallVisibility = Success ? Visibility.Collapsed : Visibility.Visible;
            UpdateAvailable = !Success;
            GameData.Installed = Success;
            AppManager.Instance.SaveGamesLocally();
        }
        
        public void CheckForupdate()
        {
            var downloader = new AppDownloader(GameData.GameID, EAppType.GAME);
            UpdateAvailable = downloader.IsNewUpdateAvailable(GameData.InstallFolder);
        }
    }
}
