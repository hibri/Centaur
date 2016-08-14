using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Centaur.Tests
{
    internal class Helpers
    {
        internal static string ConfigurePath(string directory)
        {
            var configFilePath = Path.GetFullPath(directory + "/applicationhost.config");
            var webAppPath = Path.GetFullPath(directory + "../../../../Centaur.ExampleWebApp/");
            configurePhysicalPath(configFilePath, webAppPath);

            return configFilePath;
        }

        internal static void CleanConfig(string config)
        {
            configurePhysicalPath(config, "(none)");
        }
        internal static void configurePhysicalPath(string configFilePath, string applicationPath)
        {
            using (var serverManager = new ServerManager(configFilePath))
            {
                serverManager.Sites[0].Applications[0].VirtualDirectories[0].PhysicalPath = applicationPath;
                serverManager.CommitChanges();
            }
        }

        internal static string Get(string url)
        {
            var request = WebRequest.Create(url);
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return null;
                    var reader = new StreamReader(responseStream);
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
