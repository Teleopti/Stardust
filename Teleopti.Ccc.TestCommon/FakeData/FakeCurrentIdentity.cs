using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentIdentity : ICurrentIdentity
	{
		private readonly string _name;

		public FakeCurrentIdentity(string name)
		{
			_name = name;
		}

		public ITeleoptiIdentity Current()
		{
			return new TeleoptiIdentity(_name,null,null,null);
		}
	}
}