using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture]
	class AgentsPerPagePersisterAndProviderTest
	{
		private const string agentsPerPageSettingKey = "AgentsPerPage";

		[Test]
		public void ShouldPersist()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var agentsPerPageSetting = new AgentsPerPageSetting
			{
				AgentsPerPage = 20
			};
			var returnedSettings = new AgentsPerPageSetting();
			personalSettingDataRepository.Stub(x => x.FindValueByKey(agentsPerPageSettingKey, new AgentsPerPageSetting())).IgnoreArguments().Return(returnedSettings);
			var target = new AgentsPerPageSettingPersisterAndProvider(personalSettingDataRepository);
			var result = target.Persist(agentsPerPageSetting);
			result.AgentsPerPage.Should().Be.EqualTo(agentsPerPageSetting.AgentsPerPage);
		}

		[Test]
		public void ShouldGet()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new AgentsPerPageSetting
			{
				AgentsPerPage = 20
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKey(agentsPerPageSettingKey, new AgentsPerPageSetting())).IgnoreArguments().Return(returnedSettings);

			var target = new AgentsPerPageSettingPersisterAndProvider(personalSettingDataRepository);
			var result = target.Get();
			result.AgentsPerPage.Should().Be.EqualTo(returnedSettings.AgentsPerPage);
		}

		[Test]
		public void ShouldGetByOwner()
		{
			var person = new Person();
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new AgentsPerPageSetting
			{
				AgentsPerPage = 20
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKeyAndOwnerPerson(agentsPerPageSettingKey, person, new AgentsPerPageSetting())).IgnoreArguments().Return(returnedSettings);

			var target = new AgentsPerPageSettingPersisterAndProvider(personalSettingDataRepository);
			var result = target.GetByOwner(person);
			result.AgentsPerPage.Should().Be.EqualTo(returnedSettings.AgentsPerPage);
		}
	}
}
