using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centaur.Tests
{
    [TestFixture]
    public class IisConfigInitTests
    {
        private IISExpressHost _host;
        private string _configFilePath;

        [SetUp]
        public void Setup()
        {
            IISExpressProcess.ClearAll();
        }

        [TearDown]
        public void StopHost()
        {
            _host?.Stop();
            Helpers.CleanConfig(_configFilePath);
        }

        [Test]
        public void TestThatIisCanStartSiteUsingTheName()
        {
            _configFilePath = Helpers.ConfigurePath(TestContext.CurrentContext.TestDirectory);

            var config = new IISExpressConfig(_configFilePath, "Centaur Example Web App");
            _host = new IISExpressHost(config);
            _host.Start();

            Assert.That(Helpers.Get("http://localhost:9060"), Is.EqualTo("hello!"));
        }
    }
}
