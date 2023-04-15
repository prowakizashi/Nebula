using NebulaUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Nebula
{
    class Nebula
    {
        static void Main(string[] args)
        {
            if (CheckForUpdate())
            {
                Process.Start("NebulaLauncher.exe");
            }
        }

        private static bool CheckForUpdate()
        {
            string currentVersion = "";
            string latestVersion = "";

            if (File.Exists("version.txt"))
                currentVersion = File.ReadAllText("version.txt");

            using (var client = new WebClient())
            {
                latestVersion = client.DownloadString("http://192.168.0.3/NebulaLauncher/latest_version.txt");
            }

            if (NeedsUpdate(currentVersion, latestVersion))
            {
                using (var client = new WebClient())
                {
                    if (Directory.Exists("Installer"))
                        Directory.Delete("Installer", true);
                    Directory.CreateDirectory("Installer");

                    client.DownloadFile(Url.Combine("http://192.168.0.3/", "NebulaUpdater/NebulaUpdater.zip"), "Installer/NebulaUpdater.zip");
                    ZipFile.ExtractToDirectory("Installer/NebulaUpdater.zip", "Installer/");
                    var updaterPath = Path.Combine(Environment.CurrentDirectory, "Installer/NebulaUpdater.exe");

                    Process.Start(updaterPath);
                }
                return false;
            }
            return true;
        }

        private static bool NeedsUpdate(string CurrentVersion, string LatestVersion)
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
    }
}
