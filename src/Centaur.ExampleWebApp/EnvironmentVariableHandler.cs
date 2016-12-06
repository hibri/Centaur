using System;
using System.Web;

namespace Centaur.ExampleWebApp
{
    public class EnvironmentVariableHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write(Environment.GetEnvironmentVariable("PAYLOAD"));
            context.Response.End();
        }

        public bool IsReusable { get { return true; } }
    }
}