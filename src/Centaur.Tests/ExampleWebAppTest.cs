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
            _host = new IISExpressHost("../../../Centaur.ExampleWebApp/", 9059);
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
            Assert.That(Get("http://localhost:9059"), Is.EqualTo("hello!"));
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
