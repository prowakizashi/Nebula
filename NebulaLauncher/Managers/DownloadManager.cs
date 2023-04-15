using NebulaDownloader;
using NebulaLauncher.ViewModels.Widgets;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace NebulaLauncher.Managers
{
    public delegate void DownloadCompletedDelegate(bool Success);
    public delegate void DownloadUpdatedDelegate(ProgressData progressData);

    internal class DownloadManager
    {
        private struct FPendingInstall
        {
            public string AppID;
            public GameAppViewModel App;
            public DownloadCompletedDelegate FinishCallback;
            public DownloadUpdatedDelegate UpdateCallback;

            public FPendingInstall(GameAppViewModel app, DownloadUpdatedDelegate updateCallback, DownloadCompletedDelegate finishCallback)
            {
                AppID = app.GameData.GameID!;
                App = app;
                FinishCallback = finishCallback;
                UpdateCallback = updateCallback;
            }
        }

        private IDictionary<string, FPendingInstall> pendingInstalls;
        private List<string> pendingQueue;
        public bool isDownloading = false;

        private static DownloadManager instance = new DownloadManager();
        public static DownloadManager Instance { get { return instance; } }

        public DownloadManager()
        {
            pendingInstalls = new Dictionary<string, FPendingInstall>();
            pendingQueue = new List<string>();
        }

        public void RequestToDownloadGame(GameAppViewModel App, DownloadUpdatedDelegate UpdateCallback, DownloadCompletedDelegate CompleteCallback)
        {
            lock (pendingQueue)
            {
                if (pendingQueue.Contains(App.GameData.GameID!))
                    return;
                pendingQueue.Add(App.GameData.GameID!);
            }
            lock (pendingInstalls)
            {
                pendingInstalls.Add(App.GameData.GameID!, new FPendingInstall(App, UpdateCallback, CompleteCallback));
            }

            if (!isDownloading)
            {
                Task.Run(() =>
                {
                    StartDownload();
                });
            }
        }

        private void StartDownload()
        {
            isDownloading = true;
            while (HasPendingDownloads())
            {
                var gameId = pendingQueue[0];
                var pendingInstall = pendingInstalls[gameId];
                var data = pendingInstall.App.GameData;
                var downloader = new AppDownloader(gameId, EAppType.GAME);

                var result = downloader.DownloadLatestVersion(Path.Combine("Temp", gameId), data.InstallFolder, (ProgressData progressData) =>
                {
                    Application.Current.Dispatcher.Invoke(() => pendingInstall.UpdateCallback(progressData));
                });

                Application.Current.Dispatcher.Invoke(() => pendingInstall.FinishCallback(result.Result == EAppDownloadResult.SUCCESS || result.Result == EAppDownloadResult.ALREADY_UP_TO_DATE));
                lock (pendingQueue)
                {
                    pendingQueue.RemoveAt(0);
                }
                lock (pendingInstalls)
                {
                    pendingInstalls.Remove(gameId);
                }
            }

            isDownloading = false;
        }

        private bool HasPendingDownloads()
        {
            lock (pendingQueue)
            {
                return pendingQueue.Count > 0;
            }
        }


    }
}
