using System;
using System.IO;
using System.Linq;
using System.Net;
using ManifestCommon;
using NebulaUtils;
using Newtonsoft.Json;

namespace NebulaDownloader
{

    internal class ManifestReader
    {

        public AppDownloadResult GetPatch(string AppName, string AppTypePath, string InstallPath, ref PatchInfo Patch)
        {
            string currentVersion = "";
            string latestVersion = "";
            string currentManifestString = "";
            string latestManifestString = "";

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
                    if (currentVersion == latestVersion)
                        return new AppDownloadResult(EAppDownloadResult.ALREADY_UP_TO_DATE);
                }
                catch (Exception) { return new AppDownloadResult(EAppDownloadResult.ERROR, "A échoué à obtenir la dernière version."); }

                try
                {
                    if (currentVersion.Length > 0)
                        currentManifestString = client.DownloadString(Url.Combine(AppUrl, currentVersion, "manifest.json"));
                    latestManifestString = client.DownloadString(Url.Combine(AppUrl, latestVersion, "manifest.json"));
                }
                catch (Exception) { return new AppDownloadResult(EAppDownloadResult.ERROR, "Impossible de télécharger le manifest."); }
            }

            Manifest currentManifest = currentManifestString.Length == 0 ? null : JsonConvert.DeserializeObject<Manifest>(currentManifestString);
            Manifest latestManifest = latestManifestString.Length == 0 ? null : JsonConvert.DeserializeObject<Manifest>(latestManifestString);

            if (latestManifest == null)
                return new AppDownloadResult(EAppDownloadResult.ERROR, "Impossible de télécharger le manifest de mise à jour.");

            PatchManifests(currentManifest, latestManifest, ref Patch);
            Patch.DistantFolderPath = Url.Combine(AppUrl, latestVersion, "app/");
            return new AppDownloadResult();
        }

        private void PatchManifests(Manifest CurrentMan, Manifest LatestMan, ref PatchInfo Patch)
        {
            Patch = new PatchInfo();
            if (LatestMan == null)
                return;

            while (CurrentMan != null && CurrentMan.Files.Count > 0)
            {
                var fileDataPair = CurrentMan.Files.First();

                string fileName = fileDataPair.Key;
                FileData currFileData = fileDataPair.Value;
                FileData latestFileData;
                LatestMan.Files.TryGetValue(fileName, out latestFileData);

                if (latestFileData == null)
                {
                    Patch.ToRemoveFiles.Add(currFileData);
                }
                else
                {
                    if (currFileData != latestFileData)
                    {
                        Patch.DownloadSize += fileDataPair.Value.FileSize;
                        Patch.ToDownloadFiles.Add(latestFileData);
                    }
                    LatestMan.Files.Remove(fileName);
                }

                CurrentMan.Files.Remove(fileName);
            }

            foreach (var fileDataPair in LatestMan.Files)
            {
                Patch.DownloadSize += fileDataPair.Value.FileSize;
                Patch.ToDownloadFiles.Add(fileDataPair.Value);
            }
        }
    }
}
