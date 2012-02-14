using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
    public interface ISetThreadCulture
    {
        void SetCulture(IRegional regional);
    }
}