using System.Web;

namespace Centaur.ExampleWebApp
{
    public class ExampleHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("hello!");
            context.Response.End();
        }

        public bool IsReusable { get { return true; } }
    }
}