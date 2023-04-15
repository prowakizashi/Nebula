using ManifestCommon;
using NebulaUtils;
using System;
using System.IO;
using System.Net;

namespace NebulaDownloader
{
    public enum EAppDownloadResult
    {
        SUCCESS,
        ALREADY_UP_TO_DATE,
        ERROR
    }

    public enum EAppDownloadPhase
    {
        INIT,
        DOWNLOAD,
        INSTALL
    }

    public enum EAppType
    {
        GAME,
        LAUNCHER,
        UPDATER
    }

    public struct AppDownloadResult
    {
        public EAppDownloadResult Result;
        public string ErrorMessage;

        public bool IsSuccess() { return Result == EAppDownloadResult.SUCCESS; }
        public AppDownloadResult(EAppDownloadResult result = EAppDownloadResult.SUCCESS, string errorMessage = "") { Result = result; ErrorMessage = errorMessage; }
    }

    public struct ProgressData
    {
        public float Percent;
        public long RemainingSize;
        public EAppDownloadPhase Phase;

        public ProgressData(EAppDownloadPhase phase, long remainingSize = 0, float percent = 0.0f) { Phase = phase; RemainingSize = remainingSize; Percent = percent; }
    }

    public class AppDownloader
    {

        private string AppName;
        private string AppTypePath;

        public AppDownloader(string AppName, EAppType AppType)
        {
            this.AppName = AppName;
            this.AppTypePath = GetAppTypePath(AppType);
        }

        public bool IsNewUpdateAvailable(string InstallPath)
        {
            string currentVersion = "";
            string latestVersion = "";

            var localVersionPath = Path.Combine(InstallPath + "version.txt");
            if (File.Exists(localVersionPath))
            {
                currentVersion = File.ReadAllText(localVersionPath);
            }

            string AppUrl = Url.Combine("http://192.168.0.3/", AppTypePath, AppName);
            using (var client = new WebClient())
            {
                try
                {
                    var url = Url.Combine(AppUrl, "latest_version.txt");
                    latestVersion = client.DownloadString(url);
                    return string.IsNullOrEmpty(currentVersion) != string.IsNullOrEmpty(latestVersion) && currentVersion != latestVersion;
                }
                catch (Exception) { return false; }
            }
        }

        public AppDownloadResult DownloadLatestVersion(string TempFoldPath, string InstallPath, Action<ProgressData> ProgressCallback)
        {
            if (InstallPath == null) { return new AppDownloadResult(EAppDownloadResult.ERROR, "Le chemin vers le dossier d'installation est invalide."); }

            bool UseTempFolder = TempFoldPath != null && TempFoldPath.Length > 0;
            var manifestReader = new ManifestReader();

            PatchInfo Patch = null;
            ProgressCallback(new ProgressData(EAppDownloadPhase.INIT));
            AppDownloadResult result = manifestReader.GetPatch(AppName, AppTypePath, InstallPath, ref Patch);
            if (Patch != null && result.IsSuccess())
            {
                var AppDownloadFolder = Path.Combine(TempFoldPath, AppName);
                ProgressCallback(new ProgressData(EAppDownloadPhase.DOWNLOAD));
                result = DownloadPatch(Patch, UseTempFolder ? AppDownloadFolder : InstallPath, ProgressCallback);
                ProgressCallback(new ProgressData(EAppDownloadPhase.INSTALL));
                if (result.IsSuccess())
                {
                    if (!Directory.Exists(InstallPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(InstallPath);
                        } catch (Exception) { return new AppDownloadResult(EAppDownloadResult.ERROR, "Impossible de créer le dossier d'installation."); }
                    }
                }
                if (UseTempFolder)
                {
                    result = MoveFiles(new DirectoryInfo(AppDownloadFolder), new DirectoryInfo(InstallPath));
                }

                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(Url.Combine("http://192.168.0.3/", AppTypePath, AppName, "latest_version.txt"), Path.Combine(InstallPath, "version.txt"));
                    }
                    catch (Exception) {
                        return new AppDownloadResult(EAppDownloadResult.ERROR, "Impossible de mettre à jour le fichier version.txt. Peut être est-il utilisé par un autre programme.");
                    }
                }
            }

            return result;
        }

        private string GetAppTypePath(EAppType AppType)
        {
            switch (AppType)
            {
                case EAppType.GAME:
                    return "Games";
                default:
                    return "";
            }
        }

        private AppDownloadResult DownloadPatch(PatchInfo Patch, string AppDownloadFolder, Action<ProgressData> ProgressCallback)
        {
            if (Patch == null)
                return new AppDownloadResult(EAppDownloadResult.ERROR, "Le patch généré est invalide.");


            long downloaded = 0;
            using (var client = new WebClient())
            {
                foreach (var file in Patch.ToDownloadFiles)
                {
                    ProgressCallback(new ProgressData(EAppDownloadPhase.DOWNLOAD, Patch.DownloadSize - downloaded, ((float)downloaded / (float)Patch.DownloadSize) * 100.0f));
                    var localFolderPath = Path.Combine(AppDownloadFolder, file.Path);
                    if (!Directory.Exists(localFolderPath))
                        Directory.CreateDirectory(localFolderPath);

                    var localFilePath = Path.Combine(localFolderPath, file.FileName);
                    if (File.Exists(localFilePath))
                    {
                        string checksum = Checksum.GenerateFileChecksum(localFilePath);
                        if (checksum == file.Checksum)
                        {
                            downloaded += file.FileSize;
                            continue;
                        }
                    }

                    Console.WriteLine("Downloading file '" + file.Path + file.FileName);
                    var url = Url.Combine(Patch.DistantFolderPath, file.Path, file.FileName);
                    try
                    {
                        client.DownloadFile(url, localFilePath);
                    }
                    catch (Exception) { } // manifest contains wrong files
                    downloaded += file.FileSize;
                }
            }

            return new AppDownloadResult();
        }

        private AppDownloadResult MoveFiles(DirectoryInfo FromDirectory, DirectoryInfo ToDirectory)
        {
            var result = FileSystem.MoveAll(FromDirectory, ToDirectory);
            switch (result.Result)
            {
                default:
                case EFileOperationResult.SUCCESS:
                    return new AppDownloadResult(EAppDownloadResult.SUCCESS);
                case EFileOperationResult.ERROR:
                    return new AppDownloadResult(EAppDownloadResult.ERROR, string.IsNullOrEmpty(result.FileName) ? result.ErrorMessage : result.FileName + ": " + result.ErrorMessage);

            }
        }
    }
}
