using System.IO;
using System.Net;
using NUnit.Framework;

namespace Centaur.Tests
{
    [TestFixture]
    public class EnvironmentVariablesTest
    {
        private IISExpressHost _host;

        [SetUp]
        public void StartHost()
        {
            _host = new IISExpressHost(TestContext.CurrentContext.TestDirectory + "../../../../Centaur.ExampleWebApp/", 9059);
            _host.EnvironmentVariables.Add("PAYLOAD", "injected");
            _host.Start();
        }

        [TearDown]
        public void StopHost()
        {
            _host.Stop();
        }

        [Test]
        public void IisExpressHostedWebAppCanHaveInjectedEnvironmentVariables()
        {
            Assert.That(Get("http://localhost:9059/envvar"), Is.EqualTo("injected"));
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
