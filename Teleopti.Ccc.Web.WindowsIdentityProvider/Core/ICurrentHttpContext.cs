using System.Web;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public interface ICurrentHttpContext
	{
		HttpContextBase Current();
	}
}