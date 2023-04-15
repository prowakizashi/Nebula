using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManifestGenerator
{
    class ManifestGenerator
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: <AppsFolder> <AppName> <version_number>");
                return;
            }

            string AppsFolder = args[0];
            string AppName = args[1];
            string Version = args[2];

            if (!Directory.Exists(AppsFolder))
            {
                Console.WriteLine("Error: Couldn't find Apps directory at '" + AppsFolder + "'.");
                return;
            }

            if (!Directory.Exists(Path.Combine(AppsFolder, AppName)))
            {
                Console.WriteLine("Error: Couldn't find directory for '" + AppName + "' int Apps folder.");
                return;
            }

            if (!Directory.Exists(Path.Combine(AppsFolder, AppName, Version)))
            {
                Console.WriteLine("Error: Couldn't find directory for version: '" + Version + "'.");
                return;
            }

            string path = Path.Combine(AppsFolder, AppName, Version, "app/");
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Error: Couldn't find any app sub-directory in version directory.");
                return;
            }

            ManifestWriter.GenerateManifest(path);
        }
    }
}
