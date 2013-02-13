using System.Web;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.IdentityProvider.Code
{
    public class TransferResult : RedirectResult
    {
        public TransferResult(string url)
            : base(url)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var httpContext = HttpContext.Current;

            httpContext.RewritePath(Url, false);

            IHttpHandler httpHandler = new MvcHttpHandler();
            httpHandler.ProcessRequest(HttpContext.Current);
        }
    }
}