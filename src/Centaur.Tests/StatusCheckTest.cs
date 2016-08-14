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
            IISExpressProcess.ClearAll();
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
            var finalStatusResponseBody = Helpers.Get("http://localhost:9058/status");
            var attempts = finalStatusResponseBody.Split(',').Select(ticks => new DateTime(long.Parse(ticks))).ToArray();
            Assert.That(attempts.Count(), Is.EqualTo(10));
            Assert.That((attempts.Last() - attempts.First()).TotalMilliseconds, Is.GreaterThan(800).And.LessThan(1200));
        }

        [Test]
        public void CallsStatusResponding404SucceedsWhenOptionEnabled()
        {
            _host.StatusCheckPath = "/notaurl";
            _host.StatusCheckAttempts = 5;
            _host.IsNotFoundValid = true;
            _host.Start();

            Assert.That(_host.IsAlive(), Is.True);
        }

        [Test]
        public void CallsStatusResponding404FailsWhenOptionDisabled()
        {
            _host.StatusCheckPath = "/example";
            _host.StatusCheckAttempts = 5;
            _host.IsNotFoundValid = false;
            _host.Start();
            _host.StatusCheckPath = "/notaurl";
            Assert.That(_host.IsAlive(), Is.False);
        }

        [Test]
        public void StatusCheckFailsWhenServerIsNotOnAnd404Enabled()
        {
            _host.StatusCheckAttempts = 1;
            _host.IsNotFoundValid = true;
            Assert.That(_host.IsAlive(), Is.False);
        }

        [Test]
        public void StatusCheckFailsWhenServerIsNotOnAnd404Disabled()
        {
            _host.StatusCheckAttempts = 1;
            _host.IsNotFoundValid = false;
            Assert.That(_host.IsAlive(), Is.False);
        }

        
    }
}