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
	public class NameFormatSettingsPersisterAndProviderTest
	{
		private const string nameFormatKey = "NameFormatSettings";

		[Test]
		public void ShouldPersist()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var nameFormatSettings = new NameFormatSettings
				{
					NameFormatId = 1
				};
			var returnedSettings = new NameFormatSettings();
			personalSettingDataRepository.Stub(x => x.FindValueByKey(nameFormatKey, new NameFormatSettings())).IgnoreArguments().Return(returnedSettings);
			var target = new NameFormatSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.Persist(nameFormatSettings);
			result.NameFormatId.Should().Be.EqualTo(nameFormatSettings.NameFormatId);
		}

		[Test]
		public void ShouldGet()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new NameFormatSettings
				{
					NameFormatId = 1
				};
			personalSettingDataRepository.Stub(x => x.FindValueByKey(nameFormatKey, new NameFormatSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new NameFormatSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.Get();
			result.NameFormatId.Should().Be.EqualTo(returnedSettings.NameFormatId);
		}

		[Test]
		public void ShouldGetByOwner()
		{
			var person = new Person();
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new NameFormatSettings
			{
				NameFormatId = 1
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKeyAndOwnerPerson(nameFormatKey, person, new NameFormatSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new NameFormatSettingsPersisterAndProvider(personalSettingDataRepository);
			var result = target.GetByOwner(person);
			result.NameFormatId.Should().Be.EqualTo(returnedSettings.NameFormatId);
		}
	}
}