using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class FetchDayOffSettingsModelTest
	{
		public IFetchDayOffSettingsModel Target;
		public FakeDayOffSettingsRepository DayOffSettingsRepository;

		[Test]
		public void FetchAllShouldIncludeDefaultSettings()
		{
			var defaultSettings = Target.FetchAll().DayOffSettingModel.Single();

			defaultSettings.MinDayOffsPerWeek.Should().Be.EqualTo(1);
			defaultSettings.MaxDayOffsPerWeek.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveDayOffs.Should().Be.EqualTo(1);
			defaultSettings.MaxConsecutiveDayOffs.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveWorkdays.Should().Be.EqualTo(2);
			defaultSettings.MaxConsecutiveWorkdays.Should().Be.EqualTo(6);
			defaultSettings.Id.Should().Be.EqualTo(DayOffSettingsRepository.LoadAll().Single().Id.Value);
			defaultSettings.Default.Should().Be.True();
		}
	}
}