using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCommonAgentNameProvider : ICommonAgentNameProvider
	{
		public ICommonNameDescriptionSetting CommonAgentNameSettings { get; private set; }

		public FakeCommonAgentNameProvider()
		{
			CommonAgentNameSettings = new CommonNameDescriptionSetting("{FirstName}@{LastName}");
		}
	}
}
