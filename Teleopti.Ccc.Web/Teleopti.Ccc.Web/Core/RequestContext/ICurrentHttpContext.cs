using System.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ICurrentHttpContext
	{
		HttpContextBase Current();
	}
}