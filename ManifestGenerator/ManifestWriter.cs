using ManifestCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManifestGenerator
{
    class ManifestWriter
    {
        static public void GenerateManifest(string FolderPath)
        {
            var Manifest = new Manifest();
            AddDirectoryToManifest(Manifest, FolderPath, "");

            var AppPath = Path.Combine(FolderPath, "../manifest.json");
            using (StreamWriter sw = File.CreateText(AppPath))
            {
                sw.Write(JsonConvert.SerializeObject(Manifest));
            }
        }

        public static void AddDirectoryToManifest(Manifest NewManifest, string distantDirectory, string localDirectory)
        {
            Console.WriteLine("Adding directory '" + localDirectory + "' to Manifest:");

            string[] fileEntries = Directory.GetFiles(distantDirectory);
            foreach (string filePath in fileEntries)
            {
                var fileName = filePath.Split(new[] { '/', '\\' }).Last();
                var distantPath = Path.Combine(distantDirectory, fileName);
                var localPath = Path.Combine(localDirectory, fileName);

                Console.Write("- " + distantPath + " to " + localPath);

                string checksum = Checksum.GenerateFileChecksum(distantPath);
                long fileSize = new FileInfo(distantPath).Length;

                Console.WriteLine(", Checksum: " + checksum + ", File Size: " + fileSize);
                NewManifest.TotalSize += fileSize;
                NewManifest.Files.Add(localPath, new FileData(fileName, localDirectory, checksum, fileSize));
            }

            string[] subdirectoryEntries = Directory.GetDirectories(distantDirectory);
            foreach (string subdirectoryPath in subdirectoryEntries)
            {
                Console.WriteLine("");
                var directoryName = subdirectoryPath.Split(new[] { '/', '\\' }).Last() + "/";
                AddDirectoryToManifest(NewManifest, Path.Combine(distantDirectory, directoryName), Path.Combine(localDirectory, directoryName));
            }
        }
    }
}
