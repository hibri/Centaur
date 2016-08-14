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
        private readonly IISExpressPathResolver _iisExpressPathResolver = new IISExpressPathResolver();
        private readonly string _scriptPath;
        private Process _process;

        

        public IISExpressHost(string webSitePath, int port)
        {
            LogOutput = true;
            WebSitePath = webSitePath;
            Port = port;
            
        }

        public IISExpressHost(IISExpressConfig iisExpressConfig)
        {
            LogOutput = true;
            Config = iisExpressConfig;
        }

        [Obsolete("Don't use to launch IIS Express via scripts")]
        public IISExpressHost(string scriptPath)
        {
            _scriptPath = scriptPath;
            WebSitePath = "";
            if (!File.Exists(_scriptPath))
                throw new ArgumentException("Couldn't locate the configuration script for IIS express site");
        }

        public int Port { get; private set; }
        public string WebSitePath { get; private set; }
        public IISExpressConfig Config { get; private set; }

        public bool LogOutput { get; set; }

        public string StatusCheckPath { get; set; }
        public TimeSpan StatusCheckInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public int StatusCheckAttempts { get; set; } = 0;
        public bool IsNotFoundValid { get; set; } = false;
        public bool TraceErrors { get; set; }
        public bool SystemTray { get; set; } = false;

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
            if (_process == null) return;
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

        private void Log(string args)
        {
            Console.WriteLine("Started IISExpress with args: {0}", args);
        }

        private void StartFromPath(string iisExpressPath)
        {
            string args;
            if (Config != null)
            {
                var configPath = Path.GetFullPath(Config.Path);
                args = string.IsNullOrEmpty(Config.Site) ? 
                    String.Format("/config:\"{0}\" /systray:{1}", configPath, SystemTray.ToString().ToLower())
                    : String.Format("/config:\"{0}\" /site:\"{1}\" /systray:{2} ", configPath, Config.Site, SystemTray.ToString().ToLower());
            }
            else
            {
                var path = Path.GetFullPath(WebSitePath);
                args = String.Format("/path:{0} /port:{1} /systray:{2}", path, Port, SystemTray.ToString().ToLower());
            }

            if (TraceErrors)
            {
                args += " /trace:error";
            }

            StartProcess(iisExpressPath, args);

            if (!string.IsNullOrEmpty(StatusCheckPath))
            {
                PerformStatusCheck();
            }

            Log(args);
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
                string outputPrefix = "";
                if (! string.IsNullOrEmpty(WebSitePath))
                {
                    var pathSegments = WebSitePath.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
                    if (pathSegments.Count() > 0)
                    {
                        outputPrefix =
                            pathSegments
                                .Last();
                    }
                }
                else if (Config != null)
                {
                    outputPrefix = Path.GetFileName(Config.Path);
                }

                _process.OutputDataReceived +=
                    (sender, eventArgs) => Console.WriteLine("{0} STDOUT => {1}", outputPrefix, eventArgs.Data);
                _process.ErrorDataReceived +=
                    (sender, eventArgs) => Console.WriteLine("{0} STDERR => {1}", outputPrefix, eventArgs.Data);
                
            }

            _process.Start();
            _process.BeginOutputReadLine();
        }

        public bool IsAlive()
        {
            try
            {
                PerformStatusCheck();
                return true;
            }
            catch { return false; }
        }

        private bool TryStatusCheck(string url)
        {
            try
            {
                WebRequest r = WebRequest.Create(url);
                var resp = r.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Message.Contains("404") && IsNotFoundValid) return true;
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