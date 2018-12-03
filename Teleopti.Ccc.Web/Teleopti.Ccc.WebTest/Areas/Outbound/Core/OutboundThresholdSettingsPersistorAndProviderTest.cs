using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	public class OutboundThresholdSettingsPersistorAndProviderTest
	{
		private const string outboundThresholdSettingKey = "OutboundThresholdSettings";

		[Test]
		public void ShouldPersist()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var thresholdSettings = new OutboundThresholdSettings
			{
				RelativeWarningThreshold = new Percent(0.5),
				WarningThresholdType = WarningThresholdType.Relative
			};
			var returnedSettings = new OutboundThresholdSettings();
			personalSettingDataRepository.Stub(x => x.FindValueByKey(outboundThresholdSettingKey, new OutboundThresholdSettings())).IgnoreArguments().Return(returnedSettings);
			var target = new OutboundThresholdSettingsPersistorAndProvider(personalSettingDataRepository);
			var result = target.Persist(thresholdSettings);
			result.RelativeWarningThreshold.Should().Be.EqualTo(thresholdSettings.RelativeWarningThreshold);
			result.WarningThresholdType.Should().Be.EqualTo(WarningThresholdType.Relative);
		}

		[Test]
		public void ShouldGet()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new OutboundThresholdSettings
			{
				RelativeWarningThreshold = new Percent(1)
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKey(outboundThresholdSettingKey, new OutboundThresholdSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new OutboundThresholdSettingsPersistorAndProvider(personalSettingDataRepository);
			var result = target.Get();
			result.RelativeWarningThreshold.Should().Be.EqualTo(returnedSettings.RelativeWarningThreshold);
		}

		[Test]
		public void ShouldGetByOwner()
		{
			var person = new Person();
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var returnedSettings = new OutboundThresholdSettings
			{
				RelativeWarningThreshold = new Percent(1)
			};
			personalSettingDataRepository.Stub(x => x.FindValueByKeyAndOwnerPerson(outboundThresholdSettingKey, person, new OutboundThresholdSettings())).IgnoreArguments().Return(returnedSettings);

			var target = new OutboundThresholdSettingsPersistorAndProvider(personalSettingDataRepository);
			var result = target.GetByOwner(person);
			result.RelativeWarningThreshold.Should().Be.EqualTo(returnedSettings.RelativeWarningThreshold);
		}
	}
}
