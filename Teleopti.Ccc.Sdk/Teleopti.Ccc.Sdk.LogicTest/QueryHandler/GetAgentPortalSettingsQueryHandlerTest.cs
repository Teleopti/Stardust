using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAgentPortalSettingsQueryHandlerTest
	{
		[Test]
		public void ShouldGetTheAgentPortalSettings()
		{
			var personalSettingDataRepository = new FakePersonalSettingDataRepository();
			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var target = new GetAgentPortalSettingsQueryHandler(personalSettingDataRepository, unitOfWorkFactory);

			var settings = new AgentPortalSettings {Resolution = 15};

			personalSettingDataRepository.PersistSettingValue("AgentPortalSettings", settings);
		
			var result = target.Handle(new GetAgentPortalSettingsQueryDto());
			result.First().Resolution.Should().Be.EqualTo(15);
		}
	}
}
