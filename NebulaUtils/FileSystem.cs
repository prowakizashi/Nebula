using System;
using System.IO;

namespace NebulaUtils
{
    public enum EFileOperationResult
    {
        SUCCESS,
        ERROR
    }

    public struct FileOperationResult
    {
        public EFileOperationResult Result;
        public string ErrorMessage;
        public string FileName;

        public bool IsSuccess() { return Result == EFileOperationResult.SUCCESS; }
        public FileOperationResult(EFileOperationResult result = EFileOperationResult.SUCCESS, string errorMessage = "", string fileName = null)
        { Result = result; ErrorMessage = errorMessage; FileName = fileName; }
    }

    public class FileSystem
    {
        public static FileOperationResult CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
                return new FileOperationResult(EFileOperationResult.ERROR, "Le dossier d'origine ou de destination est invalide.");

            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                try
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                } catch (Exception) { new FileOperationResult(EFileOperationResult.ERROR, "Impossible de copier le fichier.", fi.Name); }
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }

            return new FileOperationResult();
        }

        public static FileOperationResult MoveAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
                return new FileOperationResult(EFileOperationResult.ERROR, "Le dossier d'origine ou de destination est invalide.");

            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                try
                {
                    if (File.Exists(Path.Combine(target.ToString(), fi.Name)))
                        File.Delete(Path.Combine(target.ToString(), fi.Name));
                    fi.MoveTo(Path.Combine(target.ToString(), fi.Name));
                }
                catch (Exception) { new FileOperationResult(EFileOperationResult.ERROR, "Impossible de déplacer le fichier.", fi.Name); }
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }

            return new FileOperationResult();
        }
    }
}
