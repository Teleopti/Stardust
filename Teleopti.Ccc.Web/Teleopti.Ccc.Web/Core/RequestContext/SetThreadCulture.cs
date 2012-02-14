using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
    public class SetThreadCulture : ISetThreadCulture
    {
        public void SetCulture(IRegional regional)
        {
            Thread.CurrentThread.CurrentCulture = regional.Culture;
            Thread.CurrentThread.CurrentUICulture = regional.UICulture;
        }
    }
}