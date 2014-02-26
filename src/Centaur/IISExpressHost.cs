using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;

namespace Centaur
{
    public class IISExpressHost : IDisposable
    {
        private readonly IISExpressPathResolver _iisExpressPathResolver;
        private readonly string _scriptPath;
        private Process _process;

        private IISExpressHost()
        {
            _iisExpressPathResolver = new IISExpressPathResolver();
        }

        public IISExpressHost(string webSitePath, int port)
            : this()
        {
            LogOutput = true;
            WebSitePath = webSitePath;
            Port = port;
            
        }

        public IISExpressHost(string scriptPath)
            : this()
        {
            _scriptPath = scriptPath;
            if (!File.Exists(_scriptPath))
                throw new ArgumentException("Couldn't locate the configuration script for IIS express site");
        }

        public int Port { get; private set; }
        public string WebSitePath { get; private set; }

        public bool LogOutput { get; set; }

        public string StatusCheckPath { get; set; }
        public TimeSpan StatusCheckInterval { get; set; }
        public int StatusCheckAttempts { get; set; }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception)
            {
            }
        }

        public void Start()
        {
            var iisExpressPath = _iisExpressPathResolver.GetPath();
            if (!File.Exists(iisExpressPath))
            {
                throw new MissingIISExpressException(iisExpressPath);
            }

            _process = new Process();
            if (_scriptPath == null)
            {
                StartFromPath(iisExpressPath);
            }
            else
            {
                StartFromScript();
            }
        }

        public void Stop()
        {
            _process.StandardInput.Write("Q");
            KillProcessAndChildren(_process.Id);
        }

        private void PerformStatusCheck()
        {
            var url = String.Format("http://localhost:{0}{1}", Port, StatusCheckPath);

            for (var i = 0; i < StatusCheckAttempts; i++)
            {
                if (TryStatusCheck(url))
                    return;
            }

            throw new Exception("Failed to connect to " + url);
        }

        private void Log(string path, string args)
        {
            Console.WriteLine("Started IISExpress for {0} {1} on port {2}", path, args, Port);
        }

        private void StartFromPath(string iisExpressPath)
        {
            var path = Path.GetFullPath(WebSitePath);
            var args = String.Format("/path:{0} /port:{1} /systray:false", path, Port);

            StartProcess(iisExpressPath, args);

            if (!string.IsNullOrEmpty(StatusCheckPath))
            {
                PerformStatusCheck();
            }
            Log(path, args);
        }

        private void StartFromScript()
        {
            StartProcess(_scriptPath, "");
        }

        private void StartProcess(string iisExpressPath, string args)
        {
            if (LogOutput) Console.WriteLine(iisExpressPath + " " + args);
            _process.StartInfo = new ProcessStartInfo(iisExpressPath, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            _process.EnableRaisingEvents = true;
            if (LogOutput)
            {
                var dirname =
                    WebSitePath.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
                        .Last();
                _process.OutputDataReceived +=
                    (sender, eventArgs) => Console.WriteLine("{0} STDOUT => {1}", dirname, eventArgs.Data);
                _process.ErrorDataReceived +=
                    (sender, eventArgs) => Console.WriteLine("{0} STDERR => {1}", dirname, eventArgs.Data);
                
            }

            _process.Start();
            _process.BeginOutputReadLine();
        }

        private bool TryStatusCheck(string url)
        {
            try
            {
                new WebClient().DownloadString(url);
            }
            catch (WebException)
            {
                Thread.Sleep(StatusCheckInterval);
                return false;
            }
            return true;
        }

        private static void KillProcessAndChildren(int pid)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            var managementObjects = searcher.Get().Cast<ManagementObject>();
            foreach (var managementObject in managementObjects)
            {
                KillProcessAndChildren(Convert.ToInt32(managementObject["ProcessID"]));
            }
            try
            {
                Process.GetProcessById(pid).Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }
}