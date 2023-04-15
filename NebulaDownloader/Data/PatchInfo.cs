using ManifestCommon;
using System.Collections.Generic;

namespace NebulaDownloader
{
    internal class PatchInfo
    {
        public long DownloadSize;
        public string DistantFolderPath;
        public List<FileData> ToDownloadFiles = new List<FileData>();
        public List<FileData> ToRemoveFiles = new List<FileData>();
    }
}
