using EyeTrackVR_Installer;
using System.IO;
using System.IO.Compression;

namespace EyeTrackVR_Installer
{
    internal static class ZipArchiveExtensionsHelpers
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = System.IO.Path.Combine(destinationDirectoryName, file.FullName);
                string directory = System.IO.Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }
}