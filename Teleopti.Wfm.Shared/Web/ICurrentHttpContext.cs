using System.Web;

namespace Teleopti.Ccc.Infrastructure.Web
{
	public interface ICurrentHttpContext
	{
		HttpContextBase Current();
	}
}