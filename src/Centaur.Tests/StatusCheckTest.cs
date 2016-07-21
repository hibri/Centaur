using System;
using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Centaur.Tests
{
    [TestFixture]
    public class StatusCheckTest
    {
        private IISExpressHost _host;

        [SetUp]
        public void CreateHost()
        {
            _host = new IISExpressHost(TestContext.CurrentContext.TestDirectory + "../../../../Centaur.ExampleWebApp/", 9058)
            {
                StatusCheckPath = "/status",
                StatusCheckInterval = TimeSpan.FromMilliseconds(100),
                StatusCheckAttempts = 11
            };
        }

        [TearDown]
        public void StopHost()
        {
            try
            {
                _host.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to stop the host: {0} {1}", e.GetType(), e.Message);
            }
        }

        [Test]
        public void CallsStatusCheckRepeatedlyUntilItRespondsWith200()
        {
            _host.Start();
            var finalStatusResponseBody = Get("http://localhost:9058/status");
            var attempts = finalStatusResponseBody.Split(',').Select(ticks => new DateTime(long.Parse(ticks))).ToArray();
            Assert.That(attempts.Count(), Is.EqualTo(10));
            Assert.That((attempts.Last() - attempts.First()).TotalMilliseconds, Is.GreaterThan(800).And.LessThan(1200));
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