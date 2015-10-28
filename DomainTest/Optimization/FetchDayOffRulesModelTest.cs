using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class FetchDayOffRulesModelTest
	{
		public IFetchDayOffRulesModel Target;
		public FakeDayOffRulesRepository DayOffRulesRepository;

		[Test]
		public void ShouldFetchDefaultSettingsIfNotExists()
		{
			var defaultSettings = Target.FetchDefaultRules();
			defaultSettings.MinDayOffsPerWeek.Should().Be.EqualTo(1);
			defaultSettings.MaxDayOffsPerWeek.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveDayOffs.Should().Be.EqualTo(1);
			defaultSettings.MaxConsecutiveDayOffs.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveWorkdays.Should().Be.EqualTo(2);
			defaultSettings.MaxConsecutiveWorkdays.Should().Be.EqualTo(6);
			defaultSettings.Id.Should().Be.EqualTo(Guid.Empty);
			defaultSettings.Default.Should().Be.True();
		}

		[Test]
		public void ShouldFetchCurrentDefaultSettings()
		{
			var curr = new DayOffRules
			{
				ConsecutiveDayOffs = new MinMax<int>(1, 2),
				ConsecutiveWorkdays = new MinMax<int>(2, 3),
				DayOffsPerWeek = new MinMax<int>(3, 4)
			}.MakeDefault_UseOnlyFromTest();
			curr.SetId(Guid.NewGuid());
			DayOffRulesRepository.Add(curr);

			var defaultSettings = Target.FetchDefaultRules();
			defaultSettings.MinDayOffsPerWeek.Should().Be.EqualTo(3);
			defaultSettings.MaxDayOffsPerWeek.Should().Be.EqualTo(4);
			defaultSettings.MinConsecutiveDayOffs.Should().Be.EqualTo(1);
			defaultSettings.MaxConsecutiveDayOffs.Should().Be.EqualTo(2);
			defaultSettings.MinConsecutiveWorkdays.Should().Be.EqualTo(2);
			defaultSettings.MaxConsecutiveWorkdays.Should().Be.EqualTo(3);
			defaultSettings.Id.Should().Be.EqualTo(curr.Id);
			defaultSettings.Default.Should().Be.True();
		}
	}
}