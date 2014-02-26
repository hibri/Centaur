using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Centaur.ExampleWebApp
{
    public class StatusHandler : IHttpHandler
    {
        private static readonly List<DateTime> WarmUps = new List<DateTime>();

        public void ProcessRequest(HttpContext context)
        {
            if (WarmUps.Count == 10)
            {
                var body = string.Join(",", WarmUps.Select(w => w.Ticks.ToString(CultureInfo.InvariantCulture)).ToArray());
                context.Response.Write(body);
            }
            else
            {
                WarmUps.Add(DateTime.Now);
                context.Response.StatusCode = 500;
                context.Response.Write("warming up!");
            }
        }

        public bool IsReusable { get { return true; } }
    }
}