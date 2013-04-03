using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	public interface ICurrentIdentity
	{
		ITeleoptiIdentity Current();
	}
}