using System;
using System.Globalization;
using System.Reflection;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.TeamSchedule
{
	[Binding]
	public sealed class SeeWholeTeamSchedulesStepDefinitions
	{
		[Given(@"I have page size '(.*)' in personal setting")]
		public void GivenIHavePageSizeInPersonalSetting(int pageSize)
		{
			var pageSizeSetting = new PageSizeConfigurable(pageSize);
			DataMaker.Data().Apply(pageSizeSetting);
		}

		[Then(@"I can see page size picker")]
		public void ThenICanSeePageSizePicker()
		{
			Browser.Interactions.AssertExists(".page-size-selector");
		}

		[Then(@"page size picker is filled with '(.*)'")]
		public void ThenPageSizePickerIsFilledWith(int pageSize)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.paginationOptions.pageSize", 50,
				() =>
				{
					Browser.Interactions.AssertAnyContains(".page-size-selector option[selected]", pageSize.ToString());
				});
		}
	}

	public class PageSizeConfigurable : IUserDataSetup
	{
		private const string agentsPerPageSettingKey = "AgentsPerPage";
		private readonly AgentsPerPageSetting pageSizeSetting;

		public PageSizeConfigurable(int pageSize)
		{
			pageSizeSetting = new AgentsPerPageSetting { AgentsPerPage = pageSize };
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			setAgentsPerPageSettingOwner(pageSizeSetting, person);
			PersonalSettingDataRepository.DONT_USE_CTOR(unitOfWork) .PersistSettingValue(pageSizeSetting);
		}

		private void setAgentsPerPageSettingOwner(AgentsPerPageSetting setting, IPerson user)
		{
			var personDataSetting = new PersonalSettingData(agentsPerPageSettingKey, user);
			var setOwnerMethod = typeof (CalendarLinkSettings)
				.GetMethod("SetOwner", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new[] {typeof (ISettingData)}, null);

			setOwnerMethod.Invoke(setting, new object[] { personDataSetting });
		}
	}
}
