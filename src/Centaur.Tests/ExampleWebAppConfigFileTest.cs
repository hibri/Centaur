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
        private string _configFilePath;

        [SetUp]
        public void StartHost()
        {
            _configFilePath = Helpers.ConfigurePath(TestContext.CurrentContext.TestDirectory);

            _host = new IISExpressHost(new IISExpressConfig(_configFilePath));
            _host.Start();
        }

        [TearDown]
        public void StopHost()
        {
            _host.Stop();
            Helpers.CleanConfig(_configFilePath);
        }

        [Test]
        public void IisExpressHostedWebAppRespondsToRequests()
        {
            Assert.That(Helpers.Get("http://localhost:9060"), Is.EqualTo("hello!"));
        }
    }
}
