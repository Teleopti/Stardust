using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCommonAgentNameProvider : ICommonAgentNameProvider
	{
		public ICommonNameDescriptionSetting CommonAgentNameSettings { get; private set; }

		public FakeCommonAgentNameProvider()
		{
			CommonAgentNameSettings = new CommonNameDescriptionSetting("{FirstName}@{LastName}");
		}

		public FakeCommonAgentNameProvider(string aliasFormat)
		{
			CommonAgentNameSettings = new CommonNameDescriptionSetting(aliasFormat);
		}

		public FakeCommonAgentNameProvider Has(CommonNameDescriptionSetting setting)
	    {
	        CommonAgentNameSettings = setting;
	        return this;
	    }
	}
}
