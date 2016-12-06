using System.IO;
using System.Net;
using NUnit.Framework;

namespace Centaur.Tests
{
    [TestFixture]
    public class ExampleWebAppTest
    {
        private IISExpressHost _host;

        [SetUp]
        public void StartHost()
        {
            _host = new IISExpressHost(TestContext.CurrentContext.TestDirectory + "../../../../Centaur.ExampleWebApp/", 9059);
            _host.Start();
        }

        [TearDown]
        public void StopHost()
        {
            _host.Stop();
        }

        [Test]
        public void IisExpressHostedWebAppRespondsToRequests()
        {
            Assert.That(Helpers.Get("http://localhost:9059"), Is.EqualTo("hello!"));
        }
    }
}
