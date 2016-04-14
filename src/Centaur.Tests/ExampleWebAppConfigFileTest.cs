using System.IO;
using System.Net;
using NUnit.Framework;
using Microsoft.Web.Administration;

namespace Centaur.Tests
{
    [TestFixture]
    public class ExampleWebAppConfigFileTest
    {
        private IISExpressHost _host;
        private readonly string _configFilePath = Path.GetFullPath("../../applicationhost.config");
        private readonly string _webAppPath = Path.GetFullPath("../../../Centaur.ExampleWebApp/");

        [SetUp]
        public void StartHost()
        {
            configurePhysicalPath(_configFilePath, _webAppPath);

            _host = new IISExpressHost(new IISExpressConfig(_configFilePath));
            _host.Start();
        }

        [TearDown]
        public void StopHost()
        {
            _host.Stop();
            configurePhysicalPath(_configFilePath, "(none)");
        }

        private void configurePhysicalPath(string configFilePath, string applicationPath)
        {
            using (var serverManager = new ServerManager(configFilePath))
            {
                serverManager.Sites[0].Applications[0].VirtualDirectories[0].PhysicalPath = applicationPath;
                serverManager.CommitChanges();
            }
        }

        [Test]
        public void IisExpressHostedWebAppRespondsToRequests()
        {
            Assert.That(Get("http://localhost:9060"), Is.EqualTo("hello!"));
        }

        static string Get(string url)
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
