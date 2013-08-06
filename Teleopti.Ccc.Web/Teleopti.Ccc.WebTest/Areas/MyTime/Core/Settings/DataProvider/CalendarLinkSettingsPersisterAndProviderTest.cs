using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.DataProvider
{
	[TestFixture]
	public class CalendarLinkSettingsPersisterAndProviderTest
	{
		private const string calendarLinkKey = "CalendarLinkSettings";

		[Test]
		public void ShouldPersist()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var calendarLinkSettings = new CalendarLinkSettings
				{
					IsActive = true
				};
			var returnedSettings = new CalendarLinkSettings();
			personalSettingDataRepository.Stub(x => x.FindValueByKey(calendarLinkKey, new CalendarLinkSettings())).IgnoreArguments().Return(returnedSettings);
			var target = new CalendarLinkSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.Persist(calendarLinkSettings);
			result.IsActive.Should().Be.EqualTo(calendarLinkSettings.IsActive);
		}

		[Test]
		public void ShouldGet()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new CalendarLinkSettings
				{
					IsActive = true
				};
			personalSettingDataRepository.Stub(x => x.FindValueByKey(calendarLinkKey, new CalendarLinkSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new CalendarLinkSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.Get();
			result.IsActive.Should().Be.EqualTo(returnedSettings.IsActive);
		}

		[Test]
		public void ShouldGetByOwner()
		{
			var person = new Person();
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new CalendarLinkSettings
			{
				IsActive = true
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKeyAndOwnerPerson(calendarLinkKey, person, new CalendarLinkSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new CalendarLinkSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.GetByOwner(person);
			result.IsActive.Should().Be.EqualTo(returnedSettings.IsActive);
		}
	}
}