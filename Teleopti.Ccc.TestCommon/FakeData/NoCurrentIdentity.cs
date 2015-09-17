using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class NoCurrentIdentity : ICurrentIdentity
	{
		public ITeleoptiIdentity Current()
		{
			return null;
		}
	}
}