using System.ComponentModel;
using System.Diagnostics;

namespace Centaur
{
    public static class IISExpressProcess
    {
        public static void ClearAll()
        {
            var processesByName = Process.GetProcessesByName("iisexpress");
            foreach (var process in processesByName)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Win32Exception)
                {
                }
            }
        }
    }
}