using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManifestCommon
{
    public class FileData
    {
        public string FileName = "";
        public string Path = "";
        public string Checksum = "";
        public long FileSize = 0;

        public FileData(string FileName, string Path, string Checksum, long FileSize)
        {
            this.FileName = FileName;
            this.Path = Path;
            this.Checksum = Checksum;
            this.FileSize = FileSize;
        }
    }

    public class Manifest
    {
        public long TotalSize = 0;
        public IDictionary<string, FileData> Files = new Dictionary<string, FileData>();
    }
}
