using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class DayOffSettingsRepositoryTest : RepositoryTest<DayOffSettings>
	{
		protected override DayOffSettings CreateAggregateWithCorrectBusinessUnit()
		{
			return new DayOffSettings
			{
				ConsecutiveDayOffs = new MinMax<int>(2,6),
				DayOffsPerWeek = new MinMax<int>(4,6),
				ConsecutiveWorkdays = new MinMax<int>(5,7)
			};
		}

		protected override void VerifyAggregateGraphProperties(DayOffSettings loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.DayOffsPerWeek.Should().Be.EqualTo(expected.DayOffsPerWeek);
			loadedAggregateFromDatabase.ConsecutiveDayOffs.Should().Be.EqualTo(expected.ConsecutiveDayOffs);
			loadedAggregateFromDatabase.ConsecutiveWorkdays.Should().Be.EqualTo(expected.ConsecutiveWorkdays);
		}

		protected override Repository<DayOffSettings> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new DayOffSettingsRepository(currentUnitOfWork);
		}

		[Test]
		public void DefaultSettingShouldBeIncluded()
		{
			var defaultSettings = new DayOffSettingsRepository(CurrUnitOfWork).LoadAll().Single();
			defaultSettings.Default.Should().Be.True();
			defaultSettings.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(1, 3));
			defaultSettings.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(1, 3));
			defaultSettings.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(2, 6));
		}

		[Test]
		public void CanNotAddAnotherDefaultSettings()
		{
			var setting = new DayOffSettings
			{
				ConsecutiveDayOffs = new MinMax<int>(1, 2),
				ConsecutiveWorkdays = new MinMax<int>(1, 2),
				DayOffsPerWeek = new MinMax<int>(2, 3),
			}.MakeDefault_UseOnlyFromTest();
			Assert.Throws<ArgumentException>(() =>
				new DayOffSettingsRepository(CurrUnitOfWork).Add(setting));
		}

		[Test]
		public void CanNotRemoveDefaultSetting()
		{
			var rep = new DayOffSettingsRepository(CurrUnitOfWork);
			var defaultSetting = rep.LoadAll().Single(x => x.Default);
			Assert.Throws<ArgumentException>(() => rep.Remove(defaultSetting));
		}
	}
}