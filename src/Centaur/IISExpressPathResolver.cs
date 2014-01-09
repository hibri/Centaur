using System;
using System.IO;

namespace Centaur
{
    internal class IISExpressPathResolver
    {
        public const string IISExpressDir = @"IIS Express";
        public const string IISExpressExe = "iisexpress.exe";

        public string GetPath()
        {
            var iisExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                IISExpressDir,
                IISExpressExe);
            if (Environment.Is64BitOperatingSystem)
            {
                iisExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    IISExpressDir, IISExpressExe);
            }
            return iisExpressPath;
        }
    }
}